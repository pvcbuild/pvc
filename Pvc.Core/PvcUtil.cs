using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public static class PvcUtil
    {
        public static PvcStream StringToStream(string data, string streamName)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(data);
            writer.Flush();

            return new PvcStream(stream).As(streamName);
        }

        public static string StreamToTempFile(PvcStream stream)
        {
            // be sure to start from beginning
            stream.Position = 0;

            var tmpFileName = Path.GetTempFileName();
            File.WriteAllText(tmpFileName, new StreamReader(stream).ReadToEnd());

            return tmpFileName;
        }
    }
}
