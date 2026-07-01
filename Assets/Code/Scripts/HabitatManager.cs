using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Data;

public static class HabitatManager
{
    private readonly static Dictionary<int, HabitatSpace> habitats = new();

    public static HabitatSpace GetHabitat(int id)
    {
        return habitats[id];
    }
    public static void AddHabitat(HabitatSpace habitat)
    {
        habitats.Add(habitat.id, habitat);
    }
    public static void RemoveHabitat(int id)
    {
        habitats.Remove(id);
    }

    public static int GetNextId()
    {
        int id = 0;
        while (habitats.ContainsKey(id))
        {
            id++;
        }
        return id;
    }

    public static IReadOnlyCollection<HabitatSpace> GetAllHabitats() => habitats.Values;

    public static void Clear()
    {
        habitats.Clear();
    }

    public static HabitatSpace GetHabitatAtPosition(Vector2 gridPos)
    {
        foreach (var habitat in habitats.Values)
        {
            if (gridPos.x >= habitat.xMin && gridPos.x <= habitat.xMax &&
                gridPos.y >= habitat.yMin && gridPos.y <= habitat.yMax)
                return habitat;
        }
        return null;
    }

    public static bool IsTooCloseToAnyHabitat(int minX, int maxX, int minY, int maxY, int minDistance)
    {
        foreach (var h in habitats.Values)
        {
            int gapX = Mathf.Max(0, Mathf.Max(minX - h.xMax - 1, h.xMin - maxX - 1));
            int gapY = Mathf.Max(0, Mathf.Max(minY - h.yMax - 1, h.yMin - maxY - 1));
            if (gapX < minDistance && gapY < minDistance) return true;
        }
        return false;
    }

}