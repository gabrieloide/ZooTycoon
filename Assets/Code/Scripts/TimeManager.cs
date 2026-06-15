using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float dayDurationInMinutes = 5;

    [Header("Complete Day")]
    [SerializeField] private int startDayHour = 6;
    [SerializeField] private int startDayMinute = 0;
    [SerializeField] private int endDayHour = 22;
    [SerializeField] private int endDayMinute = 0;

    [Header("Work Day")]
    [SerializeField] private int startWorkHour = 8;
    [SerializeField] private int startWorkMinute = 0;
    [SerializeField] private int endWorkHour = 18;
    [SerializeField] private int endWorkMinute = 0;

    public static Action onDayEnded;
    public static Action onNewDayStart;
    public static Action onWorkDayStart;
    public static Action onWorkDayEnd;

    private float dayTotalSeconds;
    private int currentDay;
    private float currentTime;

    private bool newDayFired;
    private bool workDayStartFired;
    private bool workDayEndFired;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        dayTotalSeconds = dayDurationInMinutes * 60f;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        CheckWorkDay();
    }

    public void OnNextDay()
    {
        currentTime = 0f;
        currentDay++;
        newDayFired = false;
        workDayStartFired = false;
        workDayEndFired = false;
        onDayEnded?.Invoke();
    }

    public int GetCurrentDay() => currentDay;

    public Vector2Int GetCurrentTimeInDay()
    {
        if (dayTotalSeconds <= 0) return new Vector2Int(startDayHour, startDayMinute);

        int startTotal = startDayHour * 60 + startDayMinute;
        int endTotal = endDayHour * 60 + endDayMinute;
        if (endTotal <= startTotal) endTotal += 24 * 60;

        float progress = Mathf.Clamp01(currentTime / dayTotalSeconds);
        int elapsed = (int)(progress * (endTotal - startTotal));
        int current = startTotal + elapsed;

        return new Vector2Int((current / 60) % 24, current % 60);
    }

    private void CheckWorkDay()
    {
        var t = GetCurrentTimeInDay();

        if (!newDayFired && t.x == startDayHour && t.y == startDayMinute)
        {
            newDayFired = true;
            onNewDayStart?.Invoke();
        }

        if (!workDayStartFired && t.x == startWorkHour && t.y == startWorkMinute)
        {
            workDayStartFired = true;
            onWorkDayStart?.Invoke();
        }

        if (!workDayEndFired && t.x == endWorkHour && t.y == endWorkMinute)
        {
            workDayEndFired = true;
            onWorkDayEnd?.Invoke();
        }

        if (t.x == endDayHour && t.y == endDayMinute)
            OnNextDay();
    }
}
