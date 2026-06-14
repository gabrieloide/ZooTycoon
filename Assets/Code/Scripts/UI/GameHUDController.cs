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

        private UIDocument uiDocument;
        private Label lblDay;
        private Label lblTime;
        private Label lblMode;
        private Label lblMoney;
        private Label lblGridSize;
        private VisualElement shopPanel;
        private Button btnTabHabitats;
        private Button btnTabAnimals;
        private Button btnTabDecorations;
        private ScrollView shopContent;

        private HabitatBuilder habitatBuilder;
        private BiomeDefinition selectedBiome;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            lblDay = root.Q<Label>("lbl-day");
            lblTime = root.Q<Label>("lbl-time");
            lblMode = root.Q<Label>("lbl-mode");
            lblMoney = root.Q<Label>("lbl-money");
            lblGridSize = root.Q<Label>("lbl-grid-size");
            shopPanel = root.Q<VisualElement>("shop-panel");
            btnTabHabitats = root.Q<Button>("btn-tab-habitats");
            btnTabAnimals = root.Q<Button>("btn-tab-animals");
            btnTabDecorations = root.Q<Button>("btn-tab-decorations");
            shopContent = root.Q<ScrollView>("shop-content");

            btnTabHabitats.clicked += OpenHabitatsTab;
            btnTabAnimals.clicked += OpenAnimalsTab;
            btnTabDecorations.clicked += OpenDecorationsTab;

            TimeManager.onDayChanged += UpdateDayDisplay;
            EconomyManager.OnCapitalChanged += UpdateMoneyDisplay;

            habitatBuilder = FindAnyObjectByType<HabitatBuilder>();
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
            TimeManager.onDayChanged -= UpdateDayDisplay;
            EconomyManager.OnCapitalChanged -= UpdateMoneyDisplay;

            if (GameManager.Instance != null)
                GameManager.Instance.OnModeChanged -= UpdateModeUI;

            if (btnTabHabitats != null) btnTabHabitats.clicked -= OpenHabitatsTab;
            if (btnTabAnimals != null) btnTabAnimals.clicked -= OpenAnimalsTab;
            if (btnTabDecorations != null) btnTabDecorations.clicked -= OpenDecorationsTab;
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
                var btn = CreateShopButton(biome.displayName, $"${biome.buildCost}");
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
                var btn = CreateShopButton(data.displayName, $"${data.purchaseCost}");
                shopContent.Add(btn);
            }
        }

        private void OpenDecorationsTab()
        {
            shopContent.Clear();
            ResetTabStyles();
            SetActiveTabStyle(btnTabDecorations);

            var label = new Label("Decorations coming soon...");
            label.style.color = Color.gray;
            label.style.alignSelf = Align.Center;
            label.style.marginTop = 20;
            shopContent.Add(label);
        }

        private Button CreateShopButton(string title, string subtitle)
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
            return btn;
        }
    }
}
