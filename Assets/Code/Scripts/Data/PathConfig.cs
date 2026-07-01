using UnityEngine;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "PathConfig", menuName = "ZooTycoon/Config/Path Config")]
    public class PathConfig : ScriptableObject
    {
        [Header("Economy")]
        public float costPerTile = 10f;

        [Header("Visual")]
        [ColorUsage(false)] public Color pathColor = new Color(0.76f, 0.70f, 0.50f, 1f);
    }
}
