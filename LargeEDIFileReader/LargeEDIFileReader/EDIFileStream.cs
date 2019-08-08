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

        private static UTF8Encoding Encoding = new UTF8Encoding(true);

        public EDIFileStream(string FileName)
        {
            this.Reader = File.OpenRead(FileName);
        }

        public string Read(int start, int length)
        {
            byte[] buffer = new byte[length];
            this.Reader.Read(buffer, start, length);
            this.Reader.Seek(0, SeekOrigin.Begin);
            return Encoding.GetString(buffer);
        }

        public string ReadPage(int start, int segmentCount, char segmentDelimeter)
        {
            int segmentsRead = 0;
            string buffer = String.Empty;

            //If we read the segment delimeter, replace it with a new line.
            //Stop when we've read the requested number of elements.
            int next = Reader.ReadByte();
            while (segmentsRead < segmentCount && next > 0)
            {               
                if (next != segmentDelimeter)
                {
                    buffer += (char)next;
                }
                else
                {
                    buffer += Environment.NewLine;
                    segmentsRead++;
                }
                next = Reader.ReadByte();
            }
            return buffer;
        }

        public void Dispose()
        {
            this.Reader.Close();
            this.Reader.Dispose();
        }
    }
}
