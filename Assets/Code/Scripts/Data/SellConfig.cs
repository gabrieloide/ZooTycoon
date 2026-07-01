using UnityEngine;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "SellConfig", menuName = "ZooTycoon/Config/Sell Config")]
    public class SellConfig : ScriptableObject
    {
        public float staledRefundRate = 0.75f;
    }
}
