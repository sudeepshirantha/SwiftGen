using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Swift
{
    public class SwiftImportDataLine
    {
        public SwiftImportDataLine()
        {
        }

        public int MetaLineID { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public int Ordinal { get; set; }
    }
}
