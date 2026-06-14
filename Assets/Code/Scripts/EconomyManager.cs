using UnityEngine;
using System;
using ZooTycoon.Data;

namespace ZooTycoon.Core
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private EconomyConfig config;

        public float Capital { get; private set; }

        public static event Action<float> OnCapitalChanged;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            Capital = config != null ? config.startingCapital : 0f;
        }

        private void Start()
        {
            OnCapitalChanged?.Invoke(Capital);
        }

        public bool CanAfford(float cost) => Capital >= cost;

        public void Spend(float cost)
        {
            Capital -= cost;
            OnCapitalChanged?.Invoke(Capital);
        }

        public void Earn(float amount)
        {
            Capital += amount;
            OnCapitalChanged?.Invoke(Capital);
        }
    }
}
