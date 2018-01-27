using iTextSharp.text.pdf;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Swift
{

    public enum MessageSourceFormat
    {
        [Display(Name = "Unknown")]
        Unknown,
        [Display(Name = "Management")]
        MessageManagement,
        [Display(Name = "CR")]
        CR,
        [Display(Name = "PDF")]
        Pdf,
        [Display(Name = "Text")]
        Text,
    }

    public class SwiftImportFactory
    {
        public static SwiftImportBase GetHandler(string fileName, List<IMetaSwiftLine> metaSwiftLines)
        {
            //Read file
            string extension = System.IO.Path.GetExtension(fileName);
            string extractedText = null;
            if (extension != null)
            {
                if (".pdf".Equals(extension.ToLower()))
                {
                    extractedText = pdfText(fileName);
                }
                else if (".txt".Equals(extension.ToLower()))
                {
                    extractedText = textText(fileName);
                }
                else
                {
                    throw new Exception("Unknown file type '" + extension + "'");
                }
            }
            else
            {
                throw new Exception("Unknown file type with empty extension");
            }
        }

        private static MessageSourceFormat CaptureSourceRules(string extractedText, string extension)
        {
            //Identify Message source RULES
            if (extractedText.IndexOf("Alt") > 0)
            {
                return MessageSourceFormat.AllianceMessageManagement;
            }
            else if (extension.ToLower().Equals(".txt"))
            {
                return MessageSourceFormat.Text;
            }
            else
            {
                return MessageSourceFormat.Unknown;
            }
        }

        private static string pdfText(string fileName)
        {
            var reader = new PdfReader(fileName);

            StringBuilder sb = new StringBuilder();

            try
            {
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    var cpage = reader.GetPageN(page);
                    var content = cpage.Get(PdfName.CONTENTS);

                    if (content.IsArray())
                    {
                        foreach (PdfObject line in ((PdfArray)content).ArrayList)
                        {
                            ExtractLines(reader, sb, line);
                        }
                    }
                    else
                    {
                        ExtractLines(reader, sb, content);
                    }

                }
            }
            catch (Exception ee)
            {
                throw ee;
            }
            finally
            {
                reader.Close();
            }

            return sb.ToString();
        }

        private static void ExtractLines(PdfReader reader, StringBuilder sb, PdfObject content)
        {
            var ir = (PRIndirectReference)content;

            var value = reader.GetPdfObject(ir.Number);

            if (value.IsStream())
            {
                PRStream stream = (PRStream)value;

                var streamBytes = PdfReader.GetStreamBytes(stream);

                var tokenizer = new PRTokeniser(new RandomAccessFileOrArray(streamBytes));

                try
                {
                    while (tokenizer.NextToken())
                    {
                        if (tokenizer.TokenType == PRTokeniser.TK_STRING)
                        {
                            string str = tokenizer.StringValue;
                            sb.Append(str);
                        }
                    }
                }
                finally
                {
                    tokenizer.Close();
                }
            }
        }

        private static string textText(string path)
        {
            string result;
            using (StreamReader streamReader = new StreamReader(path, UTF8Encoding.ASCII))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }
    }
}
