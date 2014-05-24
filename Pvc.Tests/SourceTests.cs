using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pvc.Tests
{
    public class SourceTests
    {
        public SourceTests()
        {
            createSourceTestStructures();
        }

        private void createSourceTestStructures()
        {
            try
            {
                Directory.CreateDirectory("inputs/");
                File.WriteAllText("inputs/test1.css", "");
                File.WriteAllText("inputs/test2.css", "");
                File.WriteAllText("test1.js", "");
                File.WriteAllText("test2.js", "");
            }
            catch (IOException) { }
        }

        private void globTest(string pattern, int expectedCount)
        {
            var pvc = new PvcCore.Pvc();
            var src = pvc.Source(pattern);
            Assert.DoesNotThrow(() =>
            {
                src.Pipe((streams) =>
                {
                    Assert.True(streams.Count() == 2, "Stream count was not " + 2);
                    return streams;
                });
            });
        }

        [Fact]
        public void SourceGlobsRelativeDirectoryShallowMatchWithoutExtension()
        {
            globTest("inputs/*", 2);
        }

        [Fact]
        public void SourceGlobsRelativeDirectoryShallowMatchWithExtension()
        {
            globTest("inputs/*.css", 2);
        }
        
        [Fact]
        public void SourceGlobsBaseDirectoryShallowMatchWithExtension()
        {
            globTest("*.css", 2);
        }
        
        [Fact]
        public void SourceGlobsRecursiveMatchWithExtension()
        {
            globTest("**/*.css", 2);
        }

        [Fact]
        public void SourceGlobsRecursiveMatchWithoutExtension()
        {
            var pvc = new PvcCore.Pvc();
            var src = pvc.Source("**/*");
            Assert.DoesNotThrow(() =>
            {
                src.Pipe((streams) =>
                {
                    // We put at least 4 files in that should match but assemblies are there too
                    Assert.True(streams.Count() > 2);
                    return streams;
                });
            });
        }
    }
}
