using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Game.AI
{
    public static class AStarPathfinder
    {
        private static readonly Vector2Int[] Directions =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        };

        public static List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
        {
            if (TilemapNavGrid.Instance == null)
            {
                Debug.LogError("No TilemapNavGrid instance found!");
                return new List<Vector3>();
            }

            var grid = TilemapNavGrid.Instance;
            Vector2Int start = grid.WorldToCell(startWorld);
            Vector2Int end = grid.WorldToCell(endWorld);

            HashSet<Vector2Int> closedSet = new();
            PriorityQueue<Node> openSet = new();
            Dictionary<Vector2Int, Node> allNodes = new();

            Node startNode = GetOrCreate(allNodes, start);
            startNode.gCost = 0;
            startNode.hCost = Heuristic(start, end);
            openSet.Enqueue(startNode);

            while (openSet.Count > 0)
            {
                Node current = openSet.Dequeue();
                if (current.position == end)
                {
                    List<Vector3> path = ReconstructPath(allNodes, grid, start, end);
                    TilemapNavGrid.Instance?.SetLastPath(path);
                    return path;
                }


                closedSet.Add(current.position);

                foreach (var dir in Directions)
                {
                    Vector2Int neighborPos = current.position + dir;
                    if (!grid.IsWalkable(neighborPos) || closedSet.Contains(neighborPos))
                        continue;

                    float newG = current.gCost + Vector2Int.Distance(current.position, neighborPos);
                    Node neighbor = GetOrCreate(allNodes, neighborPos);

                    if (newG < neighbor.gCost)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = newG;
                        neighbor.hCost = Heuristic(neighborPos, end);
                        if (!openSet.Contains(neighbor))
                            openSet.Enqueue(neighbor);
                    }
                }
            }

            return new List<Vector3>(); // no path found
        }

        private static Node GetOrCreate(Dictionary<Vector2Int, Node> nodes, Vector2Int pos)
        {
            if (!nodes.TryGetValue(pos, out Node node))
            {
                node = new Node(pos);
                nodes[pos] = node;
            }
            return node;
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static List<Vector3> ReconstructPath(Dictionary<Vector2Int, Node> nodes, TilemapNavGrid grid, Vector2Int start, Vector2Int end)
        {
            List<Vector3> path = new();
            Node current = nodes[end];

            while (current != null && current.position != start)
            {
                path.Add(grid.CellToWorld(current.position));
                current = current.parent;
            }

            path.Reverse();
            return path;
        }

        private class Node : System.IComparable<Node>
        {
            public Vector2Int position;
            public float gCost = float.MaxValue;
            public float hCost = 0;
            public Node parent;

            public float fCost => gCost + hCost;

            public Node(Vector2Int pos) => position = pos;

            public int CompareTo(Node other) => fCost.CompareTo(other.fCost);
        }

        private class PriorityQueue<T> where T : System.IComparable<T>
        {
            private readonly List<T> elements = new();
            public int Count => elements.Count;

            public void Enqueue(T item)
            {
                elements.Add(item);
                elements.Sort();
            }

            public T Dequeue()
            {
                T item = elements[0];
                elements.RemoveAt(0);
                return item;
            }

            public bool Contains(T item) => elements.Contains(item);
        }
    }
}
