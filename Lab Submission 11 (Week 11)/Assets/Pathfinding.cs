using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PriorityQueue<TElement, TPriority>
{
    private class Node
    {
        public TElement Element;
        public TPriority Priority;
    }

    private List<Node> nodes = new List<Node>();
    private IComparer<TPriority> comparer;

    public int Count => nodes.Count;

    public PriorityQueue() : this(Comparer<TPriority>.Default) { }

    public PriorityQueue(IComparer<TPriority> comparer)
    {
        this.comparer = comparer;
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        nodes.Add(new Node { Element = element, Priority = priority });
    }

    public TElement Dequeue()
    {
        if (nodes.Count == 0)
        {
            throw new System.InvalidOperationException("Queue is empty.");
        }

        int bestIndex = 0;
        for (int i = 1; i < nodes.Count; i++)
        {
            if (comparer.Compare(nodes[i].Priority, nodes[bestIndex].Priority) < 0)
            {
                bestIndex = i;
            }
        }

        TElement bestElement = nodes[bestIndex].Element;
        nodes.RemoveAt(bestIndex);
        return bestElement;
    }
}

public class Pathfinding : MonoBehaviour
{
    private List<Vector2Int> path = new List<Vector2Int>();
    public Vector2Int start = new Vector2Int(0, 1);
    public Vector2Int goal = new Vector2Int(4, 4);
    private Vector2Int next;
    private Vector2Int current;

    private readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    [SerializeField] private int[,] grid = new int[,]
    {
        { 0, 1, 0, 0, 0 },
        { 0, 1, 0, 1, 0 },
        { 0, 0, 0, 1, 0 },
        { 0, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 0 }
    };

    private void Start()
    {
        GenerateRandomGrid(10, 10, 0.2f);
        SetStartAndGoalPositions(new Vector2Int(0, 0), new Vector2Int(9, 9));
    }

    private void OnDrawGizmos()
    {
        float cellSize = 1f;

        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = grid[y, x] == 1 ? Color.black : Color.white;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }

        foreach (var step in path)
        {
            Vector3 cellPosition = new Vector3(step.x * cellSize, 0, step.y * cellSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(start.x * cellSize, 0, start.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(goal.x * cellSize, 0, goal.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));
    }

    private bool IsInBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < grid.GetLength(1) && point.y >= 0 && point.y < grid.GetLength(0);
    }

    private void FindPath()
    {
        path.Clear();

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        Vector2Int current;
        Vector2Int next;

        bool pathFound = false;

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current == goal)
            {
                pathFound = true;
                break;
            }

            foreach (Vector2Int direction in directions)
            {
                next = current + direction;
                if (IsInBounds(next) && grid[next.y, next.x] == 0 && !cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!pathFound)
        {
            Debug.Log("Path not found with current obstacles.");
            return;
        }

        Vector2Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        path.Reverse();
    }

    public void GenerateRandomGrid(int width, int height, float obstacleProbability)
    {
        grid = new int[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Random.value < obstacleProbability)
                {
                    grid[y, x] = 1;
                }
                else
                {
                    grid[y, x] = 0;
                }
            }
        }
        if (IsInBounds(start)) grid[start.y, start.x] = 0;
        if (IsInBounds(goal)) grid[goal.y, goal.x] = 0;
    }

    public void AddObstacle(Vector2Int position)
    {
        if (IsInBounds(position))
        {
            if (position == start || position == goal)
            {
                Debug.LogWarning("Cannot place an obstacle on the start or goal position.");
                return;
            }

            if (grid[position.y, position.x] == 0)
            {
                grid[position.y, position.x] = 1;
                FindPath();
            }
        }
    }

    public void SetStartAndGoalPositions(Vector2Int newStart, Vector2Int newGoal)
    {
        if (IsInBounds(newStart) && IsInBounds(newGoal) && grid[newStart.y, newStart.x] == 0 && grid[newGoal.y, newGoal.x] == 0)
        {
            start = newStart;
            goal = newGoal;
            FindPath();
        }
        else
        {
            Debug.LogWarning("Start or Goal position is out of bounds or on an obstacle!");
        }
    }
}
