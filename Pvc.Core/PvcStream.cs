using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcStream : Stream
    {
        private readonly Stream stream;

        public string StreamName { get; private set; }

        public PvcStream(FileStream fileStream)
        {
            this.stream = fileStream;
        }

        public PvcStream(MemoryStream memoryStream)
        {
            this.stream = memoryStream;
        }

        public PvcStream(BufferedStream bufferedStream)
        {
            this.stream = bufferedStream;
        }

        public PvcStream(NetworkStream networkStream)
        {
            this.stream = networkStream;
        }

        public PvcStream As(string streamName)
        {
            this.StreamName = streamName;
            return this;
        }

        public override bool CanRead
        {
            get { return this.stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.stream.CanWrite; }
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override long Length
        {
            get { return this.stream.Length; }
        }

        public override long Position
        {
            get
            {
                return this.stream.Position;
            }
            set
            {
                this.stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Read the contents of the stream from the beginning and return as a string. Position is
        /// reset to its previous location after the read is completed.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var streamPosition = this.stream.Position;
            this.stream.Position = 0;

            var result = new StreamReader(this.stream).ReadToEnd();
            this.stream.Position = streamPosition;

            return result;
        }
    }
}
