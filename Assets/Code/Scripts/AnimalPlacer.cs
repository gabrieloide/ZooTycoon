using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class AnimalPlacer : MonoBehaviour
{
    public void TryPlace(Vector2 gridPos, AnimalData data)
    {
        if (data == null) return;
        var habitat = HabitatManager.GetHabitatAtPosition(gridPos);
        if (habitat == null) return;
        if (habitat.currentOcupation >= habitat.maxOcupation) return;
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(data.purchaseCost)) return;
        EconomyManager.Instance.Spend(data.purchaseCost);
        habitat.AddAnimal(data);
    }
}
