using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Day Duration")]
    [SerializeField] private float dayDurationInMinutes = 5f;

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

    public static Action onNewDayStart;
    public static Action onWorkDayStart;
    public static Action onWorkDayEnd;
    public static Action onDayEnded;

    private float dayTotalSeconds;
    private int currentDay;
    public float currentTime;

    private bool newDayStartFired;
    private bool workDayStartFired;
    private bool workDayEndFired;
    private bool dayEndedFired;

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

    public int GetCurrentDay() => currentDay;

    public Vector2Int GetCurrentTimeInDay()
    {
        if (dayTotalSeconds <= 0) return new Vector2Int(startDayHour, startDayMinute);

        int startTotal = startDayHour * 60 + startDayMinute;
        int endTotal = endDayHour * 60 + endDayMinute;
        if (endTotal <= startTotal) endTotal += 24 * 60;

        float progress = Mathf.Clamp01(currentTime / dayTotalSeconds);
        int elapsed = (int)(progress * (endTotal - startTotal));
        int now = startTotal + elapsed;

        return new Vector2Int((now / 60) % 24, now % 60);
    }

    public void ForceNewDay(bool skipToWorkStart = false)
    {
        ResetEventFlags();
        currentDay++;

        if (skipToWorkStart)
        {
            int startTotal = startDayHour * 60 + startDayMinute;
            int endTotal = endDayHour * 60 + endDayMinute;
            if (endTotal <= startTotal) endTotal += 24 * 60;
            int workStartTotal = startWorkHour * 60 + startWorkMinute;
            float workProgress = (float)(workStartTotal - startTotal) / (endTotal - startTotal);
            currentTime = dayTotalSeconds * workProgress;
            newDayStartFired = true;
        }
        else
        {
            currentTime = 0f;
        }
    }

    private void ResetEventFlags()
    {
        newDayStartFired = false;
        workDayStartFired = false;
        workDayEndFired = false;
        dayEndedFired = false;
    }

    private void CheckWorkDay()
    {
        var t = GetCurrentTimeInDay();

        if (!newDayStartFired && t.x == startDayHour && t.y == startDayMinute)
        {
            newDayStartFired = true;
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
        if (!dayEndedFired && t.x == endDayHour && t.y == endDayMinute)
        {
            dayEndedFired = true;
            onDayEnded?.Invoke();
        }
    }
}
