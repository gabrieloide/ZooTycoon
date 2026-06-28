using UnityEngine;
using UnityEngine.UIElements;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class OfficeController : MonoBehaviour
{
    public static OfficeController Instance { get; private set; }

    [Header("Event Channels In")]
    [SerializeField] private FloatEventChannelSO onCapitalChangedChannel;
    [SerializeField] private LicenseEventChannelSO onLicensePurchasedChannel;

    private UIDocument uiDocument;
    private VisualElement screenOffice;
    private ScrollView contractsList;
    private Label lblCapital;
    private Button btnClose;

    private float cachedCapital;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        uiDocument = FindAnyObjectByType<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        screenOffice = root.Q<VisualElement>("screen-office");
        if (screenOffice == null) return;

        contractsList = screenOffice.Q<ScrollView>("contracts-list");
        lblCapital    = screenOffice.Q<Label>("lbl-office-capital");
        btnClose      = screenOffice.Q<Button>("btn-office-close");

        if (btnClose != null) btnClose.clicked += Hide;

        screenOffice.style.display = DisplayStyle.None;

        if (EconomyManager.Instance != null)
            cachedCapital = EconomyManager.Instance.Capital;
    }

    private void OnEnable()
    {
        onCapitalChangedChannel?.Subscribe(OnCapitalChanged);
        onLicensePurchasedChannel?.Subscribe(OnLicensePurchased);
    }

    private void OnDisable()
    {
        onCapitalChangedChannel?.Unsubscribe(OnCapitalChanged);
        onLicensePurchasedChannel?.Unsubscribe(OnLicensePurchased);
        if (btnClose != null) btnClose.clicked -= Hide;
    }

    public void Show()
    {
        if (screenOffice == null) return;
        RefreshContracts();
        screenOffice.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (screenOffice == null) return;
        screenOffice.style.display = DisplayStyle.None;
    }

    private void OnCapitalChanged(float capital)
    {
        cachedCapital = capital;
        if (lblCapital != null) lblCapital.text = $"Available Capital: ${Mathf.RoundToInt(capital)}";
        if (screenOffice != null && screenOffice.style.display == DisplayStyle.Flex)
            RefreshContracts();
    }

    private void OnLicensePurchased(LicenseData _) => RefreshContracts();

    private void RefreshContracts()
    {
        if (contractsList == null || LicenseManager.Instance == null) return;
        if (lblCapital != null) lblCapital.text = $"Available Capital: ${Mathf.RoundToInt(cachedCapital)}";
        contractsList.Clear();
        foreach (var license in LicenseManager.Instance.GetAllLicenses())
        {
            if (license != null) contractsList.Add(BuildContractCard(license));
        }
    }

    private VisualElement BuildContractCard(LicenseData license)
    {
        bool purchased = LicenseManager.Instance.IsPurchased(license);
        bool canAfford = license.cost <= 0 || cachedCapital >= license.cost;

        var card = new VisualElement();
        card.style.backgroundColor = purchased ? new Color(0.07f, 0.22f, 0.07f) : new Color(0.1f, 0.1f, 0.13f);
        card.style.borderTopLeftRadius = 8; card.style.borderTopRightRadius = 8;
        card.style.borderBottomLeftRadius = 8; card.style.borderBottomRightRadius = 8;
        card.style.borderTopWidth = 1; card.style.borderBottomWidth = 1;
        card.style.borderLeftWidth = 1; card.style.borderRightWidth = 1;
        var borderCol = purchased ? new Color(0.2f, 0.6f, 0.2f, 0.4f) : new Color(1f, 0.78f, 0.3f, 0.15f);
        card.style.borderTopColor = borderCol; card.style.borderBottomColor = borderCol;
        card.style.borderLeftColor = borderCol; card.style.borderRightColor = borderCol;
        card.style.paddingTop = 14; card.style.paddingBottom = 14;
        card.style.paddingLeft = 16; card.style.paddingRight = 16;
        card.style.marginBottom = 10;
        card.style.flexDirection = FlexDirection.Row;
        card.style.justifyContent = Justify.SpaceBetween;
        card.style.alignItems = Align.Center;

        var info = new VisualElement();
        info.style.flexGrow = 1; info.style.marginRight = 16;
        var lblName = new Label(license.displayName);
        lblName.style.color = Color.white; lblName.style.fontSize = 14;
        lblName.style.unityFontStyleAndWeight = FontStyle.Bold; lblName.style.marginBottom = 4;
        var lblDesc = new Label(license.description);
        lblDesc.style.color = new Color(0.65f, 0.65f, 0.65f); lblDesc.style.fontSize = 11;
        lblDesc.style.whiteSpace = WhiteSpace.Normal; lblDesc.style.marginBottom = 4;
        info.Add(lblName); info.Add(lblDesc);

        if (license.unlockedBiomes != null && license.unlockedBiomes.Count > 0)
        {
            var names = string.Join(", ", license.unlockedBiomes.ConvertAll(b => b != null ? b.displayName : "?"));
            var lblBiomes = new Label($"Unlocks: {names}");
            lblBiomes.style.color = new Color(1f, 0.78f, 0.3f); lblBiomes.style.fontSize = 10;
            info.Add(lblBiomes);
        }
        card.Add(info);

        if (purchased)
        {
            var badge = new Label("ACTIVE");
            badge.style.color = new Color(0.4f, 0.9f, 0.4f); badge.style.fontSize = 11;
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            card.Add(badge);
        }
        else
        {
            var btnSign = new Button();
            btnSign.text = license.cost <= 0 ? "SIGN FREE" : $"SIGN\n${license.cost}";
            btnSign.style.width = 80; btnSign.style.height = 52;
            btnSign.style.backgroundColor = canAfford ? new Color(0.85f, 0.65f, 0.1f) : new Color(0.25f, 0.25f, 0.25f);
            btnSign.style.color = canAfford ? new Color(0.05f, 0.05f, 0.05f) : new Color(0.5f, 0.5f, 0.5f);
            btnSign.style.fontSize = 11; btnSign.style.unityFontStyleAndWeight = FontStyle.Bold;
            btnSign.style.borderTopLeftRadius = 6; btnSign.style.borderTopRightRadius = 6;
            btnSign.style.borderBottomLeftRadius = 6; btnSign.style.borderBottomRightRadius = 6;
            btnSign.style.borderTopWidth = 0; btnSign.style.borderBottomWidth = 0;
            btnSign.style.borderLeftWidth = 0; btnSign.style.borderRightWidth = 0;
            btnSign.SetEnabled(canAfford);
            var captured = license;
            btnSign.clicked += () => { LicenseManager.Instance.TryPurchase(captured); RefreshContracts(); };
            card.Add(btnSign);
        }
        return card;
    }
}
