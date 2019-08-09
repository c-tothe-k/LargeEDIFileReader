using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LargeEDIFileReader
{
    public static class FileUtils
    {
        public static int CurrentPageNumber { get; set; } = 0;

        private static EDIFileStream FileReader { get; set; }

        private static readonly int PageSize = 10000;


       

        public static bool OpenEDIFile(EDIFileStream EdiFileStream)
        {

            FileReader = EdiFileStream;
            //Read the first 106 bytes, and validate that it at least looks like a 5010 X12 EDI file
             string Envelope = String.Empty;

              try
                {
                   Envelope = FileReader.ReadEnvelope();
                   //Check for X12 Control Segment Name
                   if (String.IsNullOrEmpty(Envelope) || Envelope.Length != 106 || Envelope.Substring(0, 3) != "ISA")
                       throw new ArgumentException("File is not an X12 EDI file or file is missing control segment.");
                                
                   FileReader.LoadSegmentOffset();         
                   return true;
                }
                catch (ArgumentException e)
                {
                    return false;
                }
            
        }

        public static string LoadNextPage()
        {
            CurrentPageNumber++;
            string page = FileReader.ReadPage(CurrentPageNumber);         
            return page;
        }
       
        public static void Close()
        {
            FileReader.Dispose();
        }
    }
}
