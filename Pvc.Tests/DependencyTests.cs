using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pvc.Tests
{
    public class DependencyTests
    {
        [Fact]
        public void DependencyGraphSingleItem()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A");
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);

            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(1, pathstrings.Count);
            Assert.Contains("A", pathstrings);
        }

        [Fact]
        public void DependencyGraphSimpleChain()
        {
            var g = new PvcDependencyGraph();
            g.AddDependency("A", "B");
            g.AddDependency("B", "C");
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);

            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(1, pathstrings.Count);
            Assert.Contains("C B A", pathstrings);
        }

        [Fact]
        public void DependencyGraphSingleBranch()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);

            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(2, pathstrings.Count);
            Assert.Contains("B C A", pathstrings);
            Assert.Contains("C B A", pathstrings);
        }

        [Fact]
        public void DependencyGraphBigBranch()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C", "D", "E" });
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);

            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();
            Assert.Equal(24, pathstrings.Count);
            Assert.Contains("E D C B A", pathstrings);
            Assert.Contains("D E C B A", pathstrings);
            Assert.Contains("E C D B A", pathstrings);
            Assert.Contains("C E D B A", pathstrings);
            Assert.Contains("D C E B A", pathstrings);
            Assert.Contains("C D E B A", pathstrings);
            Assert.Contains("E D B C A", pathstrings);
            Assert.Contains("D E B C A", pathstrings);
            Assert.Contains("E B D C A", pathstrings);
            Assert.Contains("B E D C A", pathstrings);
            Assert.Contains("D B E C A", pathstrings);
            Assert.Contains("B D E C A", pathstrings);
            Assert.Contains("E C B D A", pathstrings);
            Assert.Contains("C E B D A", pathstrings);
            Assert.Contains("E B C D A", pathstrings);
            Assert.Contains("B E C D A", pathstrings);
            Assert.Contains("C B E D A", pathstrings);
            Assert.Contains("B C E D A", pathstrings);
            Assert.Contains("D C B E A", pathstrings);
            Assert.Contains("C D B E A", pathstrings);
            Assert.Contains("D B C E A", pathstrings);
            Assert.Contains("B D C E A", pathstrings);
            Assert.Contains("C B D E A", pathstrings);
            Assert.Contains("B C D E A", pathstrings);
        }

        [Fact]
        public void DependencyGraphHugeBranches()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C", "D", "E", "F", "G", "H", "I" });
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);

            var pathstrings = paths.Select(p => string.Join("", p)).ToList();

            Assert.Equal(1, pathstrings.Count);
            Assert.Equal(9, pathstrings[0].Count());
            Assert.EndsWith("A", pathstrings[0]);
        }

        [Fact]
        public void DependencyGraphHugeBranchesWithMoreBranching()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C", "D", "E", "F", "G", "H", "I" });
            g.AddDependency("F", "J");
            g.AddDependency("F", "K");
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);
            Assert.Equal(1, paths.Count);
        }

        [Fact]
        public void DependencyGraphMultipleBranches()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            g.AddDependency("B", "D");
            g.AddDependency("C", "E");
            g.AddDependency("E", "F");
            g.AddDependency("E", "G");
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);
            Assert.Equal(12, paths.Count);
        }

        [Fact]
        public void DependencyGraphRemovesExtraNodes()
        {
            var g = new PvcDependencyGraph();
            g.AddDependency("A", "B");
            // These two aren't actually part of the graph for A.
            g.AddDependency("C", "D");
            // This depends on A, so shouldn't be included in the dependencies for A.
            g.AddDependency("Z", "A");

            var paths = g.GetPaths("A");
            CheckPaths(g, paths);

            Assert.Equal(1, paths.Count);
            Assert.Equal(2, paths[0].Count);
        }

        [Fact]
        public void DependencyGraphCircular()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            g.AddDependency("B", "C");
            g.AddDependency("C", "B");
            Assert.Throws<PvcCircularDependencyException>(() =>
            {
                var paths = g.GetPaths("A");
            });
            try
            {
                var paths = g.GetPaths("A");
            }
            catch (PvcCircularDependencyException e)
            {
                Assert.Equal(e.CircularPath.Count, 4);
                Assert.Equal(e.CircularPath[0], "A");
                Assert.Equal(e.CircularPath[1], "B");
                Assert.Equal(e.CircularPath[2], "C");
                Assert.Equal(e.CircularPath[3], "B");
            }
        }

        [Fact]
        public void DependencyGraphComplex()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            g.AddDependencies("B", new string[] { "D", "E" });
            g.AddDependencies("C", new string[] { "D", "E" });
            var paths = g.GetPaths("A");
            CheckPaths(g, paths);

            var pathstrings = paths.Select(p => string.Join(" ", p)).Distinct().ToList();

            Assert.Equal(4, pathstrings.Count);
            Assert.Contains("E D C B A", pathstrings);
            Assert.Contains("D E C B A", pathstrings);
            Assert.Contains("E D B C A", pathstrings);
            Assert.Contains("D E B C A", pathstrings);
        }

        /// <summary>
        /// Test the test method that we use everywhere else.
        /// </summary>
        [Fact]
        public void DependencyGraphCheckPaths()
        {
            var g = new PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });

            Assert.Throws<InvalidPathException>(() =>
            {
                CheckPath(g, new List<string>(new string[]{"A", "B", "C"}));
            });

            Assert.DoesNotThrow(() =>
            {
                CheckPath(g, new List<string>(new string[] { "C", "B", "A" }));
            });
        }

        private void CheckPaths(PvcDependencyGraph g, List<List<string>> paths)
        {
            Assert.True(paths.Count >= 1);
            foreach (var path in paths)
            {
                CheckPath(g, path);
            }
        }

        private void CheckPath(PvcDependencyGraph g, List<string> path)
        {
            var deps = new HashSet<string>();
            foreach (var node in path)
            {
                foreach (var dep in g.GetDependencies(node))
                {
                    if (!deps.Contains(dep))
                    {
                        throw new InvalidPathException(path);
                    }
                }
                deps.Add(node);
            }
        }

        private class InvalidPathException : Exception
        {
            List<string> Path;
            public InvalidPathException(List<string> path)
            {
                Path = path;
            }
        }
    }
}
