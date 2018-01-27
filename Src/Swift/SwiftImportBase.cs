using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biz.Swift
{
    public abstract class SwiftImportBase
    {
        //In
        public string OriginalBodyText { set; get; }
        List<IMetaSwiftLine> metaSwiftLines { set; get; }

        //Out
        public MessageSourceFormat messageSourceFormat = MessageSourceFormat.Unknown;
        public List<SwiftImportDataLine> MessageDataLines { get; set; }
        public string MessageType { set; get; }
        public string EnverlopType { set; get; }
        public string DlcRef { set; get; }
        public string ReceiverBIC { set; get; }
        public string SenderBIC { set; get; }

        public SwiftImportBase(string fileText, List<IMetaSwiftLine> metaSwiftLines, MessageSourceFormat messageSourceFormat)
        {
            this.OriginalBodyText = fileText;
            this.metaSwiftLines = metaSwiftLines;
            this.messageSourceFormat = messageSourceFormat;
        }

        //
        //Process Message
        //
        public void ProcessMessage()
        {
            ProcessMessageBody();
        }

        //Get Sender ref (Override)
        protected abstract string GetSender(string messageBody);

        //Get Reciver ref (Override)
        protected abstract string GetReciver(string messageBody);

        //Strip off unwanted secsions (Override)
        protected abstract string ClearMessageBody(string messageBody);

        //Find Message Type
        protected abstract string FindMessageType(string messageBody);

        //Get Body Regexp
        protected abstract string GetBodyRegExp();

        //Get Sum Message Regexp
        protected abstract string GetSubRegExp();

        //Some formats have title in the first line
        protected abstract bool IgnoreBodyFirstLine();

        //Some formats have title in the first line
        protected abstract bool IgnoreSubFirstLine();


        protected virtual void ProcessMessageBody()
        {
            MessageDataLines = new List<SwiftImportDataLine>();
            bool hasTradeBody = false;
            MessageType = FindMessageType(OriginalBodyText);
            SenderBIC = GetSender(OriginalBodyText);
            ReceiverBIC = GetReciver(OriginalBodyText);

            //Strip off unwanted secsions (Override)
            string messageBody = ClearMessageBody(OriginalBodyText);

            //RegEx for body
            string regExp = GetBodyRegExp();

            //Check for Trade Body
            if ("798".Equals(MessageType) || "998".Equals(MessageType))
            {
                hasTradeBody = true;
                EnverlopType = MessageType;
           }

            //Process message Lines
            ProcessMessageBodyLines(messageBody, hasTradeBody);

            //Add Message Type at the end because of both Env and msg type need now
            if (hasTradeBody)
            {
                MessageDataLines.Add(AddDataLine("MT", EnverlopType+"/"+MessageType, false, false));
            }
            else
            {
                MessageDataLines.Add(AddDataLine("MT", MessageType, false, false));
            }
        }

        private void ProcessMessageBodyLines(string messageBody, bool hasTradeEnvirlop)
        {
            string regExp = hasTradeEnvirlop ? GetSubRegExp() : GetBodyRegExp();
            MatchCollection resultMatch = Regex.Matches(messageBody, regExp);
            int[] posArray = Regex.Matches(messageBody, regExp).Cast<Match>().Select(m => m.Index).ToArray();
            List<string> textArray = Split(messageBody, posArray);

            int pos = 0;
            if (resultMatch != null)
            {
                foreach (Match m in resultMatch)
                {
                    string code = m.ToString();
                    //Actual Code
                    int codeOffset = 0;
                    int length = code.Length - 1;
                    Match matchOffset = Regex.Match(code, "[0-9]"); // Find firt number pos
                    if (matchOffset.Success)
                    {
                        codeOffset = matchOffset.Index;
                    }
                    if (code.EndsWith(":"))
                    {
                        length = code.Length - (codeOffset + 1);
                    }
                    string actualCode = code.Substring(codeOffset, length);
                    //Message has Envirlop
                    if (actualCode.Equals("77E"))
                    {
                        //Take the string from this and send to process again
                        string string77E = messageBody.Substring(posArray[pos]);
                        //Substring 4 to skip 77E
                        string77E = string77E.Substring(3 + codeOffset);
                        ProcessMessageBodyLines(string77E, true);
                        return;
                    }

                    SwiftImportDataLine line = AddDataLine(actualCode, textArray[pos], true, hasTradeEnvirlop);
                    if (line!=null)
                    {
                        MessageDataLines.Add(line);
                    }
                    pos++;
                }
            }

        }


        private SwiftImportDataLine AddDataLine(string code, string textBlock, bool scan, bool subMessage)
        {

            IMetaSwiftLine swiftLine = metaSwiftLines.Where(a => a.Code.Equals(code)).FirstOrDefault();
            if (swiftLine != null)
            {
                string text = ExtractValue(textBlock, scan, subMessage);

                SwiftImportDataLine dataLine = new SwiftImportDataLine();
                dataLine.Code = code;
                dataLine.MetaLineID = swiftLine.ID;
                dataLine.Ordinal = swiftLine.Ordinal;
                dataLine.Value = text;

                //Fetch DLC Ref is it's not yet fetched
                if (DlcRef == null && ("20".Equals(dataLine.Code) || "21".Equals(dataLine.Code)) && DlcRef == null)
                {
                    DlcRef = text;
                }

                return dataLine;
            }
            else if ("12".Equals(code))
            {
                string text = ExtractValue(textBlock, scan, subMessage);
                //Fetch real message type if this has enverlop
                if (EnverlopType != null)
                {
                    MessageType = text;
                }
                return null;
            }
            else
            {
                throw new Exception("Swift code '" + code + "' does not exist in META!");
            }
        }

        private string ExtractValue(string textBlock, bool scan, bool subMessage)
        {
            string[] textLines = textBlock.Split('\n');
            string text = textBlock;
            if (textLines.Length > 0)
            {
                if (scan)
                {
                    text = "";
                    int lineNo = 0;
                    //Skip Line one?
                    int firstLine = 0;
                    if (subMessage)
                    {
                        firstLine = IgnoreSubFirstLine() ? 1 : 0;
                    }
                    else
                    {
                        firstLine = IgnoreBodyFirstLine() ? 1 : 0;
                    }
                    for (int i = firstLine; i < textLines.Count(); i++)
                    {
                        string textLine = textLines[i];

                        if (textLine != null && textLine.Trim().Length > 0)
                        {
                            if (lineNo > 0)
                            {
                                text += Environment.NewLine;
                            }
                            string s = textLine.Trim();
                            if (firstLine == 0) // Clear first line prefix
                            {
                                s = s.Substring(s.LastIndexOf(':') + 1);
                            }
                            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s{2,}", " ");
                            text += s;
                        }
                        //}
                        lineNo++;
                    }
                }
            }
            return text;
        }


        private List<string> Split(string source, params int[] sizes)
        {
            List<string> result = new List<string>();

            var index = 0;
            foreach (int pos in sizes)
            {
                if (index >= sizes.Length - 1)
                {
                    result.Add(source.Substring(pos));
                }
                else
                {
                    int start = pos;
                    int end = sizes[index + 1] - start;
                    result.Add(source.Substring(start, end));
                }
                index++;
            }

            return result;
        }
    }
}
