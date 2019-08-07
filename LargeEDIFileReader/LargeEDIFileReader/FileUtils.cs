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
        public static int CurrentPageStart { get; set; }

        private static StreamReader FileReader { get; set; }

        private static readonly int PageSize = 1000;

        private static readonly int EnvelopeSize = 106;

        private static char SegmentDelimter { get; set; }

        private static char ElementDelimeter { get; set; }
      
        public static bool OpenEDIFile(string FileName)
        {

            CloseFileReader();

            FileReader = new StreamReader(FileName);
           
            //Read the first 106 bytes, and validate that it at least looks like a 5010 X12 EDI file

            char[] Envelope = new char[EnvelopeSize];
            
            try
            {
                FileReader.Read(Envelope, 1, EnvelopeSize);

                SegmentDelimter = Envelope[103];

                ElementDelimeter = Envelope[105];

                return true;
            }
            catch (Exception e)
            {
                return false; 
            }

        }

        public static void CloseFileReader() => FileReader.Close();



    }
}
