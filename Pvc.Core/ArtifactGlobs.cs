using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public static class ArtifactGlobs
    {
        public static string[] VisualStudio = new[] { "**/obj/**/*.*" };
        public static string[] BinFolder = new[] { "**/bin/**/*.*" };
        public static string[] Git = new[] { "**/.git/**/*.*" };
        public static string[] MSTestResults = new[] { "**/TestResults/**/*.*" };
    }
}
