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
        public static int CurrentPageStart { get; set; } = 0;

        private static EDIFileStream FileReader { get; set; }

        private static readonly int PageSize = 1000;

        private static readonly int EnvelopeSize = 106;

        private static char SegmentDelimter { get; set; }

        private static char ElementDelimeter { get; set; }

        public static bool OpenEDIFile(EDIFileStream EdiFileStream)
        {

            FileReader = EdiFileStream;
            //Read the first 106 bytes, and validate that it at least looks like a 5010 X12 EDI file
                string Envelope = String.Empty;

                try
                {
                    Envelope = FileReader.Read( 0, EnvelopeSize);

                    ElementDelimeter  = Envelope[103];

                    SegmentDelimter = Envelope[105];//TODO account for trailing newlines

                    //Check for X12 Control Segment Name
                    if (Envelope.Substring(0,3) != "ISA")
                        throw new ArgumentException("File is not an X12 EDI file or file is missing control segment");


                    return true;
                }
                catch (ArgumentException e)
                {
                    return false;
                }
            
        }

        public static string LoadPage()
        {
          string page =  FileReader.ReadPage(CurrentPageStart, PageSize, SegmentDelimter);
          CurrentPageStart = CurrentPageStart + PageSize;
          return page;
        }
       
        public static void Close()
        {
            FileReader.Dispose();
        }
    }
}
