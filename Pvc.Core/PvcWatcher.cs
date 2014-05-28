using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcWatcher
    {
        public static List<PvcWatcherItem> Items = new List<PvcWatcherItem>();

        public static void RegisterWatchPipe(List<string> globs, List<Func<PvcPipe, PvcPipe>> pipeline, List<string> additionalFiles)
        {
            Items.Add(new PvcWatcherItem(globs, pipeline, additionalFiles));
        }
    }

    public class PvcWatcherItem
    {
        public readonly List<string> Globs;
        public readonly List<Func<PvcPipe, PvcPipe>> Pipeline;
        public readonly List<string> AdditionalFiles;

        public PvcWatcherItem(List<string> globs, List<Func<PvcPipe, PvcPipe>> pipeline, List<string> additionalFiles)
        {
            this.Globs = globs;
            this.Pipeline = pipeline;
            this.AdditionalFiles = additionalFiles;
        }
    }
}
