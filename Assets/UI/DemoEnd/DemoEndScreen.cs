using UnityEngine;
using UnityEngine.UIElements;
using ZooTycoon.Core;

public class DemoEndScreen : MonoBehaviour
{
    [Header("Event Channels In")]
    [SerializeField] private VoidEventChannelSO onDemoEndedChannel;

    private UIDocument uiDocument;
    private VisualElement screenDemoEnd;
    private Label lblFinalScore;

    private void Awake()
    {
        uiDocument = FindAnyObjectByType<UIDocument>();
    }

    private void Start()
    {
        BuildUI();
    }

    private void OnEnable()
    {
        onDemoEndedChannel?.Subscribe(ShowScreen);
    }

    private void OnDisable()
    {
        onDemoEndedChannel?.Unsubscribe(ShowScreen);
    }

    private void BuildUI()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        screenDemoEnd = new VisualElement();
        screenDemoEnd.style.position = Position.Absolute;
        screenDemoEnd.style.top = 0;
        screenDemoEnd.style.bottom = 0;
        screenDemoEnd.style.left = 0;
        screenDemoEnd.style.right = 0;
        screenDemoEnd.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.92f);
        screenDemoEnd.style.alignItems = Align.Center;
        screenDemoEnd.style.justifyContent = Justify.Center;
        screenDemoEnd.style.display = DisplayStyle.None;

        var card = new VisualElement();
        card.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.98f);
        card.style.paddingTop = 32;
        card.style.paddingBottom = 32;
        card.style.paddingLeft = 40;
        card.style.paddingRight = 40;
        card.style.borderTopLeftRadius = 10;
        card.style.borderTopRightRadius = 10;
        card.style.borderBottomLeftRadius = 10;
        card.style.borderBottomRightRadius = 10;
        card.style.alignItems = Align.Center;

        var title = new Label("DEMO COMPLETE");
        title.style.color = Color.white;
        title.style.fontSize = 26;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.marginBottom = 8;

        var subtitle = new Label("You survived 3 days at Panic Zoo");
        subtitle.style.color = new Color(0.75f, 0.75f, 0.75f);
        subtitle.style.fontSize = 13;
        subtitle.style.marginBottom = 20;

        var scoreLabel = new Label("FINAL SCORE");
        scoreLabel.style.color = new Color(0.8f, 0.8f, 0.5f);
        scoreLabel.style.fontSize = 13;
        scoreLabel.style.marginBottom = 4;

        lblFinalScore = new Label("$0");
        lblFinalScore.style.color = new Color(0.47f, 0.86f, 0.47f);
        lblFinalScore.style.fontSize = 34;
        lblFinalScore.style.unityFontStyleAndWeight = FontStyle.Bold;

        card.Add(title);
        card.Add(subtitle);
        card.Add(scoreLabel);
        card.Add(lblFinalScore);
        screenDemoEnd.Add(card);
        root.Add(screenDemoEnd);
    }

    private void ShowScreen()
    {
        if (screenDemoEnd == null) return;
        float capital = EconomyManager.Instance != null ? EconomyManager.Instance.Capital : 0f;
        lblFinalScore.text = $"${Mathf.RoundToInt(capital)}";
        lblFinalScore.style.color = capital >= 0f ? new Color(0.47f, 0.86f, 0.47f) : new Color(0.86f, 0.31f, 0.31f);
        screenDemoEnd.style.display = DisplayStyle.Flex;
    }
}
