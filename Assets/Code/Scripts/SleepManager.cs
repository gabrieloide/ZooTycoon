using UnityEngine;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance { get; private set; }

    [SerializeField] private float autoSleepStressPenalty = 25f;

    private bool voluntarySleptToday;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        TimeManager.onNewDayStart += OnNewDayStarted;
        TimeManager.onDayEnded += HandleAutoSleep;
    }

    private void OnDisable()
    {
        TimeManager.onNewDayStart -= OnNewDayStarted;
        TimeManager.onDayEnded -= HandleAutoSleep;
    }

    public void TrySleep()
    {
        if (voluntarySleptToday) return;
        voluntarySleptToday = true;
        ResetAllAnimalStress();
        TimeManager.Instance.ForceNewDay(false);
    }

    private void HandleAutoSleep()
    {
        if (voluntarySleptToday)
        {
            voluntarySleptToday = false;
            return;
        }

        ApplyStressPenalty();
        TimeManager.Instance.ForceNewDay(true);
    }

    private void OnNewDayStarted()
    {
        voluntarySleptToday = false;
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
