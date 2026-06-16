using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    [SerializeField] private readonly float dayDurationInMinutes = 5;

    /// <summary>
    /// This is the time when the day starts and ends
    /// </summary>
    [Header("Complete Day")]
    [SerializeField] private readonly int startDayHour = 6;
    [SerializeField] private readonly int startDayMinute = 0;
    [SerializeField] private readonly int endDayHour = 22;
    [SerializeField] private readonly int endDayMinute = 0;


    /// <summary>
    /// This is the time when the zoo work day starts and ends
    /// </summary>
    [Header("Work Day")]
    [SerializeField] private readonly int startWorkHour = 8;
    [SerializeField] private readonly int startWorkMinute = 0;
    [SerializeField] private readonly int endWorkHour = 18;
    [SerializeField] private readonly int endWorkMinute = 0;


    public static Action onDayChanged;
    public static Action onWorkDayStart;
    public static Action onNewDayStart;
    public static Action onWorkDayEnd;
    public static Action onDayEnded;
    private float dayTotalSeconds;
    private int currentDay;
    public float currentTime;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        MinutesToSeconds();
    }

    private void OnEnable() 
    {
        onDayChanged += OnNextDay;
    }

    private void OnDisable() 
    {
        onDayChanged -= OnNextDay;
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        CheckWorkDay();

    }
    public void MinutesToSeconds() => dayTotalSeconds = dayDurationInMinutes * 60;
    public void OnNextDay()
    {
        currentTime = 0;
        currentDay++;
   
    }
    public int GetCurrentDay() => currentDay;
    public Vector2Int GetCurrentTimeInDay()
    {
        if (dayTotalSeconds <= 0) return new Vector2Int(startDayHour, startDayMinute);

        int startTotalMinutes = startDayHour * 60 + startDayMinute;
        int endTotalMinutes = endDayHour * 60 + endDayMinute;

        if (endTotalMinutes <= startTotalMinutes)
            endTotalMinutes += 24 * 60;

        int totalOperatingMinutes = endTotalMinutes - startTotalMinutes;

        float progress = Mathf.Clamp01(currentTime / dayTotalSeconds);

        int elapsedGameMinutes = (int)(progress * totalOperatingMinutes);
        int currentTotalMinutes = startTotalMinutes + elapsedGameMinutes;

        int hours = (currentTotalMinutes / 60) % 24;
        int minutes = currentTotalMinutes % 60;

        return new Vector2Int(hours, minutes);
    }
    private void CheckWorkDay()
    {
        var time = GetCurrentTimeInDay();
        switch (time)
        {
            case var t when t.x == startDayHour && t.y == startDayMinute:
                onNewDayStart?.Invoke();
                break;
            case var t when t.x == startWorkHour && t.y == startWorkMinute:
                onWorkDayStart?.Invoke();
                break;
            case var t when t.x == endWorkHour && t.y == endWorkMinute:
                onWorkDayEnd?.Invoke();
                break;
            case var t when t.x == endDayHour && t.y == endDayMinute:
                onDayChanged?.Invoke();
                break;
        }
    }
}
