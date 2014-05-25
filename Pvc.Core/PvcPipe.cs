using PvcPlugins;
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

        public PvcPipe Pipe(PvcPlugin plugin)
        {
            return this.Pipe(plugin.SupportedTags, (streams) => plugin.Execute(streams));
        }

        public PvcPipe Pipe(Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> plugin)
        {
            return this.Pipe("*", plugin);
        }

        public PvcPipe Pipe(string tag, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> plugin)
        {
            IEnumerable<string> tags = new[] { tag };
            if (tag.IndexOf(',') != -1)
            {
                tags = tag.Split(',').ToList().Select(x => x.Trim());
            }

            return this.Pipe(tags.ToArray(), plugin);
        }

        public PvcPipe Pipe(string[] tags, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> plugin)
        {
            var applicableStreams = new List<PvcStream>();
            var nonApplicableStreams = new List<PvcStream>();

            var isWildcardMatch = tags.Contains("*");

            if (isWildcardMatch)
            {
                applicableStreams = this.streams.ToList();
            }
            else
            {
                foreach (var stream in this.streams)
                {
                    if (tags.Intersect(stream.Tags).Count() > 0)
                        applicableStreams.Add(stream);
                    else
                        nonApplicableStreams.Add(stream);
                }
            }

            var resultStreams = plugin(applicableStreams);
            this.streams = nonApplicableStreams.Concat(resultStreams);
            this.resetStreamPositions(applicableStreams);
            return this;
        }

        public PvcPipe PipeIf(string ifRegex, PvcPlugin plugin)
        {
            return this.PipeIf(new Regex(ifRegex), plugin);
        }

        public PvcPipe PipeIf(string ifRegex, PvcPlugin truePlugin, PvcPlugin falsePlugin)
        {
            return this.PipeIf(new Regex(ifRegex), truePlugin, falsePlugin);
        }

        public PvcPipe PipeIf(string ifRegex, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> plugin)
        {
            return this.PipeIf(new Regex(ifRegex), plugin);
        }

        public PvcPipe PipeIf(string ifRegex, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> truePlugin, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> falsePlugin)
        {
            return this.PipeIf(new Regex(ifRegex), truePlugin, falsePlugin);
        }

        public PvcPipe PipeIf(Regex ifRegex, PvcPlugin plugin)
        {
            return this.PipeIf(ifRegex, (streams) => plugin.Execute(streams));
        }

        public PvcPipe PipeIf(Regex ifRegex, PvcPlugin truePlugin, PvcPlugin falsePlugin)
        {
            return this.PipeIf(ifRegex, (streams) => truePlugin.Execute(streams), (streams) => falsePlugin.Execute(streams));
        }

        public PvcPipe PipeIf(Regex ifRegex, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> plugin)
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

            this.streams = plugin(matchingStreams).Concat(nonMatchingStreams);
            this.resetStreamPositions(matchingStreams);

            return this;
        }

        public PvcPipe PipeIf(Regex ifRegex, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> truePlugin, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> falsePlugin)
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

            this.streams = truePlugin(matchingStreams).Concat(falsePlugin(nonMatchingStreams));
            this.resetStreamPositions(this.streams);
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

            this.resetStreamPositions(this.streams);

            return this;
        }

        private void resetStreamPositions(IEnumerable<PvcStream> resetStreams)
        {
            resetStreams.ToList().ForEach(x => x.ResetStreamPosition());
        }
    }
}
