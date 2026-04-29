using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    [SerializeField] private readonly float dayDurationInMinutes = 5;
    [SerializeField] private readonly int startHour = 8;
    [SerializeField] private readonly int startMinute = 0;
    [SerializeField] private readonly int endHour = 18;
    [SerializeField] private readonly int endMinute = 0;
    public static Action onDayChanged;
    private float dayTotalSeconds;
    private int currentDay;
    private float currentTime;


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

    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= dayTotalSeconds)
        {
            OnNextDay();
        }
    }
    public void MinutesToSeconds() => dayTotalSeconds = dayDurationInMinutes * 60;
    public void OnNextDay()
    {
        currentTime = 0;
        currentDay++;
        onDayChanged?.Invoke();
    }
    public int GetCurrentDay() => currentDay;
    public Vector2Int GetCurrentTimeInDay()
    {
        if (dayTotalSeconds <= 0) return new Vector2Int(startHour, startMinute);

        int startTotalMinutes = startHour * 60 + startMinute;
        int endTotalMinutes = endHour * 60 + endMinute;

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
}
