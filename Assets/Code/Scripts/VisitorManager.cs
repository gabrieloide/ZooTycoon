using System.Collections;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class VisitorManager : MonoBehaviour
{
    public static VisitorManager Instance { get; private set; }

    [SerializeField] private VisitorConfig visitorConfig;
    [SerializeField] private EconomyConfig economyConfig;

    [Header("Time Channels")]
    [SerializeField] private VoidEventChannelSO onWorkDayStartChannel;
    [SerializeField] private VoidEventChannelSO onWorkDayEndChannel;

    [Header("Event Channels Out")]
    [SerializeField] private IntEventChannelSO onVisitorCountChangedChannel;

    private Coroutine incomeCoroutine;
    private int currentVisitors;
    private int escapePenaltyVisitors;

    public int CurrentVisitors => currentVisitors;

    public int GetVisitorQuota()
    {
        if (visitorConfig == null) return 0;
        int day = TimeManager.Instance != null ? TimeManager.Instance.GetCurrentDay() : 0;
        return visitorConfig.visitorQuotaBase + visitorConfig.visitorQuotaGrowthPerDay * day;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        onWorkDayStartChannel?.Subscribe(StartIncome);
        onWorkDayEndChannel?.Subscribe(StopIncome);
    }

    private void OnDisable()
    {
        onWorkDayStartChannel?.Unsubscribe(StartIncome);
        onWorkDayEndChannel?.Unsubscribe(StopIncome);
    }

    public void ApplyEscapePenalty(int visitorCount)
    {
        escapePenaltyVisitors += visitorCount;
        BroadcastVisitorCount();
    }

    public void RemoveEscapePenalty(int visitorCount)
    {
        escapePenaltyVisitors = Mathf.Max(0, escapePenaltyVisitors - visitorCount);
        BroadcastVisitorCount();
    }

    private void StartIncome()
    {
        escapePenaltyVisitors = 0;
        if (incomeCoroutine != null) StopCoroutine(incomeCoroutine);
        incomeCoroutine = StartCoroutine(IncomeLoop());
    }

    private void StopIncome()
    {
        if (incomeCoroutine != null) { StopCoroutine(incomeCoroutine); incomeCoroutine = null; }
        currentVisitors = 0;
        escapePenaltyVisitors = 0;
        BroadcastVisitorCount();
    }

    private IEnumerator IncomeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(visitorConfig.spawnInterval);
            UpdateVisitorCount();
            CollectIncome();
        }
    }

    private void UpdateVisitorCount()
    {
        int totalAnimals = 0;
        foreach (var habitat in HabitatManager.GetAllHabitats())
            if (habitat != null) totalAnimals += habitat.currentOcupation;

        currentVisitors = Mathf.Max(0,
            Mathf.Min(totalAnimals * visitorConfig.visitorsPerAnimal, visitorConfig.maxVisitors)
            - escapePenaltyVisitors);
        BroadcastVisitorCount();
    }

    private void BroadcastVisitorCount() => onVisitorCountChangedChannel?.Raise(currentVisitors);

    private void CollectIncome()
    {
        if (currentVisitors <= 0 || EconomyManager.Instance == null || economyConfig == null) return;
        EconomyManager.Instance.Earn(currentVisitors * economyConfig.baseTicketPrice);
    }
}
