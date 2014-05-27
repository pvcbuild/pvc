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
        private readonly Lazy<Stream> stream;

        /// <summary>
        /// Arbitrary name for a stream. For streams created by pvc.Source, this will be the name
        /// or relative path to the file. Don't rely on this for file name processing or detection,
        /// instead use OriginalSourcePath.
        /// </summary>
        public string StreamName { get; private set; }

        /// <summary>
        /// Location on disk of the original source for this stream. Useful for plugins that need
        /// to provide include paths or other similar configuration to their internals.
        /// </summary>
        public string OriginalSourcePath { get; private set; }

        /// <summary>
        /// Tags represent a way to filter streams for plugins. By default all streams created from
        /// files will receive a tag of their file extension. Other tags may be applied to streams
        /// by plugins during the pipe processing.
        /// </summary>
        public List<string> Tags { get; set; }

        public PvcStream(Func<FileStream> fileStream)
            : this()
        {
            this.stream = new Lazy<Stream>(fileStream);
        }

        public PvcStream(Func<MemoryStream> memoryStream)
            : this()
        {
            this.stream = new Lazy<Stream>(memoryStream);
        }

        public PvcStream(Func<BufferedStream> bufferedStream)
            : this()
        {
            this.stream = new Lazy<Stream>(bufferedStream);
        }

        public PvcStream(Func<NetworkStream> networkStream)
            : this()
        {
            this.stream = new Lazy<Stream>(networkStream);
        }

        public PvcStream(Func<PvcStream> pvcStream)
            : this()
        {
            this.stream = new Lazy<Stream>(pvcStream);
        }

        internal PvcStream()
        {
            this.Tags = new List<string>();
        }

        public PvcStream As(string streamName)
        {
            return this.As(streamName, null);
        }

        public PvcStream As(string streamName, string originalSourcePath)
        {
            this.StreamName = streamName;
            this.OriginalSourcePath = originalSourcePath;
            this.Tags.Add(Path.GetExtension(originalSourcePath));

            return this;
        }

        public override bool CanRead
        {
            get { return this.stream.Value.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.stream.Value.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.stream.Value.CanWrite; }
        }

        public override void Flush()
        {
            this.stream.Value.Flush();
        }

        public override long Length
        {
            get { return this.stream.Value.Length; }
        }

        public override long Position
        {
            get
            {
                return this.stream.Value.Position;
            }
            set
            {
                this.stream.Value.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.Value.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Value.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.Value.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.Value.Write(buffer, offset, count);
        }

        /// <summary>
        /// Read the contents of the stream from the beginning and return as a string. Position is
        /// reset to its previous location after the read is completed.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var streamPosition = this.stream.Value.Position;
            this.stream.Value.Position = 0;

            var result = new StreamReader(this.stream.Value).ReadToEnd();
            this.stream.Value.Position = streamPosition;

            return result;
        }

        public void ResetStreamPosition()
        {
            this.Position = 0;
        }
    }
}
