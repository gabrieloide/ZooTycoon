using UnityEngine;
using System.Collections.Generic;

namespace ZooTycoon.Data
{
    [System.Serializable]
    public struct FamilyConflict
    {
        public AnimalFamily familyA;
        public AnimalFamily familyB;

        [Range(0, 100)]
        public float tensionMultiplier;
    }

    [System.Serializable]
    public struct BiomeConflict
    {
        public BiomeDefinition biomeA;
        public BiomeDefinition biomeB;

        [Range(0, 200)]
        public float neighborTension;
    }

    [CreateAssetMenu(fileName = "CompatibilityMatrix", menuName = "ZooTycoon/Data/Compatibility Matrix")]
    public class CompatibilityMatrix : ScriptableObject
    {
        [Header("Global Family Rules")]
        public List<FamilyConflict> conflicts = new();

        [Header("Stress Constants")]
        public float overcrowdingPenaltyPerTile = 15f;
        public float specificEnemyTension = 100f;

        [Header("Neighbor Settings")]
        public int minHabitatDistance = 2;
        public int neighborInfluenceRadius = 4;
        public List<BiomeConflict> biomeConflicts = new();

        public float GetTension(AnimalFamily a, AnimalFamily b)
        {
            if (a == b) return 0f;

            foreach (var conflict in conflicts)
            {
                if ((conflict.familyA == a && conflict.familyB == b) ||
                    (conflict.familyA == b && conflict.familyB == a))
                {
                    return conflict.tensionMultiplier;
                }
            }

            return 0f;
        }

        public float GetBiomeTension(BiomeDefinition a, BiomeDefinition b)
        {
            if (a == null || b == null || a == b) return 0f;

            foreach (var conflict in biomeConflicts)
            {
                if ((conflict.biomeA == a && conflict.biomeB == b) ||
                    (conflict.biomeA == b && conflict.biomeB == a))
                    return conflict.neighborTension;
            }

            return 0f;
        }
    }
}
