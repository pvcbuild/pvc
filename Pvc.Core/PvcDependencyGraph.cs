using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcDependencyGraph
    {
        private Dictionary<string, Node> map;
        private string getPathSource;

        public PvcDependencyGraph()
        {
            map = new Dictionary<string, Node>();
        }

        public void AddDependencies(string from, IEnumerable<string> dependencies = null)
        {
            var f = GetOrAddNode(from);

            if (dependencies == null)
                return;

            foreach (string to in dependencies)
            {
                var toNode = GetOrAddNode(to);
                f.Neighbors.Add(to, toNode);
                toNode.NeighborsOf.Add(from, f);
            }
        }

        public void AddDependency(string from, string to)
        {
            GetOrAddNode(from).Neighbors.Add(to, GetOrAddNode(to));
            GetOrAddNode(to).NeighborsOf.Add(from, GetOrAddNode(from));
        }

        public List<string> GetDependencies(string node)
        {
            return GetOrAddNode(node)
                .Neighbors
                .Values
                .Select(n => n.Name)
                .ToList<string>();
        }

        private Node GetOrAddNode(string name)
        {
            if (map.ContainsKey(name))
                return map[name];
            var node = new Node(name);
            map.Add(name, node);
            return node;
        }

        public List<List<string>> GetPaths(string name)
        {
            this.getPathSource = name;
            var startNode = GetOrAddNode(name);
            var filteredMap = FilterMap(startNode);
            var paths = ExpandTiers(BuildTiers(filteredMap));
            return paths;
        }

        private void FindCircularPath(List<Node> path, Node node)
        {
            if (path.Contains(node))
            {
                path.Add(node);
                var errorPath = path.Select(x => x.Name).ToList<string>();
                throw new PvcCircularDependencyException(errorPath, "A circular dependency was found");
            }
            path.Add(node);
            foreach(var neighbor in node.Neighbors)
            {
                var newPath = new List<Node>(path);
                FindCircularPath(newPath, neighbor.Value);
            }
        }

        private void GraphToHashSet(HashSet<Node> found, Node node)
        {
            if (found.Contains(node))
            {
                // This is a circular path. Find the circular path and throw it
                var path = new List<Node>();
                FindCircularPath(path, GetOrAddNode(getPathSource));
            }
            found.Add(node);
            foreach(var neighbor in node.Neighbors)
            {
                GraphToHashSet(found, neighbor.Value);
            }
        }

        private Dictionary<string, Node> FilterMap(Node startNode)
        {
            var filteredMap = new Dictionary<string, Node>(this.map);
            var foundNodes = new HashSet<Node>();
            GraphToHashSet(foundNodes, startNode);

            foreach(var i in filteredMap.ToList())
            {
                if (!foundNodes.Contains(i.Value))
                    filteredMap.Remove(i.Key);
            }

            return filteredMap;
        }

        private List<List<string>> ExpandTiers(List<List<Node>> tiers)
        {
            var paths = new List<List<Node>>();
            paths.Add(new List<Node>());

            for (int i = 0; i < tiers.Count; i++ )
            {
                foreach(var path in paths.ToList())
                {
                    if (tiers[i].Count == 1)
                    {
                        path.Add(tiers[i][0]);
                    }
                    else if (tiers[i].Count > 4) // Avoid crazy factorial expansion.
                    {
                        path.AddRange(tiers[i]);
                    }
                    else
                    {
                        paths.Remove(path);
                        var permutations = new List<List<Node>>();
                        GetPermutations(path, permutations, tiers[i], tiers[i].Count);
                        paths.AddRange(permutations);
                    }
                }
            }

            var stringPaths = NodeToStringPaths(paths);
            return stringPaths;
        }

        private void GetPermutations(List<Node> basePath, List<List<Node>> permutations, List<Node> tier, int n)
        {
            // Heap's algorithm - http://en.wikipedia.org/wiki/Heap's_algorithm

            Node swap;
            if (n == 1)
            {
                var newPath = new List<Node>(basePath);
                newPath.AddRange(tier);
                permutations.Add(newPath);
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    GetPermutations(basePath, permutations, tier, n - 1);
                    if (n % 2 == 1)
                    {
                        swap = tier[0];
                        tier[0] = tier[n - 1];
                    }
                    else
                    {
                        swap = tier[i];
                        tier[i] = tier[n - 1];
                    }
                    tier[n - 1] = swap;
                }
            }
        }

        private List<List<string>> NodeToStringPaths(List<List<Node>> nodePaths)
        {
            var paths = new List<List<string>>();
            foreach(var nodePath in nodePaths)
            {
                var path = new List<string>();
                foreach(var node in nodePath)
                {
                    path.Add(node.Name);
                }
                paths.Add(path);
            }
            return paths;
        }

        private List<List<Node>> BuildTiers(Dictionary<string, Node> filteredMap)
        {
            var nodesToAdd = filteredMap.Values.ToList();
            var endNodes = FindEndNodes(filteredMap);
            var tiers = new List<List<Node>>();
            tiers.Add(endNodes);
            nodesToAdd.RemoveAll((x) => endNodes.Contains(x));

            var added = new HashSet<Node>(endNodes);

            var tier = 0;
            while (nodesToAdd.Count > 0)
            {
                var tierNodes = new List<Node>();
                foreach (var node in tiers[tier])
                {
                    foreach (var parent in node.NeighborsOf)
                    {
                        if (!added.Contains(parent.Value))
                        {
                            var allDepsAdded = true;
                            foreach(var neighbor in parent.Value.Neighbors)
                            {
                                if (!added.Contains(neighbor.Value))
                                {
                                    allDepsAdded = false;
                                    break;
                                }
                            }
                            if (allDepsAdded)
                            {
                                tierNodes.Add(parent.Value);
                                added.Add(parent.Value);
                                nodesToAdd.Remove(parent.Value);
                            }
                        }
                    }
                }
                tiers.Add(tierNodes);
                tier++;
            }

            return tiers;
        }

        private List<Node> FindEndNodes(Dictionary<string,Node> filteredMap)
        {
            var endNodes = new List<Node>();

            foreach(var node in filteredMap)
            {
                if (node.Value.Neighbors.Count == 0 && node.Value.NeighborsOf.Count >= 0)
                    endNodes.Add(node.Value);
            }

            return endNodes;
        }


        private class Node
        {
            public string Name;
            public Dictionary<string, Node> Neighbors;
            public Dictionary<string, Node> NeighborsOf;

            public Node(string name)
            {
                Name = name;
                Neighbors = new Dictionary<string, Node>();
                NeighborsOf = new Dictionary<string, Node>();
            }
        }
    }
}
