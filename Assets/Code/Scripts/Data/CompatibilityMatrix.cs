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

    [CreateAssetMenu(fileName = "CompatibilityMatrix", menuName = "ZooTycoon/Data/Compatibility Matrix")]
    public class CompatibilityMatrix : ScriptableObject
    {
        [Header("Global Family Rules")]
        public List<FamilyConflict> conflicts = new List<FamilyConflict>();

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
    }
}
