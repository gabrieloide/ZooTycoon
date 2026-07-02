using UnityEngine;
using UnityEngine.UIElements;
using ZooTycoon.Core;

public class DayStartScreen : MonoBehaviour
{
    [Header("Time Channels")]
    [SerializeField] private VoidEventChannelSO onNewDayStartChannel;

    private UIDocument uiDocument;
    private VisualElement screenDayStart;
    private Label lblDay;
    private Label lblBreakeven;
    private Label lblQuota;
    private Button btnStartDay;

    private void Awake()
    {
        uiDocument = FindAnyObjectByType<UIDocument>();
    }

    private void Start()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        screenDayStart = root.Q<VisualElement>("screen-day-start");
        if (screenDayStart == null) return;

        lblDay = screenDayStart.Q<Label>("lbl-daystart-day");
        lblBreakeven = screenDayStart.Q<Label>("lbl-daystart-breakeven");
        lblQuota = screenDayStart.Q<Label>("lbl-daystart-quota");
        btnStartDay = screenDayStart.Q<Button>("btn-start-day");

        if (btnStartDay != null) btnStartDay.clicked += OnStartDayClicked;

        screenDayStart.style.display = DisplayStyle.None;
    }

    private void OnEnable()
    {
        onNewDayStartChannel?.Subscribe(ShowScreen);
    }

    private void OnDisable()
    {
        onNewDayStartChannel?.Unsubscribe(ShowScreen);
        if (btnStartDay != null) btnStartDay.clicked -= OnStartDayClicked;
    }

    private void ShowScreen()
    {
        if (screenDayStart == null) return;

        int day = TimeManager.Instance != null ? TimeManager.Instance.GetCurrentDay() : 0;
        if (lblDay != null) lblDay.text = $"DAY {day + 1}";

        float breakeven = EconomyManager.Instance != null ? EconomyManager.Instance.GetDailyBreakeven() : 0f;
        if (lblBreakeven != null) lblBreakeven.text = $"${Mathf.RoundToInt(breakeven)}";

        int quota = VisitorManager.Instance != null ? VisitorManager.Instance.GetVisitorQuota() : 0;
        if (lblQuota != null) lblQuota.text = $"{quota} visitors";

        screenDayStart.style.display = DisplayStyle.Flex;
    }

    private void OnStartDayClicked()
    {
        if (screenDayStart != null) screenDayStart.style.display = DisplayStyle.None;
        TimeManager.Instance?.BeginDay();
    }
}
