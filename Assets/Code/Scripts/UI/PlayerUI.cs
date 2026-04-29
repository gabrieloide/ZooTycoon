using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Day/Time")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;

    private void Start()
    {
        TimeManager.onDayChanged += UpdateDayDisplay;
        UpdateDayDisplay();
    }

    private void OnDestroy()
    {
        TimeManager.onDayChanged -= UpdateDayDisplay;
    }

    private void Update()
    {
        UpdateClockDisplay();
    }

    private void UpdateClockDisplay()
    {
        if (TimeManager.Instance != null)
        {
            Vector2Int timeInDay = TimeManager.Instance.GetCurrentTimeInDay();
            timeText.text = string.Format("{0:00}:{1:00}", timeInDay.x, timeInDay.y);
        }
    }

    private void UpdateDayDisplay()
    {
        if (TimeManager.Instance != null)
        {
            dayText.text = $"Day: {TimeManager.Instance.GetCurrentDay()}";
        }
    }
}