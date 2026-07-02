using UnityEngine;
using UnityEngine.UIElements;
using ZooTycoon.Core;

public class DailySummary : MonoBehaviour
{
    public static DailySummaryData LastSummary { get; private set; }

    [Header("Event Channels In")]
    [SerializeField] private DailySummaryEventChannelSO onDailySummaryReadyChannel;

    [Header("State Variables")]
    [SerializeField] private FloatVariable capitalVariable;
    [SerializeField] private FloatVariable loanDebtVariable;

    private UIDocument uiDocument;
    private VisualElement screenSummary;
    private VisualElement summaryCard;
    private Label lblDay;
    private Label lblIncome;
    private Label lblBuild;
    private Label lblMaint;
    private Label lblDisaster;
    private Label lblRent;
    private Label lblNet;
    private Button btnNextDay;

    private VisualElement loanSection;
    private VisualElement debtSection;
    private Label lblLoanAmount;
    private Label lblDebtActive;
    private Button btnAcceptLoan;

    private float pendingLoanAmount;

    private void Awake()
    {
        uiDocument = FindAnyObjectByType<UIDocument>();
    }

    private void Start()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        screenSummary = root.Q<VisualElement>("screen-summary");
        if (screenSummary == null) return;

        summaryCard = screenSummary.Q<VisualElement>("summary-card");
        lblDay      = screenSummary.Q<Label>("lbl-summary-day");
        lblIncome   = screenSummary.Q<Label>("lbl-income");
        lblBuild    = screenSummary.Q<Label>("lbl-build");
        lblMaint    = screenSummary.Q<Label>("lbl-maint");
        lblDisaster = screenSummary.Q<Label>("lbl-disaster");
        lblRent     = screenSummary.Q<Label>("lbl-rent");
        lblNet      = screenSummary.Q<Label>("lbl-net");
        btnNextDay  = screenSummary.Q<Button>("btn-next-day");

        if (btnNextDay != null) btnNextDay.clicked += OnNextDayClicked;

        BuildLoanUI();
        screenSummary.style.display = DisplayStyle.None;
    }

    private void OnEnable()
    {
        onDailySummaryReadyChannel?.Subscribe(ShowSummary);
    }

    private void OnDisable()
    {
        onDailySummaryReadyChannel?.Unsubscribe(ShowSummary);
        if (btnNextDay != null) btnNextDay.clicked -= OnNextDayClicked;
        if (btnAcceptLoan != null) btnAcceptLoan.clicked -= OnAcceptLoanClicked;
    }

    private void BuildLoanUI()
    {
        if (summaryCard == null) return;

        debtSection = new VisualElement();
        debtSection.style.flexDirection = FlexDirection.Row;
        debtSection.style.justifyContent = Justify.SpaceBetween;
        debtSection.style.marginBottom = 12;
        debtSection.style.display = DisplayStyle.None;
        var debtLabel = new Label("Active Loan Debt");
        debtLabel.style.color = new Color(1f, 0.65f, 0.2f); debtLabel.style.fontSize = 13;
        lblDebtActive = new Label("$0");
        lblDebtActive.style.color = new Color(1f, 0.65f, 0.2f); lblDebtActive.style.fontSize = 13;
        lblDebtActive.style.unityFontStyleAndWeight = FontStyle.Bold;
        debtSection.Add(debtLabel); debtSection.Add(lblDebtActive);

        loanSection = new VisualElement();
        loanSection.style.backgroundColor = new Color(0.55f, 0.08f, 0.08f, 0.6f);
        loanSection.style.borderTopLeftRadius = 8; loanSection.style.borderTopRightRadius = 8;
        loanSection.style.borderBottomLeftRadius = 8; loanSection.style.borderBottomRightRadius = 8;
        loanSection.style.paddingTop = 14; loanSection.style.paddingBottom = 14;
        loanSection.style.paddingLeft = 16; loanSection.style.paddingRight = 16;
        loanSection.style.marginBottom = 14; loanSection.style.display = DisplayStyle.None;

        var loanTitle = new Label("! NEGATIVE BALANCE");
        loanTitle.style.color = new Color(1f, 0.35f, 0.35f); loanTitle.style.fontSize = 12;
        loanTitle.style.unityFontStyleAndWeight = FontStyle.Bold; loanTitle.style.marginBottom = 6;

        lblLoanAmount = new Label();
        lblLoanAmount.style.color = Color.white; lblLoanAmount.style.fontSize = 13;
        lblLoanAmount.style.marginBottom = 4;

        float rate = EconomyManager.Instance != null && EconomyManager.Instance.GetConfig() != null
            ? EconomyManager.Instance.GetConfig().loanInterestRate * 100f : 10f;
        var loanInterestNote = new Label($"Daily interest: {rate:F0}% on total debt");
        loanInterestNote.style.color = new Color(0.75f, 0.75f, 0.75f); loanInterestNote.style.fontSize = 11;
        loanInterestNote.style.marginBottom = 10;

        btnAcceptLoan = new Button();
        btnAcceptLoan.text = "SIGN EMERGENCY LOAN";
        btnAcceptLoan.style.height = 36;
        btnAcceptLoan.style.backgroundColor = new Color(0.85f, 0.2f, 0.2f);
        btnAcceptLoan.style.color = Color.white; btnAcceptLoan.style.fontSize = 12;
        btnAcceptLoan.style.unityFontStyleAndWeight = FontStyle.Bold;
        btnAcceptLoan.style.borderTopLeftRadius = 5; btnAcceptLoan.style.borderTopRightRadius = 5;
        btnAcceptLoan.style.borderBottomLeftRadius = 5; btnAcceptLoan.style.borderBottomRightRadius = 5;
        btnAcceptLoan.style.borderTopWidth = 0; btnAcceptLoan.style.borderBottomWidth = 0;
        btnAcceptLoan.style.borderLeftWidth = 0; btnAcceptLoan.style.borderRightWidth = 0;
        btnAcceptLoan.clicked += OnAcceptLoanClicked;

        loanSection.Add(loanTitle); loanSection.Add(lblLoanAmount);
        loanSection.Add(loanInterestNote); loanSection.Add(btnAcceptLoan);

        summaryCard.Insert(summaryCard.IndexOf(btnNextDay), debtSection);
        summaryCard.Insert(summaryCard.IndexOf(btnNextDay), loanSection);
    }

    private void ShowSummary(DailySummaryData data)
    {
        LastSummary = data;
        if (screenSummary == null) return;

        int day = TimeManager.Instance != null ? TimeManager.Instance.GetCurrentDay() : 0;
        if (lblDay != null) lblDay.text = $"DAY {day}";
        if (lblIncome != null) lblIncome.text = $"+${Mathf.RoundToInt(data.income)}";
        if (lblBuild != null) lblBuild.text = $"-${Mathf.RoundToInt(data.buildExpenses)}";
        if (lblMaint != null) lblMaint.text = $"-${Mathf.RoundToInt(data.maintenance)}";
        if (lblDisaster != null) lblDisaster.text = $"-${Mathf.RoundToInt(data.disasterLosses)}";
        if (lblRent != null) lblRent.text = $"-${Mathf.RoundToInt(data.rent)}";

        if (lblNet != null)
        {
            lblNet.text = $"${Mathf.RoundToInt(data.net)}";
            lblNet.style.color = data.net >= 0f ? new Color(0.47f, 0.86f, 0.47f) : new Color(0.86f, 0.31f, 0.31f);
        }

        RefreshLoanUI();
        screenSummary.style.display = DisplayStyle.Flex;
    }

    private void RefreshLoanUI()
    {
        float debt = loanDebtVariable != null ? loanDebtVariable.Value : (EconomyManager.Instance != null ? EconomyManager.Instance.LoanDebt : 0f);
        float capital = capitalVariable != null ? capitalVariable.Value : (EconomyManager.Instance != null ? EconomyManager.Instance.Capital : 0f);

        if (debtSection != null)
        {
            debtSection.style.display = debt > 0f ? DisplayStyle.Flex : DisplayStyle.None;
            if (lblDebtActive != null) lblDebtActive.text = $"${Mathf.RoundToInt(debt)}";
        }

        if (loanSection != null)
        {
            bool inDanger = capital < 0f;
            loanSection.style.display = inDanger ? DisplayStyle.Flex : DisplayStyle.None;
            if (inDanger && lblLoanAmount != null)
            {
                pendingLoanAmount = Mathf.Abs(capital) + 500f;
                lblLoanAmount.text = $"Emergency loan available: ${Mathf.RoundToInt(pendingLoanAmount)}";
            }
        }
    }

    private void OnAcceptLoanClicked()
    {
        if (EconomyManager.Instance == null || pendingLoanAmount <= 0f) return;
        EconomyManager.Instance.TakeLoan(pendingLoanAmount);
        pendingLoanAmount = 0f;
        RefreshLoanUI();
    }

    private void OnNextDayClicked()
    {
        if (screenSummary != null) screenSummary.style.display = DisplayStyle.None;
        if (SleepManager.Instance != null) SleepManager.Instance.ConfirmNextDay();
    }
}
