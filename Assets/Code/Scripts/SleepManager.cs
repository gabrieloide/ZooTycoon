using UnityEngine;
using ZooTycoon.Core;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance { get; private set; }

    [SerializeField] private float autoSleepStressPenalty = 25f;

    [Header("Time Channels")]
    [SerializeField] private VoidEventChannelSO onNewDayStartChannel;
    [SerializeField] private VoidEventChannelSO onWorkDayEndChannel;
    [SerializeField] private VoidEventChannelSO onDayEndedChannel;

    [Header("Demo")]
    [SerializeField] private int demoDayLimit = 3;
    [SerializeField] private VoidEventChannelSO onDemoEndedChannel;

    private bool voluntarySleptToday;
    private bool canSleep;
    private bool demoEnded;
    private bool pendingSleep;
    private bool pendingSleepForced;

    public bool CanSleep => canSleep;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        onNewDayStartChannel?.Subscribe(OnNewDayStarted);
        onWorkDayEndChannel?.Subscribe(OnWorkDayEnded);
        onDayEndedChannel?.Subscribe(HandleAutoSleep);
    }

    private void OnDisable()
    {
        onNewDayStartChannel?.Unsubscribe(OnNewDayStarted);
        onWorkDayEndChannel?.Unsubscribe(OnWorkDayEnded);
        onDayEndedChannel?.Unsubscribe(HandleAutoSleep);
    }

    public void TrySleep()
    {
        if (!canSleep) return;
        if (voluntarySleptToday) return;
        if (demoEnded) return;
        if (pendingSleep) return;
        voluntarySleptToday = true;
        pendingSleep = true;
        pendingSleepForced = false;
        EconomyManager.Instance?.PublishDailySummary();
    }

    private void OnWorkDayEnded()
    {
        canSleep = true;
    }

    private void HandleAutoSleep()
    {
        if (voluntarySleptToday)
        {
            voluntarySleptToday = false;
            return;
        }
        if (demoEnded) return;
        if (pendingSleep) return;
        pendingSleep = true;
        pendingSleepForced = true;
        EconomyManager.Instance?.PublishDailySummary();
    }

    public void ConfirmNextDay()
    {
        if (!pendingSleep) return;
        if (demoEnded) return;
        pendingSleep = false;

        if (pendingSleepForced) ApplyStressPenalty();
        else ResetAllAnimalStress();

        if (IsFinalDemoDay())
        {
            EndDemo();
            return;
        }

        TimeManager.Instance.ForceNewDay(pendingSleepForced);
        SaveManager.Instance.Save();
    }

    private bool IsFinalDemoDay()
    {
        return TimeManager.Instance != null && TimeManager.Instance.GetCurrentDay() >= demoDayLimit - 1;
    }

    private void EndDemo()
    {
        demoEnded = true;
        Time.timeScale = 0f;
        onDemoEndedChannel?.Raise();
    }

    private void OnNewDayStarted()
    {
        voluntarySleptToday = false;
        canSleep = false;
    }

    private void ResetAllAnimalStress()
    {
        foreach (var habitat in HabitatManager.GetAllHabitats())
            habitat?.ResetAnimalStress();
    }

    private void ApplyStressPenalty()
    {
        foreach (var habitat in HabitatManager.GetAllHabitats())
            habitat?.AddStressToAll(autoSleepStressPenalty);
    }
}
