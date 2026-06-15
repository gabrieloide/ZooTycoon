using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ZooTycoon.Core;
using ZooTycoon.Data;

namespace ZooTycoon.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHUDController : MonoBehaviour
    {
        [SerializeField] private List<BiomeDefinition> availableBiomes = new();
        [SerializeField] private List<LicenseData> availableLicenses = new();

        private UIDocument uiDocument;
        private VisualElement uiRoot;
        private Label lblDay;
        private Label lblTime;
        private Label lblMode;
        private Label lblMoney;
        private Label lblGridSize;
        private Label lblVisitors;
        private VisualElement shopPanel;
        private Button btnTabHabitats;
        private Button btnTabAnimals;
        private Button btnTabDecorations;
        private Button btnTabLicenses;
        private ScrollView shopContent;

        private VisualElement tooltipPanel;
        private Label tooltipLblName;
        private Label tooltipLblDesc;

        private HabitatBuilder habitatBuilder;
        private BiomeDefinition selectedBiome;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            uiRoot = uiDocument.rootVisualElement;

            lblDay = uiRoot.Q<Label>("lbl-day");
            lblTime = uiRoot.Q<Label>("lbl-time");
            lblMode = uiRoot.Q<Label>("lbl-mode");
            lblMoney = uiRoot.Q<Label>("lbl-money");
            lblGridSize = uiRoot.Q<Label>("lbl-grid-size");
            lblVisitors = uiRoot.Q<Label>("lbl-visitors");
            shopPanel = uiRoot.Q<VisualElement>("shop-panel");
            btnTabHabitats = uiRoot.Q<Button>("btn-tab-habitats");
            btnTabAnimals = uiRoot.Q<Button>("btn-tab-animals");
            btnTabDecorations = uiRoot.Q<Button>("btn-tab-decorations");
            btnTabLicenses = uiRoot.Q<Button>("btn-tab-licenses");
            shopContent = uiRoot.Q<ScrollView>("shop-content");

            btnTabHabitats.clicked += OpenHabitatsTab;
            btnTabAnimals.clicked += OpenAnimalsTab;
            btnTabDecorations.clicked += OpenDecorationsTab;
            btnTabLicenses.clicked += OpenLicensesTab;

            shopPanel.RegisterCallback<PointerEnterEvent>(_ => ShopDetector.IsOverShop = true, TrickleDown.TrickleDown);
            shopPanel.RegisterCallback<PointerLeaveEvent>(_ => ShopDetector.IsOverShop = false, TrickleDown.TrickleDown);

            SetupTooltip(uiRoot);

            TimeManager.onDayEnded += UpdateDayDisplay;
            EconomyManager.OnCapitalChanged += UpdateMoneyDisplay;
            VisitorManager.OnVisitorCountChanged += UpdateVisitorDisplay;

            habitatBuilder = FindAnyObjectByType<HabitatBuilder>();
        }

        private void SetupTooltip(VisualElement root)
        {
            tooltipPanel = new VisualElement();
            tooltipPanel.style.position = Position.Absolute;
            tooltipPanel.style.bottom = 95f;
            tooltipPanel.style.width = 230f;
            tooltipPanel.style.backgroundColor = new Color(0.07f, 0.07f, 0.07f, 0.94f);
            tooltipPanel.style.borderTopLeftRadius = 6f;
            tooltipPanel.style.borderTopRightRadius = 6f;
            tooltipPanel.style.borderBottomLeftRadius = 6f;
            tooltipPanel.style.borderBottomRightRadius = 6f;
            tooltipPanel.style.borderTopWidth = 1f;
            tooltipPanel.style.borderBottomWidth = 1f;
            tooltipPanel.style.borderLeftWidth = 1f;
            tooltipPanel.style.borderRightWidth = 1f;
            tooltipPanel.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            tooltipPanel.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            tooltipPanel.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            tooltipPanel.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
            tooltipPanel.style.paddingTop = 8f;
            tooltipPanel.style.paddingBottom = 8f;
            tooltipPanel.style.paddingLeft = 10f;
            tooltipPanel.style.paddingRight = 10f;
            tooltipPanel.style.display = DisplayStyle.None;
            tooltipPanel.pickingMode = PickingMode.Ignore;

            tooltipLblName = new Label();
            tooltipLblName.style.color = Color.white;
            tooltipLblName.style.fontSize = 12f;
            tooltipLblName.style.unityFontStyleAndWeight = FontStyle.Bold;
            tooltipLblName.style.marginBottom = 4f;

            tooltipLblDesc = new Label();
            tooltipLblDesc.style.color = new Color(0.75f, 0.75f, 0.75f, 1f);
            tooltipLblDesc.style.fontSize = 10f;
            tooltipLblDesc.style.whiteSpace = WhiteSpace.Normal;

            tooltipPanel.Add(tooltipLblName);
            tooltipPanel.Add(tooltipLblDesc);
            root.Add(tooltipPanel);
        }

        private void ShowTooltip(string name, string desc, Vector2 pointerWorldPos)
        {
            if (tooltipPanel == null || string.IsNullOrEmpty(desc)) return;
            tooltipLblName.text = name;
            tooltipLblDesc.text = desc;

            Vector2 local = uiRoot.WorldToLocal(pointerWorldPos);
            float tooltipWidth = 234f;
            float left = Mathf.Clamp(local.x - tooltipWidth * 0.5f, 4f, uiRoot.resolvedStyle.width - tooltipWidth - 4f);
            tooltipPanel.style.left = left;
            tooltipPanel.style.display = DisplayStyle.Flex;
        }

        private void HideTooltip()
        {
            if (tooltipPanel == null) return;
            tooltipPanel.style.display = DisplayStyle.None;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnModeChanged += UpdateModeUI;

            UpdateDayDisplay();
            UpdateModeUI();

            if (EconomyManager.Instance != null)
                UpdateMoneyDisplay(EconomyManager.Instance.Capital);
        }

        private void OnDisable()
        {
            TimeManager.onDayEnded -= UpdateDayDisplay;
            EconomyManager.OnCapitalChanged -= UpdateMoneyDisplay;
            VisitorManager.OnVisitorCountChanged -= UpdateVisitorDisplay;

            if (GameManager.Instance != null)
                GameManager.Instance.OnModeChanged -= UpdateModeUI;

            if (btnTabHabitats != null) btnTabHabitats.clicked -= OpenHabitatsTab;
            if (btnTabAnimals != null) btnTabAnimals.clicked -= OpenAnimalsTab;
            if (btnTabDecorations != null) btnTabDecorations.clicked -= OpenDecorationsTab;
            if (btnTabLicenses != null) btnTabLicenses.clicked -= OpenLicensesTab;

            ShopDetector.IsOverShop = false;
        }

        private void Update()
        {
            if (TimeManager.Instance != null && lblTime != null)
            {
                Vector2Int t = TimeManager.Instance.GetCurrentTimeInDay();
                lblTime.text = string.Format("{0:00}:{1:00}", t.x, t.y);
            }

            if (GameManager.Instance != null && GameManager.Instance.isBuildMode && habitatBuilder != null && lblGridSize != null)
            {
                var size = habitatBuilder.GetSizeGrid(out bool isCorrect);
                lblGridSize.text = $"{size.x} x | {size.y} y";
                lblGridSize.style.color = isCorrect ? Color.green : Color.red;
            }
        }

        private void UpdateDayDisplay()
        {
            if (TimeManager.Instance != null && lblDay != null)
                lblDay.text = $"Day: {TimeManager.Instance.GetCurrentDay()}";
        }

        private void UpdateMoneyDisplay(float capital)
        {
            if (lblMoney != null)
                lblMoney.text = $"${Mathf.RoundToInt(capital)}";
        }

        private void UpdateVisitorDisplay(int visitors)
        {
            if (lblVisitors != null)
                lblVisitors.text = $"Visitors: {visitors}";
        }

        private void UpdateModeUI()
        {
            if (GameManager.Instance == null || shopPanel == null) return;

            if (GameManager.Instance.isBuildMode)
            {
                lblMode.text = "Build Mode";
                lblMode.style.color = Color.yellow;
                shopPanel.style.display = DisplayStyle.Flex;
                OpenHabitatsTab();
            }
            else
            {
                lblMode.text = "Normal Mode";
                lblMode.style.color = Color.white;
                shopPanel.style.display = DisplayStyle.None;
                HideTooltip();
                ShopDetector.IsOverShop = false;
                if (lblGridSize != null)
                {
                    lblGridSize.text = "0 x | 0 y";
                    lblGridSize.style.color = Color.white;
                }
            }
        }

        private void ResetTabStyles()
        {
            SetInactiveTabStyle(btnTabHabitats);
            SetInactiveTabStyle(btnTabAnimals);
            SetInactiveTabStyle(btnTabDecorations);
            SetInactiveTabStyle(btnTabLicenses);
        }

        private void SetInactiveTabStyle(Button tab)
        {
            tab.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            tab.style.color = new Color(0.55f, 0.55f, 0.55f);
            tab.style.unityFontStyleAndWeight = FontStyle.Normal;
        }

        private void SetActiveTabStyle(Button tab)
        {
            tab.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            tab.style.color = Color.white;
            tab.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        private void OpenHabitatsTab()
        {
            HideTooltip();
            shopContent.Clear();
            ResetTabStyles();
            SetActiveTabStyle(btnTabHabitats);

            if (availableBiomes == null || availableBiomes.Count == 0)
            {
                Debug.LogWarning("GameHUDController: availableBiomes list is empty.");
                return;
            }

            foreach (var biome in availableBiomes)
            {
                if (biome == null) continue;
                var btn = CreateShopButton(biome.displayName, $"${biome.buildCost}", biome.description);
                if (selectedBiome == biome)
                    btn.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f);

                var captured = biome;
                btn.clicked += () => OnBiomeSelected(btn, captured);
                shopContent.Add(btn);
            }
        }

        private void OnBiomeSelected(Button btn, BiomeDefinition biome)
        {
            if (selectedBiome == biome)
            {
                selectedBiome = null;
                habitatBuilder.SelectHabitatType(null);
                OpenHabitatsTab();
            }
            else
            {
                selectedBiome = biome;
                habitatBuilder.SelectHabitatType(biome);
                OpenHabitatsTab();
            }
        }

        private void OpenAnimalsTab()
        {
            HideTooltip();
            shopContent.Clear();
            ResetTabStyles();
            SetActiveTabStyle(btnTabAnimals);

            if (AnimalManager.Instance == null)
            {
                Debug.LogWarning("GameHUDController: AnimalManager not found in scene.");
                return;
            }

            foreach (var data in AnimalManager.Instance.GetAnimalDataList())
            {
                if (data == null) continue;
                var btn = CreateShopButton(data.displayName, $"${data.purchaseCost}", data.description);
                shopContent.Add(btn);
            }
        }

        private void OpenDecorationsTab()
        {
            HideTooltip();
            shopContent.Clear();
            ResetTabStyles();
            SetActiveTabStyle(btnTabDecorations);

            var label = new Label("Decorations coming soon...");
            label.style.color = Color.gray;
            label.style.alignSelf = Align.Center;
            label.style.marginTop = 20;
            shopContent.Add(label);
        }

        private void OpenLicensesTab()
        {
            HideTooltip();
            shopContent.Clear();
            ResetTabStyles();
            SetActiveTabStyle(btnTabLicenses);

            if (availableLicenses == null || availableLicenses.Count == 0)
            {
                var empty = new Label("No licenses configured.");
                empty.style.color = Color.gray;
                empty.style.alignSelf = Align.Center;
                empty.style.marginTop = 20;
                shopContent.Add(empty);
                return;
            }

            foreach (var license in availableLicenses)
            {
                if (license == null) continue;
                string costLabel = license.cost <= 0 ? "Free" : $"${license.cost}";
                var btn = CreateShopButton(license.displayName, costLabel, license.description);
                btn.style.width = 100;
                shopContent.Add(btn);
            }
        }

        private Button CreateShopButton(string title, string subtitle, string description = "")
        {
            var btn = new Button();
            btn.style.width = 80;
            btn.style.height = 80;
            btn.style.marginRight = 8;
            btn.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.style.borderTopWidth = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderLeftWidth = 0;
            btn.style.borderRightWidth = 0;
            btn.style.flexDirection = FlexDirection.Column;
            btn.style.alignItems = Align.Center;
            btn.style.justifyContent = Justify.Center;

            var lblTitle = new Label(title);
            lblTitle.style.color = Color.white;
            lblTitle.style.fontSize = 11;
            lblTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            lblTitle.style.whiteSpace = WhiteSpace.Normal;
            lblTitle.style.unityTextAlign = TextAnchor.MiddleCenter;

            var lblSub = new Label(subtitle);
            lblSub.style.color = new Color(0.8f, 0.9f, 0.5f);
            lblSub.style.fontSize = 10;
            lblSub.style.unityTextAlign = TextAnchor.MiddleCenter;

            btn.Add(lblTitle);
            btn.Add(lblSub);

            if (!string.IsNullOrEmpty(description))
            {
                btn.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(title, description, evt.position));
                btn.RegisterCallback<PointerLeaveEvent>(_ => HideTooltip());
            }

            return btn;
        }
    }
}
