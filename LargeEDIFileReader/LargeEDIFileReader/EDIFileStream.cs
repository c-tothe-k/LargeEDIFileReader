using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace LargeEDIFileReader
{
    public class EDIFileStream : IDisposable
    {
        public enum SearchType
        {
            Text,
            CountOnly
        }

        private Stream Reader { get; set; }

        private static char SegmentDelimter { get; set; }

        private static char ElementDelimeter { get; set; }


        private static readonly int EnvelopeSize = 106;

        private static UTF8Encoding Encoding = new UTF8Encoding(true);

        private struct PageTracker
        {
           internal int PageStartByteOffset { get; set; }

           internal int PageStartSegmentOffset { get; set; }
        }

        private static Dictionary<int, PageTracker> PageOffsetMap { get; set; } = new Dictionary<int, PageTracker>();

        public EDIFileStream(Stream stream) =>
            this.Reader = stream;
        
        private int PageSize { get; set; }

        //Extract the EDI envelope (first 106 chars) and return it
        //as a string
        public string ReadEnvelope()
        {
            string textToReturn = String.Empty;
            byte[] buffer = new byte[EnvelopeSize];
            int bytesRead =  this.Reader.Read(buffer, 0, EnvelopeSize);
            if (bytesRead > 0)
                textToReturn = Encoding.GetString(buffer);

            this.Reader.Seek(0, SeekOrigin.Begin);

            ElementDelimeter = textToReturn[103];

            SegmentDelimter = textToReturn[105];//TODO account for trailing newlines               

            return textToReturn;
        }

        //Load a dictionary that keeps track of where in the filestream
        //each page break is, so we can easily jump to any page we want.
        public int LoadSegmentOffset(int pageSize = 10_000)
        {         

            this.PageSize = pageSize;

            PageOffsetMap.Clear(); 

            int offset = 0;
            int pageNumber = 1;
            int segmentsRead = 0;
            PageOffsetMap.Add(pageNumber, new PageTracker
            { PageStartByteOffset = offset,
                PageStartSegmentOffset = segmentsRead 
             });

            int next = Reader.ReadByte();
            while (next > 0)
            {
                if (next == SegmentDelimter)
                {
                    segmentsRead++;
                    if (segmentsRead % pageSize == 0)
                    {
                        pageNumber++;             

                        PageOffsetMap.Add(pageNumber, new PageTracker()
                        {
                            PageStartByteOffset = offset + 1,
                            PageStartSegmentOffset = segmentsRead
                        }); 
                    }
                }               
                offset++;                
                next = Reader.ReadByte();
            }
            return pageNumber;
        }

        //Read and build a segment from the stream
        private string ReadSegment()
        {
            var segmentText = new StringBuilder();
            bool keepReading = true;
            int next = Reader.ReadByte();
            while (keepReading && next > 0)
            {
                if (next != SegmentDelimter)
                {
                    segmentText.Append((char)next);
                    next = Reader.ReadByte();
                }
                else
                {
                    segmentText.Append(Environment.NewLine);
                    keepReading = false;
                }              
            }
            return segmentText.ToString();
        }

        //Read a number of segments from the stream, and return them a string.
        //This a "page" and gets loaded into the main window TextBox
        public string ReadPage(int pageNumber)
        {
 
            var builder = new StringBuilder();
            int pageStartPos = PageOffsetMap[pageNumber].PageStartByteOffset;
            this.Reader.Seek(pageStartPos, SeekOrigin.Begin);
            int i = 0;
            string curSegment = "x";
            while (i< this.PageSize && !String.IsNullOrEmpty(curSegment))
            {
               // builder.Append(Convert.ToString( i + 1 + (this.PageSize * (pageNumber-1) ) ).PadRight(10));
                curSegment = ReadSegment();
                builder.Append(curSegment);
                i++;
            }
            return builder.ToString();
        }

        public string LoadPageFromSegment(int segmentLineNumber, out int pageNumber, out int pageSegmentStart)
        {
            var pageInfo = PageOffsetMap.Where((kvp) => kvp.Value.PageStartSegmentOffset <= segmentLineNumber).Last();
            pageNumber = pageInfo.Key;
            pageSegmentStart = pageInfo.Value.PageStartSegmentOffset;

            return ReadPage(pageNumber);
        }

        public string SearchFile(SearchType type, string searchText)
        {

            this.Reader.Seek(0, SeekOrigin.Begin); //start from the begining
            var builder = new StringBuilder();
            int maxFound = type == SearchType.CountOnly ? Int32.MaxValue : 30000;
            int amtFound = 0;
            string curSegment = "x";
            int segCount = 0;
            while (amtFound < maxFound && !String.IsNullOrEmpty(curSegment))
            {
                segCount++;
                curSegment = ReadSegment();
                if (curSegment.Contains(searchText))
                {
                    amtFound++;
                    if (type == SearchType.Text)
                    {
                        builder.Append($"{segCount}: {curSegment}");
                    }

                }
            }

            if (type == SearchType.CountOnly)
            {
                builder.Append($"{amtFound} hits.");
            }

            return builder.ToString();
        }

        public void Dispose()
        {
            this.Reader.Close();
            this.Reader.Dispose();
        }
    }
}
