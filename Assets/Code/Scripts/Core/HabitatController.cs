using UnityEngine;
using System.Collections.Generic;
using ZooTycoon.Data;

namespace ZooTycoon.Core
{
    public class HabitatController : MonoBehaviour
    {
        [Header("Grid Data")]
        public int totalTiles = 0;
        public int costPerTile = 100;

        [Header("Occupants")]
        public List<AnimalData> containedAnimals = new List<AnimalData>();
        public CompatibilityMatrix globalMatrix;

        public void InitializeGridSize(int tileCount)
        {
            totalTiles = tileCount;
            Debug.Log($"Habitat initialized with {totalTiles} tiles. Cost calculated: {totalTiles * costPerTile}");
        }

        public int GetTotalRequiredSpace()
        {
            int space = 0;
            foreach (var animal in containedAnimals)
            {
                space += animal.requiredSpacePerAnimal;
            }
            return space;
        }

        public bool CanAccommodateSpaceFor(AnimalData newAnimal)
        {
            return totalTiles >= (GetTotalRequiredSpace() + newAnimal.requiredSpacePerAnimal);
        }

        public void AddAnimal(AnimalData animal)
        {
            containedAnimals.Add(animal);
            Debug.Log($"Added {animal.displayName} to habitat.");
        }

        public float CalculateCurrentTension()
        {
            float totalTension = 0f;

            int requiredSpace = GetTotalRequiredSpace();
            if (requiredSpace > totalTiles)
            {
                totalTension += (requiredSpace - totalTiles) * 15f; 
            }

            for (int i = 0; i < containedAnimals.Count; i++)
            {
                for (int j = i + 1; j < containedAnimals.Count; j++)
                {
                    AnimalData a = containedAnimals[i];
                    AnimalData b = containedAnimals[j];

                    if (a.specificEnemies.Contains(b) || b.specificEnemies.Contains(a))
                    {
                        totalTension += 100f;
                    }
                    else if (globalMatrix != null)
                    {
                        totalTension += globalMatrix.GetTension(a.family, b.family);
                    }
                }
            }

            return totalTension;
        }
    }
}
