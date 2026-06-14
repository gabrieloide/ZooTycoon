using UnityEngine;

namespace ZooTycoon.Data
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "ZooTycoon/Config/Economy Config")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Capital")]
        public float startingCapital = 5000f;
        public float loanInterestRate = 0.1f;

        [Header("Income")]
        public int baseTicketPrice = 50;
    }
}
