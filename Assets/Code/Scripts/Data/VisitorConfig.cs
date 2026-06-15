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

        [Header("Behavior")]
        public float satisfactionDecayRate = 1f;
        public float fleeRadius = 5f;
    }
}
