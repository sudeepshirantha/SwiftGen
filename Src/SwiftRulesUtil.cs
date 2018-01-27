using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWIFTFormatter
{
    public class SwiftRulesUtil
    {
        //firstRowOffset - means add extra prefix to it before validate specially for 77C
        public static string ApplySwitRule(string displayCode, string value, int firstRowOffset, int RowLength, int MaxRows, int FixLength, bool htmlFormat, ref string swiftError, bool DEBUG)
        {
            if (value != null && (RowLength > 0 || MaxRows > 0 || FixLength > 0))
            {
                if (!htmlFormat)
                {
                    StringBuilder newSwiftLine = new StringBuilder();
                    int rows = 0;
                    bool noOfRowsExceeded = false;

                    //First split individual line alerady we have
                    string[] existingLines = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    int linePos = 0;
                    bool newLineAdded = false;
                    foreach (string existingLine in existingLines)
                    {
                        newLineAdded = false;
                        string[] words = existingLine.Split(' ');
                        linePos++;
                        string line = "";

                        //Row has a limit
                        if (RowLength > 0)
                        {
                            int wordPos = 0;
                            foreach (string word in words)
                            {
                                wordPos++;
                                if ((rows == 0 && firstRowOffset > 0 && ((line + word).Length + firstRowOffset) > RowLength) || (line + word).Length > RowLength)
                                {
                                    newSwiftLine.Append(line.Trim());
                                    if (DEBUG)
                                    {
                                        newSwiftLine.Append("[CUT AT" + line.Length + "]");
                                    }
                                    newSwiftLine.Append(Environment.NewLine);
                                    line = "";
                                    rows++;
                                    newLineAdded = true;
                                }

                                line += string.Format("{0} ", word.Trim());

                                //Last Item add it
                                if (wordPos == words.Length)
                                {
                                    if (line.Length > 0)
                                    {
                                        newSwiftLine.Append(line.Trim());
                                        line = "";
                                        rows++;
                                        newLineAdded = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            line = existingLine;
                            rows++;
                        }

                        //Further lines exist so add new line if it's not added in last cycle
                        if (linePos < existingLines.Length && !newLineAdded)
                        {
                            newSwiftLine.Append(Environment.NewLine);
                            newLineAdded = true;
                        }
                        if (line.Length > 0)
                        {
                            newSwiftLine.Append(line.Trim());
                        }

                        //Raise exception if no of rows exeeded
                        if (MaxRows > 0 && rows > MaxRows)
                        {
                            if (swiftError.Length > 0)
                            {
                                swiftError += Environment.NewLine;
                            }
                            noOfRowsExceeded = true;
                        }
                    }

                    if (noOfRowsExceeded)
                    {
                        swiftError += "Swift Code " + displayCode + " exceeds the maximum no of characters [" + RowLength + "x" + MaxRows + "] for the SWIFT message!";
                    }

                    //Raise exception if fix length not meet.
                    if (FixLength > 0 && newSwiftLine.ToString().Length != FixLength)
                    {
                        if (swiftError.Length > 0)
                        {
                            swiftError += Environment.NewLine;
                        }
                        swiftError += "Swift Code " + displayCode + " exceeds the maximum length " + FixLength + " for the SWIFT message!";
                    }

                    return newSwiftLine.ToString();
                }
            }
            return value;
        }

    }
}
