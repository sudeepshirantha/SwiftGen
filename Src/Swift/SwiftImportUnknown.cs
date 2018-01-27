using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biz.Swift
{
    public class SwiftImportUnknown: SwiftImportBase
    {
        public SwiftImportUnknown(string fileText, List<IMetaSwiftLine> metaSwiftLines)
            : base(fileText, metaSwiftLines, MessageSourceFormat.Unknown)
        {

        }

        //Strip off unwanted secsions (Override)
        protected override string ClearMessageBody(string messageBody)
        {
            //Trim the messsage body by end MESSAGE HISTORY
            string tailerText = "­­­­­­­Message Trailer";
            if (messageBody.IndexOf(tailerText) > 0)
            {
                messageBody = messageBody.Substring(0, messageBody.IndexOf(tailerText));
            }

            //Trim Message by Start Block
            string headderText = "­­­­­­­­­­­­­­­­­­­­­­­­­­­ Message Text ­­­­­­­­­­­­­­­­­­­­­­­­­­­";
            if (messageBody.IndexOf(headderText) > 0)
            {
                messageBody = messageBody.Substring(messageBody.IndexOf(headderText));
            }
            
            return messageBody;
        }

        //Find Message Type
        protected override string FindMessageType(string messageBody)
        {
            MatchCollection resultMatchMT = Regex.Matches(messageBody, @"FIN [0-9][0-9][0-9]");
            if (resultMatchMT != null && resultMatchMT.Count > 0)
            {
                return resultMatchMT[0].Value.Substring(4);
            }
            else
            {
                return "NNN";
            }
            
        }

        //Get Body Regexp
        protected override string GetBodyRegExp()
        {
            return @":[0-9][0-9][A-Z]?:";
        }

        //Get Sum Message Regexp
        protected override string GetSubRegExp()
        {
            return @":[0-9][0-9][A-Z]?:";
        }

        //Get Sender ref (Override)
        protected override string GetSender(string messageBody)
        {
            MatchCollection resultMatchMT = Regex.Matches(messageBody, @"Sender\s*:\s*[A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9]");
            if (resultMatchMT != null && resultMatchMT.Count > 0)
            {
                string value = resultMatchMT[0].Value;
                return value.Substring(value.IndexOf(":") + 1);

            }
            return null;
        }

        //Get Reciver ref (Override)
        protected override string GetReciver(string messageBody)
        {
            MatchCollection resultMatchMT = Regex.Matches(messageBody, @"Receiver\s*:\s*[A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9][A-Z,0-9]");
            if (resultMatchMT != null && resultMatchMT.Count > 0)
            {
                string value = resultMatchMT[0].Value;
                return value.Substring(value.IndexOf(":") + 1);

            }
            return null;
        }

        //Some formats have title in the first line
        protected override bool IgnoreBodyFirstLine()
        {
            return false;
        }

        //Some formats have title in the first line
        protected override bool IgnoreSubFirstLine()
        {
            return false;
        }


    }
}
