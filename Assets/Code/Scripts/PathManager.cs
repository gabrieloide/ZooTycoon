using System.Collections.Generic;
using UnityEngine;

public static class PathManager
{
    private static readonly Dictionary<Vector2, (float cost, int session)> cellData = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() => cellData.Clear();

    public static void Add(Vector2 cell, float cost, int session) => cellData[cell] = (cost, session);
    public static bool Has(Vector2 cell) => cellData.ContainsKey(cell);
    public static bool Remove(Vector2 cell) => cellData.Remove(cell);
    public static int Count => cellData.Count;
    public static IReadOnlyCollection<Vector2> GetAll() => cellData.Keys;

    public static float GetRefund(Vector2 cell, float staledRate, int currentSession)
    {
        if (!cellData.TryGetValue(cell, out var d)) return 0f;
        return d.cost * (d.session == currentSession ? 1f : staledRate);
    }

    public static bool TryGetData(Vector2 cell, out float cost, out int session)
    {
        if (cellData.TryGetValue(cell, out var d))
        {
            cost = d.cost;
            session = d.session;
            return true;
        }
        cost = 0f;
        session = 0;
        return false;
    }

    public static List<Vector2> FindPath(Vector2 start, Vector2 goal)
    {
        if (!cellData.ContainsKey(start) || !cellData.ContainsKey(goal)) return null;
        if (start == goal) return new List<Vector2> { start };

        var visited = new HashSet<Vector2>();
        var prev = new Dictionary<Vector2, Vector2>();
        var queue = new Queue<Vector2>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == goal)
            {
                var path = new List<Vector2>();
                var node = goal;
                while (node != start)
                {
                    path.Add(node);
                    node = prev[node];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;
                visited.Add(neighbor);
                prev[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        return null;
    }

    public static Vector2? NearestCell(Vector3 worldPos)
    {
        if (cellData.Count == 0) return null;
        float cs = GridCreator.Instance != null ? GridCreator.Instance.cellSize : 1f;
        var gridPos = new Vector2(
            Mathf.FloorToInt(worldPos.x / cs),
            Mathf.FloorToInt(worldPos.z / cs));

        float minDist = float.MaxValue;
        Vector2 nearest = default;
        bool found = false;
        foreach (var cell in cellData.Keys)
        {
            float d = Vector2.SqrMagnitude(cell - gridPos);
            if (d < minDist) { minDist = d; nearest = cell; found = true; }
        }
        return found ? nearest : (Vector2?)null;
    }

    public static List<Vector2> GetViewingCells(int xMin, int xMax, int yMin, int yMax)
    {
        var result = new List<Vector2>();
        for (int x = xMin; x <= xMax; x++)
        {
            TryAdd(result, new Vector2(x, yMin - 1));
            TryAdd(result, new Vector2(x, yMax + 1));
        }
        for (int y = yMin; y <= yMax; y++)
        {
            TryAdd(result, new Vector2(xMin - 1, y));
            TryAdd(result, new Vector2(xMax + 1, y));
        }
        return result;
    }

    private static void TryAdd(List<Vector2> list, Vector2 cell)
    {
        if (cellData.ContainsKey(cell)) list.Add(cell);
    }

    private static List<Vector2> GetNeighbors(Vector2 cell)
    {
        var result = new List<Vector2>(4);
        var dirs = new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (var d in dirs)
        {
            var n = cell + d;
            if (cellData.ContainsKey(n)) result.Add(n);
        }
        return result;
    }
}
