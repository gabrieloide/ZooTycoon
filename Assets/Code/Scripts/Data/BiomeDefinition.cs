using UnityEngine;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "NewBiome", menuName = "ZooTycoon/Data/Biome Definition")]
    public class BiomeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string biomeID;
        public string displayName;
        public Sprite icon;

        [Header("Economy")]
        public int buildCost;
        public int dailyMaintenanceCost;
    }
}
