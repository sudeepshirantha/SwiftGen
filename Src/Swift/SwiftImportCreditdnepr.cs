using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biz.Swift
{
    public class SwiftImportCreditdnepr: SwiftImportBase
    {
        public SwiftImportCreditdnepr(string fileText, List<IMetaSwiftLine> metaSwiftLines)
            : base(fileText, metaSwiftLines, MessageSourceFormat.Creditdnepr)
        {

        }

        //Strip off unwanted secsions (Override)
        protected override string ClearMessageBody(string messageBody)
        {
            //Trim the messsage body by end MESSAGE HISTORY
            string tailerText = "Message Trailer";
            if (messageBody.IndexOf(tailerText) > 0)
            {
                messageBody = messageBody.Substring(0, messageBody.IndexOf(tailerText));
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
                throw new Exception("Cannot find message type 'FIN NNN' in the message body of type '"+ messageSourceFormat.ToString()+"'");
            }
        }


        //Get Sender ref (Override)
        protected override string GetSender(string messageBody)
        {
            MatchCollection resultMatchMT = Regex.Matches(messageBody, @"Sender[*]");
            if (resultMatchMT != null && resultMatchMT.Count > 0)
            {
                return resultMatchMT[0].Value.Substring(4);
            }
            return null;
        }

        //Get Reciver ref (Override)
        protected override string GetReciver(string messageBody)
        {
            MatchCollection resultMatchMT = Regex.Matches(messageBody, @"Receiver[*]");
            if (resultMatchMT != null && resultMatchMT.Count > 0)
            {
                return resultMatchMT[0].Value.Substring(4);
            }
            return null;
        }

        //Get Body Regexp
        protected override string GetBodyRegExp()
        {
            return @"[0-9][0-9][A-Z]?:";
        }

        //Get Sum Message Regexp
        protected override string GetSubRegExp()
        {
            return @"[0-9][0-9][A-Z]?:";
        }


        //Some formats have title in the first line
        protected override bool IgnoreBodyFirstLine()
        {
            return true;
        }

        //Some formats have title in the first line
        protected override bool IgnoreSubFirstLine()
        {
            return false;
        }

    }
}
