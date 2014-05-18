using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcPipe
    {
        private IEnumerable<PvcStream> streams = null;

        public PvcPipe(IEnumerable<PvcStream> streams)
        {
            this.streams = streams;
        }

        public PvcPipe Pipe(string ifRegex, PvcPlugin plugin)
        {
            return this.Pipe(new Regex(ifRegex), plugin);
        }

        public PvcPipe Pipe(string ifRegex, PvcPlugin truePlugin, PvcPlugin falsePlugin)
        {
            return this.Pipe(new Regex(ifRegex), truePlugin, falsePlugin);
        }

        public PvcPipe Pipe(Regex ifRegex, PvcPlugin plugin)
        {
            var matchingStreams = new List<PvcStream>();
            var nonMatchingStreams = new List<PvcStream>();

            foreach (var stream in this.streams)
            {
                if (ifRegex.IsMatch(stream.StreamName))
                {
                    matchingStreams.Add(stream);
                }
                else
                {
                    nonMatchingStreams.Add(stream);
                }
            }

            this.streams = plugin.Execute(matchingStreams).Concat(nonMatchingStreams);
            this.resetStreamPositions();

            return this;
        }

        public PvcPipe Pipe(Regex ifRegex, PvcPlugin truePlugin, PvcPlugin falsePlugin)
        {
            var matchingStreams = new List<PvcStream>();
            var nonMatchingStreams = new List<PvcStream>();

            foreach (var stream in this.streams)
            {
                if (ifRegex.IsMatch(stream.StreamName))
                {
                    matchingStreams.Add(stream);
                }
                else
                {
                    nonMatchingStreams.Add(stream);
                }
            }

            this.streams = truePlugin.Execute(matchingStreams).Concat(falsePlugin.Execute(nonMatchingStreams));

            this.resetStreamPositions();
            return this;
        }

        public PvcPipe Pipe(PvcPlugin plugin)
        {
            this.streams = plugin.Execute(this.streams);
            this.resetStreamPositions();

            return this;
        }

        public PvcPipe Save(string outputPath)
        {
            var dirPath = new DirectoryInfo(outputPath);
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            foreach (var stream in this.streams)
            {
                var streamContents = new StreamReader(stream).ReadToEnd();
                var fileSavePath = Path.Combine(outputPath, stream.StreamName);

                // verify directory exists for write
                new FileInfo(fileSavePath).Directory.Create();
                File.WriteAllText(fileSavePath, streamContents);
            }

            this.resetStreamPositions();

            return this;
        }

        private void resetStreamPositions()
        {
            // move streams back to beginning
            foreach (var stream in this.streams)
            {
                stream.Position = 0;
            }
        }
    }
}
