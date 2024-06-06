using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    public class Node
    {
        public Vector2 position;
        public float gCost; // 从起点到该节点的代价
        public float hCost; // 该节点到终点的估计代价
        public Node parent;

        public float fCost
        {
            get { return gCost + hCost; }
        }

        public Node(Vector2 pos)
        {
            position = pos;
            gCost = float.MaxValue;
            hCost = 0;
            parent = null;
        }
    }

    public List<List<Vector2>> FindPath(Vector2 start, Vector2 target, List<Vector2> obstacles, int movePoints)
    {
        bool[,] walkable = CreateWalkableGrid(obstacles);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        Node startNode = new Node(start);
        startNode.gCost = 0;
        Node targetNode = new Node(target);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.position == targetNode.position)
            {
                List<Vector2> path = RetracePath(startNode, currentNode);
                return SplitPath(path, movePoints);
            }

            foreach (Node neighbor in GetNeighbors(currentNode, walkable))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float newCostToNeighbor = currentNode.gCost + Vector2.Distance(currentNode.position, neighbor.position);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = Vector2.Distance(neighbor.position, targetNode.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<List<Vector2>>(); // 没有找到路径
    }

    private bool[,] CreateWalkableGrid(List<Vector2> obstacles)
    {
        bool[,] grid = new bool[8, 8];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                grid[x, y] = true;
            }
        }

        foreach (Vector2 obstacle in obstacles)
        {
            grid[(int)obstacle.x, (int)obstacle.y] = false;
        }

        return grid;
    }

    private List<Node> GetNeighbors(Node node, bool[,] walkable)
    {
        List<Node> neighbors = new List<Node>();
        Vector2[] directions = new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0)
        };

        foreach (Vector2 direction in directions)
        {
            Vector2 neighborPos = node.position + direction;
            if (neighborPos.x >= 0 && neighborPos.x < walkable.GetLength(0) && neighborPos.y >= 0 && neighborPos.y < walkable.GetLength(1) && walkable[(int)neighborPos.x, (int)neighborPos.y])
            {
                neighbors.Add(new Node(neighborPos));
            }
        }

        return neighbors;
    }

    private List<Vector2> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private List<List<Vector2>> SplitPath(List<Vector2> path, int movePoints)
    {
        List<List<Vector2>> splitPath = new List<List<Vector2>>();
        List<Vector2> segment = new List<Vector2>();
        int moveCount = 0;

        foreach (var point in path)
        {
            if (moveCount >= movePoints)
            {
                splitPath.Add(new List<Vector2>(segment));
                segment.Clear();
                moveCount = 0;
            }
            segment.Add(point);
            moveCount++;
        }

        if (segment.Count > 0)
        {
            splitPath.Add(segment);
        }

        return splitPath;
    }
}

///////////////
//Pathfinding pathfinding = new Pathfinding();
//Vector2 start = new Vector2(0, 0);
//Vector2 target = new Vector2(7, 7);
//List<Vector2> obstacles = new List<Vector2>
//        {
//            new Vector2(3, 3),
//            new Vector2(3, 4),
//            new Vector2(3, 5)
//        };
//int movePoints = 2;

//List<List<Vector2>> pathSegments = pathfinding.FindPath(start, target, obstacles, movePoints);

//foreach (var segment in pathSegments)
//{
//    Debug.Log("Segment:");
//    foreach (var position in segment)
//    {
//        Debug.Log(position);
//    }
//}
///////////////