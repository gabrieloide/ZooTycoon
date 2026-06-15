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
    [SerializeField] private int minBuildSize = 2;
    [SerializeField] private int maxBuildSize = 8;

    private GridCreator gridCreator;
    private bool isDragging;
    private Vector2 startDragGridPos;
    private Vector2 currentDragGridPos;

    private BiomeDefinition selectedBiome;
    private AnimalData selectedAnimal;

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
    }

    private void OnDisable()
    {
        InputManager.Instance.actions.Player.CancelBuilding.performed -= CancelBuild;
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
    }

    public void SelectAnimal(AnimalData animal)
    {
        selectedAnimal = animal;
        selectedBiome = null;
    }

    public void ClearSelection()
    {
        selectedBiome = null;
        selectedAnimal = null;
    }

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

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isBuildMode) return;
        if (ShopDetector.IsOverShop || UIButton.AnyButtonHovered) return;
        if (gridCreator == null) gridCreator = GridCreator.Instance;
        if (gridCreator == null) return;

        Vector2 gridPos = gridCreator.GetGridPosition();
        float isHolding = InputManager.Instance.actions.Player.Interact.ReadValue<float>();

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
        int minX = Mathf.Min((int)start.x, Mathf.Abs((int)end.x));
        int maxX = Mathf.Max((int)start.x, Mathf.Abs((int)end.x));
        int minY = Mathf.Min((int)start.y, Mathf.Abs((int)end.y));
        int maxY = Mathf.Max((int)start.y, Mathf.Abs((int)end.y));
        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                cells.Add(new Vector2(x, y));
        return cells;
    }
}
