using UnityEngine;
using ZooTycoon.Core;

public class BuildingUI : MonoBehaviour
{

    [SerializeField] private Transform content;
    private readonly string[] habitatTypes = { "generic", "biome1", "biome2", "biome3", "biome4" };
    [SerializeField] private GameObject habitadButtonPrefab;
    private GameObject previousSelectedHabitat;
    private UIButton habitatButton;
    private HabitatBuilder habitatBuilder;
    private void Awake()
    {
        habitatButton = habitadButtonPrefab.GetComponent<UIButton>();
        SelectorHabitats();

    }
    private void OnEnable()
    {
        if (habitatBuilder == null) habitatBuilder = FindAnyObjectByType<HabitatBuilder>();
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found");
            return;
        }
        GameManager.Instance.OnModeChanged += UpdateUI;
        UpdateUI();
    }
    private void OnDisable()
    {
        GameManager.Instance.OnModeChanged -= UpdateUI;
    }
    private void UpdateUI()
    {
        if (GameManager.Instance.isBuildMode)
        {
            PanelManager.Instance.OpenPanel("ShopPanel");
        }
        else
        {
            PanelManager.Instance.ClosePanel("ShopPanel");
        }
    }
    private void SelectorHabitats()
    {
        for (int i = 0; i < habitatTypes.Length; i++)
        {
            var btn = Instantiate(habitatButton, content);
            btn.GetComponent<UIButton>().IsToggle = true;
            var habitadType = habitatTypes[i];
            btn.name = habitadType;
            btn.OnToggleChanged.AddListener((isSelected) => OnHabitatSelected(btn.gameObject, habitadType, isSelected));
        }
    }
    private void OnHabitatSelected(GameObject btn, string type, bool isSelected)
    {
        if (isSelected)
        {
            if (previousSelectedHabitat != null)
            {
                previousSelectedHabitat.GetComponent<UIButton>().IsSelected = false;
            }
            habitatBuilder.SelectHabitatType(type);
            previousSelectedHabitat = btn;
            Debug.Log("Habitat type selected: " + type);
        }
        else
        {
            habitatBuilder.SelectHabitatType(null);
            previousSelectedHabitat = null;
        }
    }
}