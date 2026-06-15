using UnityEngine;
using System.Collections.Generic;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "NewLicense", menuName = "ZooTycoon/Data/License")]
    public class LicenseData : ScriptableObject
    {
        [Header("Identity")]
        public string licenseID;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Economy")]
        public int cost;

        [Header("Unlocks")]
        public List<BiomeDefinition> unlockedBiomes = new();
    }
}
