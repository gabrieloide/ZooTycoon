using System.Collections.Generic;

public static class HabitadManager
{
    private static List<HabitadSpace> habitats = new List<HabitadSpace>();
    public static void AddHabitad(HabitadSpace habitad)
    {
        habitats.Add(habitad);
    }
    public static void RemoveHabitad(HabitadSpace habitad)
    {
        habitats.Remove(habitad);
    }
    public static int GetNextId()
    {
        return habitats.Count + 1;
    }
}