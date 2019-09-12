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
        public enum NavigationType
        {
            Next,
            Previous
        }

        public static int CurrentPageNumber { get; set; } = 0;

        private static EDIFileStream FileReader { get; set; }


        public static int TotalPages { get; set; }

       

        public static bool OpenEDIFile(EDIFileStream EdiFileStream)
        {
            if (FileReader != null) FileReader.Dispose(); // if we're opening a 2nd (or more) file, close the stream from the previous one

            FileReader = EdiFileStream;
            //Read the first 106 bytes, and validate that it at least looks like a 5010 X12 EDI file
             string Envelope = String.Empty;
             CurrentPageNumber = 0;
              try
                {
                   Envelope = FileReader.ReadEnvelope();
                   //Check for X12 Control Segment Name
                   if (String.IsNullOrEmpty(Envelope) || Envelope.Length != 106 || Envelope.Substring(0, 3) != "ISA")
                       throw new ArgumentException("File is not an X12 EDI file or file is missing control segment.");

                   TotalPages = FileReader.LoadSegmentOffset();         
                   return true;
                }
                catch (ArgumentException e)
                {
                    return false;
                }
            
        }

        public static string PerformSearch(EDIFileStream.SearchType type, string textToFind) =>
            String.IsNullOrWhiteSpace(textToFind)? "" : FileReader.SearchFile(type, textToFind);


        public static string LoadPageFromSegmentPosition(int segmentLineNumber, out int pageStartSegmentOffset)
        {
            int newPageNumber = 0;              
            var pageText =  FileReader.LoadPageFromSegment(segmentLineNumber, out newPageNumber, out pageStartSegmentOffset);
            CurrentPageNumber = newPageNumber;

            return pageText;
        }
       

        public static string LoadPage(NavigationType type)
        {
            if (type == NavigationType.Next)
                CurrentPageNumber++;
            else
                CurrentPageNumber--;

            string page = FileReader.ReadPage(CurrentPageNumber);         
            return page;
        }
       
        public static void Close()
        {
            FileReader.Dispose();
        }
    }
}
