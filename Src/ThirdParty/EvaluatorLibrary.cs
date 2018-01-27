using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Biz.ThirdParty
{
    public interface IExpressionModel
    {
        string ExpressionValidate(string strUserEntry);
        string ConvertToPostfix(string strValidExpression);
        string EvaluateExpression(string strPostFixExpression);
    }

    public class ExpressionModel : IExpressionModel
    {


        public string ExpressionValidate(string strUserEntry)
        {
            strUserEntry = strUserEntry.Trim();

            if (string.IsNullOrEmpty(strUserEntry))
                return "There was no entry.";

            if (strUserEntry.Length > 250) //250 seemed better than 254
                return "More than 250 characters entered.";

            string[] fixes = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            bool boolStartsWith = fixes.Any(prefix => strUserEntry.StartsWith(prefix));
            if (!boolStartsWith)
                return "The expression needs to start with a number.";

            bool boolEndsWith = fixes.Any(postfix => strUserEntry.EndsWith(postfix));
            if (!boolEndsWith)
                return "The expression needs to end with a number.";

            if (!Regex.IsMatch(strUserEntry, "^[-0-9+*./ ]+$"))
                return "There were characters other than Numbers, +, -, * and /.";

            if (!Regex.IsMatch(strUserEntry, "[-+*/]"))
                return "Not a mathematical expression";

            string[] strOperator = Regex.Split(strUserEntry, @"\d+");
            for (int i = 1; i < strOperator.Length - 1; i++) //the first and last elements of the array are empty
            {
                if (strOperator[i].Trim().Length > 1)
                    return "Expression cannot have operators together '" + strOperator[i] + "'.";
            }

            return "Valid";
        }



        public string ConvertToPostfix(string strValidExpression)
        {

            StringBuilder sbPostFix = new StringBuilder();
            Stack<Char> stkTemp = new Stack<char>();

            for (int i = 0; i < strValidExpression.Length; i++)
            {
                char chExp = strValidExpression[i];

                if (chExp == '+' || chExp == '-' || chExp == '*' || chExp == '/')
                {
                    sbPostFix.Append(" ");

                    if (stkTemp.Count <= 0)
                        stkTemp.Push(chExp);
                    else if (stkTemp.Peek() == '*' || stkTemp.Peek() == '/')
                    {
                        sbPostFix.Append(stkTemp.Pop()).Append(" ");
                        i--;
                    }
                    else if (chExp == '+' || chExp == '-')
                    {
                        sbPostFix.Append(stkTemp.Pop()).Append(" ");
                        stkTemp.Push(chExp);
                    }
                    else
                    {
                        stkTemp.Push(chExp);
                    }
                }
                else
                {
                    sbPostFix.Append(chExp);
                }
            }

            for (int j = 0; j <= stkTemp.Count; j++)
            {
                sbPostFix.Append(" ").Append(stkTemp.Pop());
            }

            string strPostFix = sbPostFix.ToString();
            strPostFix = Regex.Replace(strPostFix, @"[ ]{2,}", @" ");
            return strPostFix;
        }

        public string EvaluateExpression(string strPostFixExpression)
        {
            Stack<string> stkTemp = new Stack<string>();
            string strOpr = "";
            string strNumLeft = "";
            string strNumRight = "";

            List<String> lstPostFix = strPostFixExpression.Split(' ').ToList();

            for (int i = 0; i < lstPostFix.Count; i++)
            {
                stkTemp.Push(lstPostFix[i]);
                if (stkTemp.Count >= 3)
                {
                    Func<string, bool> myFunc = (c => c == "+" || c == "-" || c == "*" || c == "/");
                    bool isOperator = myFunc(stkTemp.Peek());

                    if (isOperator)
                    {
                        strOpr = stkTemp.Pop();
                        strNumRight = stkTemp.Pop();
                        strNumLeft = stkTemp.Pop();

                        double dblNumLeft, dblNumRight;
                        bool isNumLeft = double.TryParse(strNumLeft, out dblNumLeft);
                        bool isNumRight = double.TryParse(strNumRight, out dblNumRight);

                        if (isNumLeft && isNumRight)
                        {
                            double dblTempResult;
                            switch (strOpr)
                            {
                                case ("+"):
                                    dblTempResult = dblNumLeft + dblNumRight;
                                    stkTemp.Push(dblTempResult.ToString());
                                    break;
                                case ("-"):
                                    dblTempResult = dblNumLeft - dblNumRight;
                                    stkTemp.Push(dblTempResult.ToString());
                                    break;
                                case ("*"):
                                    dblTempResult = dblNumLeft * dblNumRight;
                                    stkTemp.Push(dblTempResult.ToString());
                                    break;
                                case ("/"):
                                    dblTempResult = dblNumLeft / dblNumRight;
                                    stkTemp.Push(dblTempResult.ToString());
                                    break;
                            }
                        }
                    }
                }
            }

            return stkTemp.Pop();
        }
    }
}