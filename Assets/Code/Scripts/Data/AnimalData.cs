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
        public GameObject animalPrefab;
        [TextArea(3, 5)]
        public string description;

        [Header("Economy")]
        public int purchaseCost = 1000;

        [Header("Habitat Needs")]
        public int requiredSpacePerAnimal = 4;
        public AnimalFamily family;
        public AnimalNature nature;
        public BiomeDefinition requiredBiome;

        [Header("Stress")]
        public float stressAccumulationRate = 0.05f;
        public float annoyanceCalmRate = 1f;
        public float wrongBiomeStressRate = 5f;

        [Header("Movement")]
        public float wanderSpeed = 1.5f;
        public float wanderPauseMin = 1f;
        public float wanderPauseMax = 4f;
        public float wanderSpeedVariance = 0.2f;
        public float rotationSpeed = 5f;

        [Header("Chase (Escaped)")]
        public float chaseSpeed = 3.5f;
        public float chaseDetectionRadius = 8f;

        [Header("Interaction")]
        public float interactionRadius = 2f;
        public float calmStressReduction = 30f;
        public float calmStaminaCost = 20f;
        public float calmDurationRequired = 2f;

        [Header("Exceptions")]
        public List<AnimalData> specificEnemies = new();
        public List<AnimalData> specificFriends = new();
    }
}
