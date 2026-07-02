using UnityEngine;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "VisitorConfig", menuName = "ZooTycoon/Config/Visitor Config")]
    public class VisitorConfig : ScriptableObject
    {
        [Header("Spawning")]
        public float spawnInterval = 10f;
        public int maxVisitors = 30;
        public int visitorsPerAnimal = 2;

        [Header("NPC Pool")]
        public int maxNpcVisitors = 8;

        [Header("Behavior")]
        public float satisfactionDecayRate = 1f;
        public float fleeRadius = 5f;
        public float fleeSpeedMultiplier = 1.6f;
        public float fleeDuration = 3f;

        [Header("Growth Quota")]
        public int visitorQuotaBase = 12;
        public int visitorQuotaGrowthPerDay = 4;
    }
}
