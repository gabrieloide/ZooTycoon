using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Data;

public class HabitatSpace : MonoBehaviour
{
    [Header("Identification")]
    public int id;
    public BiomeDefinition biome;

    [Header("Grid Dimensions")]
    public int xMin;
    public int xMax;
    public int yMin;
    public int yMax;

    [Header("Occupants")]
    public int currentOcupation;
    public int maxOcupation;
    public float generalAnnoyance;
    public CompatibilityMatrix globalMatrix;

    private List<Animal> animals = new();

    public void AddAnimal(AnimalData animal)
    {
        if (currentOcupation < maxOcupation)
        {
            var instance = Instantiate(animal.animalPrefab, transform);
            var animalComponent = instance.GetComponent<Animal>();
            animalComponent.data = animal;
            animalComponent.habitat = this;
            animals.Add(animalComponent);
            currentOcupation = animals.Count;
            UpdateTemperament();
        }
    }

    public void RemoveAnimal(Animal animal)
    {
        if (animals.Contains(animal))
        {
            animals.Remove(animal);
            currentOcupation = animals.Count;
            Destroy(animal.gameObject);
            UpdateTemperament();
        }
    }

    public void RemoveAnimal()
    {
        if (animals.Count > 0)
        {
            Animal animal = animals[animals.Count - 1];
            animals.RemoveAt(animals.Count - 1);
            currentOcupation = animals.Count;
            Destroy(animal.gameObject);
            UpdateTemperament();
        }
    }

    public int GetTotalRequiredSpace()
    {
        int space = 0;
        foreach (var animal in animals)
        {
            if (animal != null && animal.data != null)
                space += animal.data.requiredSpacePerAnimal;
        }
        return space;
    }

    public int GetTotalTiles()
    {
        return (xMax - xMin + 1) * (yMax - yMin + 1);
    }

    public float CalculateCurrentTension()
    {
        if (globalMatrix == null) return 0f;

        float totalTension = 0f;
        int totalTiles = GetTotalTiles();
        int requiredSpace = GetTotalRequiredSpace();

        if (requiredSpace > totalTiles)
            totalTension += (requiredSpace - totalTiles) * globalMatrix.overcrowdingPenaltyPerTile;

        for (int i = 0; i < animals.Count; i++)
        {
            for (int j = i + 1; j < animals.Count; j++)
            {
                if (animals[i] == null || animals[j] == null) continue;
                AnimalData a = animals[i].data;
                AnimalData b = animals[j].data;
                if (a == null || b == null) continue;

                if (a.specificEnemies.Contains(b) || b.specificEnemies.Contains(a))
                    totalTension += globalMatrix.specificEnemyTension;
                else
                    totalTension += globalMatrix.GetTension(a.family, b.family);
            }
        }

        return totalTension;
    }

    private void Update()
    {
        UpdateTemperament();
    }

    public void UpdateTemperament()
    {
        generalAnnoyance = CalculateAnnoyance();
    }

    private float CalculateAnnoyance()
    {
        if (animals.Count == 0) return 0f;

        float totalAnnoyance = 0;
        foreach (var animal in animals)
        {
            if (animal != null)
                totalAnnoyance += animal.currentAnnoyance;
        }
        return totalAnnoyance / animals.Count;
    }
}
