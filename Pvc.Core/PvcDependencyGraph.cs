using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcDependencyGraph
    {
        private Dictionary<string, DependencyNode> map;

        public PvcDependencyGraph()
        {
            map = new Dictionary<string, DependencyNode>();
        }

        public void AddDependencies(string from, IEnumerable<string> dependencies)
        {
            var f = GetOrAddNode(from);
            foreach (string to in dependencies)
            {
                f.Neighbors.Add(to, GetOrAddNode(to));
            }
        }

        public void AddDependency(string from, string to)
        {
            GetOrAddNode(from).Neighbors.Add(to, GetOrAddNode(to));
        }

        private DependencyNode GetOrAddNode(string name)
        {
            if (map.ContainsKey(name))
                return map[name];
            var node = new DependencyNode(name);
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

            BuildPaths(startNode, path, inPath, paths);

            foreach (var p in paths)
            {
                p.Reverse();
            }

            return paths;
        }


        private void BuildPaths(
            DependencyNode node,
            List<string> currentPath,
            HashSet<string> inCurrentPath,
            List<List<string>> paths)
        {
            if (inCurrentPath.Contains(node.Name))
            {
                // Circular reference, destroy the path.
                paths.Remove(currentPath);
                return;
            }
            currentPath.Add(node.Name);
            inCurrentPath.Add(node.Name);
            if (node.Neighbors.Count() == 0)
            {
                return;
            }
            else if (node.Neighbors.Count() == 1)
            {
                var neighbor = node.Neighbors.First().Value;
                BuildPaths(neighbor, currentPath, inCurrentPath, paths);
            }
            else
            {
                // Add every possible sort of neighbors
                var neighbors = node.Neighbors.Values.ToList();
                AddNeighbors(node, neighbors, currentPath, inCurrentPath, paths);
            }
        }

        private void AddNeighbors(
            DependencyNode node,
            List<DependencyNode> neighbors,
            List<string> currentPath,
            HashSet<string> inCurrentPath,
            List<List<string>> paths)
        {
            var currentPathAtStart = new List<string>(currentPath);
            var inCurrentPathAtStart = new HashSet<string>(inCurrentPath);

            var first = true;
            foreach (var neighbor in neighbors)
            {
                if (!inCurrentPathAtStart.Contains(neighbor.Name))
                {
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
                    newPath.Add(neighbor.Name);
                    inNewPath.Add(neighbor.Name);
                    AddNeighbors(neighbor, neighbors, newPath, inNewPath, paths);
                }
            }

            // If we've added all the neighbors to the path, start walking the tree again.
            if (first)
            {
                var last = node.Neighbors.Last().Key;
                foreach (var neighbor in node.Neighbors)
                {
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

                    BuildPaths(neighbor.Value, newPath, inNewPath, paths);
                }
            }
        }
    }

    public class DependencyNode
    {
        public string Name;
        public Dictionary<string, DependencyNode> Neighbors;

        public DependencyNode(string name)
        {
            Name = name;
            Neighbors = new Dictionary<string, DependencyNode>();
        }
    }
}
