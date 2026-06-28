using UnityEngine;
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

        [Header("State Variables")]
        [SerializeField] private FloatVariable capitalVariable;
        [SerializeField] private FloatVariable loanDebtVariable;

        [Header("Event Channels Out")]
        [SerializeField] private FloatEventChannelSO onCapitalChangedChannel;
        [SerializeField] private FloatEventChannelSO onLoanDebtChangedChannel;
        [SerializeField] private DailySummaryEventChannelSO onDailySummaryReadyChannel;

        public float Capital { get; private set; }
        public float LoanDebt { get; private set; }
        public float LoanInterestRate => config != null ? config.loanInterestRate : 0.1f;
        public EconomyConfig GetConfig() => config;

        private float dailyIncome;
        private float dailyBuildExpenses;
        private float dailyDisasterLosses;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            Capital = config != null ? config.startingCapital : 0f;
            capitalVariable?.SetValue(Capital);
            loanDebtVariable?.SetValue(0f);
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
            BroadcastCapital();
        }

        public bool CanAfford(float cost) => Capital >= cost;

        public void TakeLoan(float amount)
        {
            if (amount <= 0f) return;
            Capital += amount;
            LoanDebt += amount;
            BroadcastCapital();
            BroadcastLoanDebt();
        }

        public void Spend(float cost)
        {
            Capital -= cost;
            dailyBuildExpenses += cost;
            BroadcastCapital();
        }

        public void Earn(float amount)
        {
            Capital += amount;
            dailyIncome += amount;
            BroadcastCapital();
        }

        public void RegisterDisasterLoss(float amount) => dailyDisasterLosses += amount;

        private void BroadcastCapital()
        {
            capitalVariable?.SetValue(Capital);
            onCapitalChangedChannel?.Raise(Capital);
        }

        private void BroadcastLoanDebt()
        {
            loanDebtVariable?.SetValue(LoanDebt);
            onLoanDebtChangedChannel?.Raise(LoanDebt);
        }

        private void ResetDailyLedger()
        {
            ApplyLoanInterest();
            dailyIncome = 0f;
            dailyBuildExpenses = 0f;
            dailyDisasterLosses = 0f;
        }

        private void ApplyLoanInterest()
        {
            if (LoanDebt <= 0f) return;
            float rate = config != null ? config.loanInterestRate : 0.1f;
            float interest = LoanDebt * rate;
            LoanDebt += interest;
            Spend(interest);
            BroadcastLoanDebt();
        }

        private void CloseDailyLedger()
        {
            float maintenance = CalculateMaintenanceCost();
            if (maintenance > 0f) Spend(maintenance);

            float pureConstruction = dailyBuildExpenses - maintenance - dailyDisasterLosses;

            var summary = new DailySummaryData
            {
                income = dailyIncome,
                buildExpenses = Mathf.Max(0f, pureConstruction),
                maintenance = maintenance,
                disasterLosses = dailyDisasterLosses,
                net = dailyIncome - dailyBuildExpenses
            };

            onDailySummaryReadyChannel?.Raise(summary);
        }

        private float CalculateMaintenanceCost()
        {
            float total = 0f;
            foreach (var habitat in HabitatManager.GetAllHabitats())
                if (habitat != null && habitat.biome != null)
                    total += habitat.biome.dailyMaintenanceCost * habitat.GetTotalTiles();
            return total;
        }
    }
}
