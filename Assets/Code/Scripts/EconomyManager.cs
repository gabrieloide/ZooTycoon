using UnityEngine;
using System;
using ZooTycoon.Core;
using ZooTycoon.Data;

namespace ZooTycoon.Core
{
    public struct DailySummaryData
    {
        public float income;
        public float buildExpenses;
        public float maintenance;
        public float disasterLosses;
        public float net;
    }

    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private EconomyConfig config;

        [Header("Time Channels")]
        [SerializeField] private VoidEventChannelSO onNewDayStartChannel;
        [SerializeField] private VoidEventChannelSO onWorkDayEndChannel;

        public float Capital { get; private set; }
        public float LoanDebt { get; private set; }
        public float LoanInterestRate => config != null ? config.loanInterestRate : 0.1f;
        public EconomyConfig GetConfig() => config;

        public static event Action<float> OnCapitalChanged;
        public static event Action<DailySummaryData> OnDailySummaryReady;
        public static event Action<float> OnLoanDebtChanged;

        private float dailyIncome;
        private float dailyBuildExpenses;
        private float dailyDisasterLosses;
        private float dailyLoanInterest;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            Capital = config != null ? config.startingCapital : 0f;
        }

        private void OnEnable()
        {
            onNewDayStartChannel?.Subscribe(ResetDailyLedger);
            onWorkDayEndChannel?.Subscribe(CloseDailyLedger);
        }

        private void OnDisable()
        {
            onNewDayStartChannel?.Unsubscribe(ResetDailyLedger);
            onWorkDayEndChannel?.Unsubscribe(CloseDailyLedger);
        }

        private void Start()
        {
            OnCapitalChanged?.Invoke(Capital);
        }

        public bool CanAfford(float cost) => Capital >= cost;

        public void TakeLoan(float amount)
        {
            if (amount <= 0f) return;
            Capital += amount;
            LoanDebt += amount;
            OnCapitalChanged?.Invoke(Capital);
            OnLoanDebtChanged?.Invoke(LoanDebt);
        }

        public void Spend(float cost)
        {
            Capital -= cost;
            dailyBuildExpenses += cost;
            OnCapitalChanged?.Invoke(Capital);
        }

        public void Earn(float amount)
        {
            Capital += amount;
            dailyIncome += amount;
            OnCapitalChanged?.Invoke(Capital);
        }

        public void RegisterDisasterLoss(float amount)
        {
            dailyDisasterLosses += amount;
        }

        private void ResetDailyLedger()
        {
            ApplyLoanInterest();
            dailyIncome = 0f;
            dailyBuildExpenses = 0f;
            dailyDisasterLosses = 0f;
            dailyLoanInterest = 0f;
        }

        private void ApplyLoanInterest()
        {
            if (LoanDebt <= 0f) return;
            float rate = config != null ? config.loanInterestRate : 0.1f;
            float interest = LoanDebt * rate;
            LoanDebt += interest;
            dailyLoanInterest = interest;
            Spend(interest);
            OnLoanDebtChanged?.Invoke(LoanDebt);
        }

        private void CloseDailyLedger()
        {
            float maintenance = CalculateMaintenanceCost();
            if (maintenance > 0f)
                Spend(maintenance);

            float pureConstruction = dailyBuildExpenses - maintenance - dailyDisasterLosses;

            var summary = new DailySummaryData
            {
                income = dailyIncome,
                buildExpenses = Mathf.Max(0f, pureConstruction),
                maintenance = maintenance,
                disasterLosses = dailyDisasterLosses,
                net = dailyIncome - dailyBuildExpenses
            };

            OnDailySummaryReady?.Invoke(summary);
        }

        private float CalculateMaintenanceCost()
        {
            float total = 0f;
            foreach (var habitat in HabitatManager.GetAllHabitats())
            {
                if (habitat != null && habitat.biome != null)
                    total += habitat.biome.dailyMaintenanceCost * habitat.GetTotalTiles();
            }
            return total;
        }
    }
}
