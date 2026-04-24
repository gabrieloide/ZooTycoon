using System.Collections.Generic;

public static class HabitadManager
{
    private static Dictionary<int, HabitadSpace> habitats = new Dictionary<int, HabitadSpace>();

    public static HabitadSpace GetHabitats(int id)
    {
        return habitats[id];
    }
    public static void AddHabitad(HabitadSpace habitad)
    {
        habitats.Add(habitad.id, habitad);
    }
    public static void RemoveHabitad(int id)
    {
        habitats.Remove(id);
    }
    public static int GetNextId()
    {
        return habitats.Count + 1;
    }
}