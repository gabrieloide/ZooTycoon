using UnityEngine;
using System.Collections.Generic;
using ZooTycoon.Data;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "NewAnimalData", menuName = "ZooTycoon/Data/Animal Data")]
    public class AnimalData : ScriptableObject
    {
        [Header("Basic Info")]
        public string animalID;
        public string displayName;
        public GameObject animalPrefab;
        [TextArea(3, 5)]
        public string description;
        [Header("Economy")]
        public int purchaseCost = 1000;

        [Header("Habitat Needs")]
        public int requiredSpacePerAnimal = 4;
        public AnimalFamily family;
        public AnimalNature nature;
        public HabitatSpace requiredHabitatType;

        [Header("Exceptions")]
        public List<AnimalData> specificEnemies = new();
        public List<AnimalData> specificFriends = new();
    }
}
