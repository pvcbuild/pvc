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
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A");
            var paths = g.GetPaths("A");
            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(1, pathstrings.Count);
            Assert.Contains("A", pathstrings);
        }

        [Fact]
        public void DependencyGraphSimpleChain()
        {
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependency("A", "B");
            g.AddDependency("B", "C");
            var paths = g.GetPaths("A");
            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(1, pathstrings.Count);
            Assert.Contains("C B A", pathstrings);
        }

        [Fact]
        public void DependencyGraphSingleBranch()
        {
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            var paths = g.GetPaths("A");
            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(2, pathstrings.Count);
            Assert.Contains("B C A", pathstrings);
            Assert.Contains("C B A", pathstrings);
        }

        [Fact]
        public void DependencyGraphBigBranch()
        {
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C", "D", "E" });
            var paths = g.GetPaths("A");
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
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C", "D", "E", "F", "G", "H", "I" });
            var paths = g.GetPaths("A");
            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(1, pathstrings.Count);
            Assert.Equal("I H G F E D C B A", pathstrings[0]);
        }

        [Fact]
        public void DependencyGraphHugeBranchesWithMoreBranching()
        {
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C", "D", "E", "F", "G", "H", "I" });
            g.AddDependency("F", "J");
            g.AddDependency("F", "K");
            var paths = g.GetPaths("A");
            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(2, pathstrings.Count);
            Assert.Contains("J K I H G F E D C B A", pathstrings);
            Assert.Contains("K J I H G F E D C B A", pathstrings);
        }


        [Fact]
        public void DependencyGraphMultipleBranches()
        {
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            g.AddDependency("B", "D");
            g.AddDependency("C", "E");
            g.AddDependency("E", "F");
            g.AddDependency("E", "G");
            var paths = g.GetPaths("A");

            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(6, pathstrings.Count);
            Assert.Contains("D B F G E C A", pathstrings);
            Assert.Contains("D B G F E C A", pathstrings);

            Assert.Contains("F G E C D B A", pathstrings);
            Assert.Contains("G F E C D B A", pathstrings);

            Assert.Contains("G D B F E C A", pathstrings);
            Assert.Contains("F D B G E C A", pathstrings);
        }

        [Fact]
        public void DependencyGraphTerminalCircular()
        {
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            g.AddDependency("B", "C");
            g.AddDependency("C", "B");
            var paths = g.GetPaths("A");

            var pathstrings = paths.Select(p => string.Join(" ", p)).ToList();

            Assert.Equal(2, pathstrings.Count);
            Assert.Contains("C B A", pathstrings);
            Assert.Contains("B C A", pathstrings);
        }

        [Fact]
        public void DependencyGraphNonterminalCircular()
        {
            var g = new PvcCore.PvcDependencyGraph();
            g.AddDependencies("A", new string[] { "B", "C" });
            g.AddDependency("B", "C");
            g.AddDependency("C", "B");
            g.AddDependency("C", "D");
            var paths = g.GetPaths("A");

            // Circular dependencies can cause some duplication in the list.
            var pathstrings = paths.Select(p => string.Join(" ", p)).Distinct().ToList();

            Assert.Equal(2, pathstrings.Count());
            Assert.Contains("B D C A", pathstrings);
            Assert.Contains("D C B A", pathstrings);
        }
    }
}
