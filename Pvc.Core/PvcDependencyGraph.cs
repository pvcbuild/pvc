using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcDependencyGraph
    {
        private Dictionary<string, PvcDependencyNode> map;

        public PvcDependencyGraph()
        {
            map = new Dictionary<string, PvcDependencyNode>();
        }

        public void AddDependencies(string from, IEnumerable<string> dependencies = null)
        {
            var f = GetOrAddNode(from);

            if (dependencies == null)
                return;

            foreach (string to in dependencies)
            {
                f.Neighbors.Add(to, GetOrAddNode(to));
            }
        }

        public void AddDependency(string from, string to)
        {
            GetOrAddNode(from).Neighbors.Add(to, GetOrAddNode(to));
        }

        private PvcDependencyNode GetOrAddNode(string name)
        {
            if (map.ContainsKey(name))
                return map[name];
            var node = new PvcDependencyNode(name);
            map.Add(name, node);
            return node;
        }

        public List<List<string>> GetPaths(string name)
        {
            var startNode = GetOrAddNode(name);
            var paths = new List<List<string>>();
            var path = new List<string>();
            var inPath = new HashSet<string>();
            paths.Add(path);

            BuildPaths(startNode, path, inPath, paths, new List<PvcDependencyNode>());
            RemoveShortPaths(paths);
            ExpandAndReversePaths(paths);

            return paths;
        }

        private void ExpandAndReversePaths(List<List<string>> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                var newpath = new List<string>();
                foreach(var p in paths[i])
                {
                    if (p.Contains('\t'))
                    {
                        foreach (var splitpath in p.Split('\t'))
                        {
                            newpath.Add(splitpath);    
                        }
                    }
                    else
                    {
                        newpath.Add(p);
                    }
                }
                newpath.Reverse();
                paths[i] = newpath;
            }
        }

        private void RemoveShortPaths(List<List<string>> paths)
        {
            int maxlength = 0;
            foreach(var path in paths.ToList())
            {
                if (path.Count < maxlength)
                    paths.Remove(path);
                else
                    maxlength = path.Count;
            }
            foreach(var path in paths.ToList())
            {
                if (path.Count < maxlength)
                    paths.Remove(path);
            }
        }

        private void BuildPaths(
            PvcDependencyNode node,
            List<string> currentPath,
            HashSet<string> inCurrentPath,
            List<List<string>> paths,
            List<PvcDependencyNode> nextNodes)
        {
            // Stop on circular dependencies.
            if (inCurrentPath.Contains(node.Name))
                return;

            currentPath.Add(node.Name);
            inCurrentPath.Add(node.Name);

            if (node.Neighbors.Count() == 0)
            {
                if (nextNodes.Count() == 0)
                    return;

                AddNeighbors(node, nextNodes, currentPath, inCurrentPath, paths, nextNodes);
            }
            else if (node.Neighbors.Count() == 1)
            {
                var neighbor = node.Neighbors.First().Value;
                BuildPaths(neighbor, currentPath, inCurrentPath, paths, nextNodes);
            }
            else if (node.Neighbors.Count() <= 4)
            {
                // Add every possible sort of neighbors
                var neighbors = node.Neighbors.Values.ToList();
                AddNeighbors(node, neighbors, currentPath, inCurrentPath, paths, nextNodes);
            }
            else
            {
                AddAndGroupNeighbors(node, currentPath, inCurrentPath, paths, nextNodes);
            }
        }

        private void AddAndGroupNeighbors(
            PvcDependencyNode node,
            List<string> currentPath,
            HashSet<string> inCurrentPath,
            List<List<string>> paths,
            List<PvcDependencyNode> nextNodes)
        {
            // 5+ dependencies on a single thing makes the graph factorial-explode.
            // Sacrifice parallelism on this and instead treat it like a single thing.

            var fakeNodeName = string.Join("\t", node.Neighbors.Values.Select(n => n.Name).ToList());
            var fakeNode = new PvcDependencyNode(fakeNodeName);
            foreach(var neighbor in node.Neighbors.Values)
            {
                inCurrentPath.Add(neighbor.Name);
                fakeNode.Neighbors = fakeNode.Neighbors
                    .Union(neighbor.Neighbors)
                    .ToDictionary(k => k.Key, k => k.Value);
            }

            BuildPaths(fakeNode, currentPath, inCurrentPath, paths, nextNodes);
        }

        private void AddNeighbors(
            PvcDependencyNode node,
            List<PvcDependencyNode> neighbors,
            List<string> currentPath,
            HashSet<string> inCurrentPath,
            List<List<string>> paths,
            List<PvcDependencyNode> nextNodes)
        {
            var currentPathAtStart = new List<string>(currentPath);
            var inCurrentPathAtStart = new HashSet<string>(inCurrentPath);
            var first = true;

            foreach (var neighbor in neighbors)
            {
                var newNextNodes = new List<PvcDependencyNode>(neighbors);
                var existingNextNodes = new List<PvcDependencyNode>(nextNodes);
                newNextNodes.RemoveAll(n => n == neighbor);
                existingNextNodes.RemoveAll(n => n == neighbor);

                foreach (var nextNode in existingNextNodes)
                {
                    if (!newNextNodes.Contains(nextNode))
                        newNextNodes.Add(nextNode);
                }

                List<string> newPath;
                HashSet<string> inNewPath;
                if (first)
                {
                    // Use the existing path
                    newPath = currentPath;
                    inNewPath = inCurrentPath;
                    first = false;
                }
                else
                {
                    // Add a new path
                    newPath = new List<string>(currentPathAtStart);
                    inNewPath = new HashSet<string>(inCurrentPathAtStart);
                    paths.Add(newPath);
                }
                BuildPaths(neighbor, newPath, inNewPath, paths, newNextNodes);
            }
        }

        private class PvcDependencyNode
        {
            public string Name;
            public Dictionary<string, PvcDependencyNode> Neighbors;

            public PvcDependencyNode(string name)
            {
                Name = name;
                Neighbors = new Dictionary<string, PvcDependencyNode>();
            }
        }
    }
}
