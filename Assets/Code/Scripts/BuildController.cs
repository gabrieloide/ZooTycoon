using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ZooTycoon.Core;
using ZooTycoon.Data;
using ZooTycoon.UI;

public class BuildController : MonoBehaviour
{
    public static BuildController Instance { get; private set; }

    [SerializeField] private HabitatBuilder habitatBuilder;
    [SerializeField] private AnimalPlacer animalPlacer;
    [SerializeField] private PathBuilder pathBuilder;
    [SerializeField] private SellController sellController;
    [SerializeField] private CompatibilityMatrix globalMatrix;
    [SerializeField] private int minBuildSize = 2;
    [SerializeField] private int maxBuildSize = 8;

    public static int CurrentSession { get; private set; } = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => CurrentSession = 0;

    public static void LoadSession(int session) => CurrentSession = session;

    private GridCreator gridCreator;
    private bool isDragging;
    private Vector2 startDragGridPos;
    private Vector2 currentDragGridPos;

    private BiomeDefinition selectedBiome;
    private AnimalData selectedAnimal;
    private bool selectedPath;
    private bool selectedSell;
    private bool prevHolding;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        gridCreator = GridCreator.Instance;
    }

    private void OnEnable()
    {
        InputManager.Instance.actions.Player.CancelBuilding.performed += CancelBuild;
        if (GameManager.Instance != null) GameManager.Instance.OnModeChanged += OnBuildModeChanged;
    }

    private void OnDisable()
    {
        InputManager.Instance.actions.Player.CancelBuilding.performed -= CancelBuild;
        if (GameManager.Instance != null) GameManager.Instance.OnModeChanged -= OnBuildModeChanged;
    }

    private void OnBuildModeChanged()
    {
        if (GameManager.Instance != null && GameManager.Instance.isBuildMode)
            CurrentSession++;
    }

    private void CancelBuild(InputAction.CallbackContext ctx)
    {
        isDragging = false;
        startDragGridPos = Vector2.zero;
        currentDragGridPos = Vector2.zero;
    }

    public void SelectBiome(BiomeDefinition biome)
    {
        selectedBiome = biome;
        selectedAnimal = null;
        selectedPath = false;
        selectedSell = false;
        prevHolding = false;
    }

    public void SelectAnimal(AnimalData animal)
    {
        selectedAnimal = animal;
        selectedBiome = null;
        selectedPath = false;
        selectedSell = false;
        prevHolding = false;
    }

    public void SelectPath()
    {
        selectedPath = true;
        selectedBiome = null;
        selectedAnimal = null;
        selectedSell = false;
        prevHolding = false;
    }

    public void SelectSell()
    {
        selectedSell = true;
        selectedBiome = null;
        selectedAnimal = null;
        selectedPath = false;
        prevHolding = false;
    }

    public void ClearSelection()
    {
        selectedBiome = null;
        selectedAnimal = null;
        selectedPath = false;
        selectedSell = false;
        prevHolding = false;
    }

    public bool IsDraggingBiome => isDragging && selectedBiome != null;
    public bool IsPathMode => selectedPath;
    public bool IsSellMode => selectedSell;

    public Vector2 GetSizeGrid(out bool isCorrect)
    {
        var size = new Vector2(
            Mathf.Abs(currentDragGridPos.x - startDragGridPos.x) + 1,
            Mathf.Abs(currentDragGridPos.y - startDragGridPos.y) + 1
        );
        isCorrect = size.x >= minBuildSize && size.y >= minBuildSize
                 && size.x < maxBuildSize && size.y < maxBuildSize;
        return size;
    }

    public (float cost, bool canAfford, bool isValid) GetBuildCostPreview()
    {
        if (!isDragging || selectedBiome == null) return (0f, false, false);

        int minX = Mathf.Min((int)startDragGridPos.x, (int)currentDragGridPos.x);
        int maxX = Mathf.Max((int)startDragGridPos.x, (int)currentDragGridPos.x);
        int minY = Mathf.Min((int)startDragGridPos.y, (int)currentDragGridPos.y);
        int maxY = Mathf.Max((int)startDragGridPos.y, (int)currentDragGridPos.y);
        int totalTiles = (maxX - minX + 1) * (maxY - minY + 1);
        float cost = selectedBiome.buildCost * totalTiles;
        GetSizeGrid(out bool isCorrect);
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(cost);
        bool tooClose = globalMatrix != null && HabitatManager.IsTooCloseToAnyHabitat(minX, maxX, minY, maxY, globalMatrix.minHabitatDistance);
        return (cost, canAfford, isCorrect && !tooClose);
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isBuildMode) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (selectedSell) ClearSelection();
            else SelectSell();
            return;
        }

        if (ShopDetector.IsOverShop || UIButton.AnyButtonHovered) return;
        if (gridCreator == null) gridCreator = GridCreator.Instance;
        if (gridCreator == null) return;

        Vector2 gridPos = gridCreator.GetGridPosition();
        float isHolding = InputManager.Instance.actions.Player.Interact.ReadValue<float>();

        if (selectedSell)
        {
            bool holding = isHolding > 0.5f;
            if (holding && !prevHolding)
                sellController?.TrySell(gridPos);
            prevHolding = holding;
            return;
        }
        prevHolding = false;

        if (selectedPath)
        {
            if (isHolding > 0.5f)
                pathBuilder?.TryPaintCell(gridPos);
            return;
        }

        if (isHolding > 0.5f)
        {
            if (!isDragging)
            {
                isDragging = true;
                startDragGridPos = gridPos;
                currentDragGridPos = gridPos;
            }
            else
            {
                currentDragGridPos = gridPos;
            }
        }
        else
        {
            if (isDragging)
            {
                isDragging = false;
                OnRelease();
            }
        }
    }

    private void OnRelease()
    {
        if (selectedAnimal != null)
        {
            animalPlacer.TryPlace(startDragGridPos, selectedAnimal);
        }
        else if (selectedBiome != null)
        {
            GetSizeGrid(out bool isCorrect);
            if (isCorrect)
                habitatBuilder.TryBuild(startDragGridPos, currentDragGridPos, selectedBiome);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || gridCreator == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.isBuildMode) return;
        if (ShopDetector.IsOverShop || UIButton.AnyButtonHovered) return;

        Vector2 hoverPos = gridCreator.GetGridPosition();

        if (isDragging && selectedBiome != null)
        {
            var cells = GetCellsInRect(startDragGridPos, currentDragGridPos);
            bool isValid = true;
            foreach (var cell in cells)
                if (gridCreator.IsGridOccupied(cell)) { isValid = false; break; }

            GetSizeGrid(out bool isCorrect);
            if (!isCorrect) isValid = false;

            if (isValid && globalMatrix != null)
            {
                int minX = Mathf.Min((int)startDragGridPos.x, (int)currentDragGridPos.x);
                int maxX = Mathf.Max((int)startDragGridPos.x, (int)currentDragGridPos.x);
                int minY = Mathf.Min((int)startDragGridPos.y, (int)currentDragGridPos.y);
                int maxY = Mathf.Max((int)startDragGridPos.y, (int)currentDragGridPos.y);
                if (HabitatManager.IsTooCloseToAnyHabitat(minX, maxX, minY, maxY, globalMatrix.minHabitatDistance))
                    isValid = false;
            }

            Gizmos.color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            foreach (var cell in cells)
                Gizmos.DrawCube(gridCreator.GetCellWorldPosition(cell), new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
        }
        else
        {
            if (selectedAnimal != null)
            {
                HabitatSpace habitat = HabitatManager.GetHabitatAtPosition(hoverPos);
                bool canPlace = habitat != null && habitat.currentOcupation < habitat.maxOcupation;
                Gizmos.color = canPlace ? new Color(0f, 0.6f, 1f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            }
            else if (selectedPath)
            {
                bool canPaint = !PathManager.Has(hoverPos) && !gridCreator.IsGridOccupied(hoverPos);
                Gizmos.color = canPaint ? new Color(0.9f, 0.8f, 0.4f, 0.5f) : new Color(1f, 0f, 0f, 0.4f);
            }
            else
            {
                bool isOccupied = gridCreator.IsGridOccupied(hoverPos);
                Gizmos.color = isOccupied ? new Color(1f, 0f, 0f, 0.4f) : new Color(0f, 1f, 0f, 0.4f);
            }
            Gizmos.DrawCube(gridCreator.GetCellWorldPosition(hoverPos), new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
        }
    }

    private List<Vector2> GetCellsInRect(Vector2 start, Vector2 end)
    {
        var cells = new List<Vector2>();
        int minX = Mathf.Min((int)start.x, (int)end.x);
        int maxX = Mathf.Max((int)start.x, (int)end.x);
        int minY = Mathf.Min((int)start.y, (int)end.y);
        int maxY = Mathf.Max((int)start.y, (int)end.y);
        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                cells.Add(new Vector2(x, y));
        return cells;
    }
}
