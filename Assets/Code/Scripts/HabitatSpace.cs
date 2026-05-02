using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Data;
public class HabitatSpace : MonoBehaviour
{
    [Header("Identification")]
    public int id;
    public string type;
    [Header("Grid Dimensions")]
    public int xMin;
    public int xMax;
    public int yMin;
    public int yMax;

    public int currentOcupation;
    public int maxOcupation;
    public float generalAnnoyance;
    private List<Animal> animals = new();

    public void AddAnimal(AnimalData animal)
    {
        if (currentOcupation < maxOcupation)
        {
            var instance = Instantiate(animal.animalPrefab, transform);
            instance.GetComponent<Animal>().data = animal;
            instance.GetComponent<Animal>().habitat = this;
            currentOcupation++;
            UpdateTemperament();
        }
    }

    public void RemoveAnimal()
    {
        if (currentOcupation > 0)
        {
            currentOcupation--;
            UpdateTemperament();
        }
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
        float totalAnnoyance = 0;
        foreach (var animal in animals)
        {
            totalAnnoyance += animal.currentAnnoyance;
        }
        return totalAnnoyance / animals.Count;
    }
}