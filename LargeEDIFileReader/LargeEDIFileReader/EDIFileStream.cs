using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace LargeEDIFileReader
{
    public class EDIFileStream : IDisposable
    {
        private FileStream Reader { get; set; }

        private static char SegmentDelimter { get; set; }

        private static char ElementDelimeter { get; set; }


        private static readonly int EnvelopeSize = 106;

        private static UTF8Encoding Encoding = new UTF8Encoding(true);

        public static Dictionary<int, int> PageOffsetMap { get; set; } = new Dictionary<int, int>();

        public EDIFileStream(string FileName)
        {
            this.Reader = File.OpenRead(FileName);
        }

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
        public int LoadSegmentOffset()
        {
            int offset = 0;
            int pageNumber = 1;
            int segmentsRead = 0;
            PageOffsetMap.Add(pageNumber, 0);

            int next = Reader.ReadByte();
            while (next > 0)
            {
                if (next == SegmentDelimter)
                {
                    segmentsRead++;
                    if (segmentsRead % 10000 == 0)
                    {
                        pageNumber++;
                        PageOffsetMap.Add(pageNumber, offset);
                    }
                } 
                else
                {
                    offset++;
                }
                next = Reader.ReadByte();
            }
            return pageNumber;
        }

        //Read and build a segment from the stream
        private string ReadSegment()
        {
            string elementText = String.Empty;
            bool keepReading = true;
            int next = Reader.ReadByte();
            while (keepReading && next > 0)
            {
                if (next != SegmentDelimter)
                {
                    elementText += (char)next;
                    next = Reader.ReadByte();
                }
                else
                {
                    elementText += Environment.NewLine;
                    keepReading = false;
                }              
            }
            return elementText;
        }

        //Read a number of segments from the stream, and return them a string.
        //This a "page" and gets loaded into the main window TextBox
        public string ReadPage(int pageNumber)
        {
 
            var builder = new StringBuilder();
            int pageStartPos = PageOffsetMap[pageNumber];
            this.Reader.Seek(pageStartPos, SeekOrigin.Begin);

            for (int i=0; i< 10000; i++)
            {
                builder.Append(Convert.ToString(i + 1).PadRight(5));
                builder.Append(ReadSegment());
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
