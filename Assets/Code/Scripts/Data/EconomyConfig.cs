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

        [Header("Office Rent")]
        public float dailyRentBase = 100f;
        public float dailyRentGrowthPerDay = 30f;
    }
}
