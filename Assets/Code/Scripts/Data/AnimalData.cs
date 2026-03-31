using UnityEngine;
using System.Collections.Generic;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "NewAnimalData", menuName = "ZooTycoon/Data/Animal Data")]
    public class AnimalData : ScriptableObject
    {
        [Header("Basic Info")]
        public string animalID;
        public string displayName;
        [TextArea(3, 5)] 
        public string description;

        [Header("Economy")]
        public int purchaseCost = 1000;
        public int appealRating = 10;

        [Header("Habitat Needs")]
        public int requiredSpacePerAnimal = 4;
        public AnimalFamily family;

        [Header("Exceptions")]
        public List<AnimalData> specificEnemies = new List<AnimalData>();
        public List<AnimalData> specificFriends = new List<AnimalData>();
    }
}
