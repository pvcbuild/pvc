using Minimatch;
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
        internal IEnumerable<PvcStream> streams;
        private List<string> globs;
        private List<Func<PvcPipe, PvcPipe>> pipeline;
        private string baseDirectoryPath;

        public PvcPipe()
        {
            this.streams = new List<PvcStream>();
            this.globs = new List<string>();
            this.pipeline = new List<Func<PvcPipe, PvcPipe>>();
            this.baseDirectoryPath = Directory.GetCurrentDirectory();
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
            this.pipeline.Add((pipe) => pipe.Pipe(tags, plugin));

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

            // reset directory (some plugins might screw this up)
            Directory.SetCurrentDirectory(this.baseDirectoryPath);
            
            // reset stream pointers
            this.resetStreamPositions(applicableStreams);
            this.resetStreamPositions(resultStreams);

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
            this.pipeline.Add((pipe) => pipe.PipeIf(ifRegex, plugin));

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

            var resultStreams = plugin(matchingStreams);
            this.streams = resultStreams.Concat(nonMatchingStreams);

            // reset directory (some plugins might screw this up)
            Directory.SetCurrentDirectory(this.baseDirectoryPath);

            this.resetStreamPositions(resultStreams);
            this.resetStreamPositions(matchingStreams);

            return this;
        }

        public PvcPipe PipeIf(Regex ifRegex, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> truePlugin, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> falsePlugin)
        {
            this.pipeline.Add((pipe) => pipe.PipeIf(ifRegex, truePlugin, falsePlugin));

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

            // reset directory (some plugins might screw this up)
            Directory.SetCurrentDirectory(this.baseDirectoryPath);

            this.resetStreamPositions(this.streams);
            return this;
        }

        public PvcPipe Save(string outputPath)
        {
            this.pipeline.Add((pipe) => pipe.Save(outputPath));
            PvcWatcher.IgnoredPaths.Add(Path.GetFullPath(outputPath));

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

        public PvcPipe Source(params string[] inputs)
        {
            this.globs.AddRange(inputs);
            var globs = inputs.Where(x => Regex.IsMatch(x, @"(\*|\!)"));
            var streams = inputs.Except(globs).Concat(FilterPaths(globs))
                .Select(x => new { RelativePath = PvcUtil.PathRelativeToCurrentDirectory(x), FullPath = Path.GetFullPath(x) })
                .Select(x => new PvcStream(() => new FileStream(x.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).As(x.RelativePath, x.FullPath));

            this.streams = this.streams.Concat(streams);
            return this;
        }

        public PvcPipe Watch(params string[] additionalFiles)
        {
            var pipe = new PvcPipe()
            {
                streams = this.streams,
                globs = this.globs
            };

            PvcWatcher.RegisterWatchPipe(this.globs, this.pipeline, additionalFiles.ToList());

            return pipe;
        }

        internal IEnumerable<string> FilterPaths(IEnumerable<string> globs)
        {
            var allPaths = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories).Select(x => PvcUtil.PathRelativeToCurrentDirectory(x));
            var miniMatches = globs.Select(g => new Minimatcher(g, new Options
            {
                AllowWindowsPaths = true,
                MatchBase = true,
                Dot = true,
                NoCase = true,
                NoNull = true
            }));

            return miniMatches.SelectMany(m => m.Filter(allPaths));
        }

        private void resetStreamPositions(IEnumerable<PvcStream> resetStreams)
        {
            resetStreams.ToList().ForEach(x => x.ResetStreamPosition());
        }
    }
}
