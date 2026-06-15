using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using ZooTycoon.Core;
using ZooTycoon.Data;


public class HabitatBuilder : MonoBehaviour
{
    private GridCreator gridCreator;

    private bool isDragging = false;
    private Vector2 startDragGridPos;
    private Vector2 currentDragGridPos;

    [SerializeField] private int minToBuildXY = 2;
    [SerializeField] private int maxToBuildXY = 8;

    private BiomeDefinition selectedBiome;
    [SerializeField] private CompatibilityMatrix globalMatrix;

    public void SelectHabitatType(BiomeDefinition biome)
    {
        selectedBiome = biome;
    }

    private void Start()
    {
        gridCreator = GridCreator.Instance;
    }

    private void CancelBuild(InputAction.CallbackContext context)
    {
        isDragging = false;
        startDragGridPos = Vector2.zero;
        currentDragGridPos = Vector2.zero;
    }

    private void OnEnable()
    {
        InputManager.Instance.actions.Player.CancelBuilding.performed += CancelBuild;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isBuildMode || UIButton.AnyButtonHovered || ZooTycoon.UI.ShopDetector.IsOverShop) return;
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
                FinalizeBuild();
            }
        }
    }

    public Vector2 GetSizeGrid(out bool isCorrect)
    {
        var sizeGrid = new Vector2(
            Mathf.Abs(currentDragGridPos.x - startDragGridPos.x) + 1,
            Mathf.Abs(currentDragGridPos.y - startDragGridPos.y) + 1
        );
        isCorrect = sizeGrid.x >= minToBuildXY && sizeGrid.y >= minToBuildXY
                 && sizeGrid.x < maxToBuildXY && sizeGrid.y < maxToBuildXY;
        return sizeGrid;
    }

    private void FinalizeBuild()
    {
        if (selectedBiome == null) return;
        if (GameManager.Instance == null) return;
        if (ZooTycoon.UI.ShopDetector.IsOverShop || UIButton.AnyButtonHovered) return;


        var cellsToBuild = GetCellsInRect(startDragGridPos, currentDragGridPos);
        GetSizeGrid(out bool isCorrect);
        if (!isCorrect)
        {
            Debug.Log("Cannot build: Habitat is too small or too large");
            return;
        }

        bool canBuild = true;

        foreach (Vector2 cell in cellsToBuild)
        {
            if (gridCreator.IsGridOccupied(cell))
            {
                canBuild = false;
                break;
            }
        }
        int minX = Mathf.Min((int)startDragGridPos.x, Mathf.Abs((int)currentDragGridPos.x));
        int maxX = Mathf.Max((int)startDragGridPos.x, Mathf.Abs((int)currentDragGridPos.x));
        int minY = Mathf.Min((int)startDragGridPos.y, Mathf.Abs((int)currentDragGridPos.y));
        int maxY = Mathf.Max((int)startDragGridPos.y, Mathf.Abs((int)currentDragGridPos.y));

        if (canBuild)
        {
            int totalTiles = (maxX - minX + 1) * (maxY - minY + 1);
            if (!EconomyManager.Instance.CanAfford(selectedBiome.buildCost * totalTiles)) return;

            EconomyManager.Instance.Spend(selectedBiome.buildCost * totalTiles);

            int newId = HabitatManager.GetNextId();
            var habitat = new GameObject($"Habitat_{selectedBiome.biomeID}_{newId}");
            habitat.AddComponent<HabitatSpace>();
            var habitatData = habitat.GetComponent<HabitatSpace>();

            habitatData.id = newId;
            habitatData.biome = selectedBiome;
            habitatData.xMin = minX;
            habitatData.xMax = maxX;
            habitatData.yMin = minY;
            habitatData.yMax = maxY;
            habitatData.maxOcupation = Mathf.Max(1, totalTiles / 4);
            habitatData.globalMatrix = globalMatrix;
            HabitatManager.AddHabitat(habitatData);

            foreach (Vector2 cell in cellsToBuild)
            {
                gridCreator.SetGridOccupied(cell, true);
                EdgeBuilder(cell, minX, maxX, minY, maxY, habitat.transform);
            }
        }
    }

    private List<Vector2> GetCellsInRect(Vector2 start, Vector2 end)
    {
        List<Vector2> cells = new();

        int minX = Mathf.Min((int)start.x, Mathf.Abs((int)end.x));
        int maxX = Mathf.Max((int)start.x, Mathf.Abs((int)end.x));
        int minY = Mathf.Min((int)start.y, Mathf.Abs((int)end.y));
        int maxY = Mathf.Max((int)start.y, Mathf.Abs((int)end.y));

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                cells.Add(new Vector2(x, y));
            }
        }

        return cells;
    }

    private void EdgeBuilder(Vector2 cell, int xMin, int xMax, int yMin, int yMax, Transform parent)
    {
        Vector3 position = gridCreator.GetCellWorldPosition(cell);
        if (cell.x == xMin || cell.x == xMax || cell.y == yMin || cell.y == yMax)
        {
            GameObject edgeCell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edgeCell.name = "fence";

            Renderer renderer = edgeCell.GetComponent<Renderer>();
            renderer.material.color = Color.red;

            edgeCell.transform.position = position;
            edgeCell.transform.parent = parent;

            if (cell.x == xMin && cell.x == xMax && cell.y == yMin && cell.y == yMax)
                edgeCell.transform.localScale = new Vector3(gridCreator.cellSize, 1.5f, 0.1f);
            else if (cell.x == xMin && cell.y == yMin || cell.x == xMax && cell.y == yMin ||
                     cell.x == xMin && cell.y == yMax || cell.x == xMax && cell.y == yMax)
                edgeCell.transform.localScale = new Vector3(gridCreator.cellSize, 1.5f, gridCreator.cellSize);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || gridCreator == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.isBuildMode || UIButton.AnyButtonHovered || ZooTycoon.UI.ShopDetector.IsOverShop) return;

        if (isDragging)
        {
            List<Vector2> cellsToBuild = GetCellsInRect(startDragGridPos, currentDragGridPos);

            bool isValid = true;
            foreach (Vector2 cell in cellsToBuild)
            {
                if (gridCreator.IsGridOccupied(cell))
                {
                    isValid = false;
                    break;
                }
            }
            GetSizeGrid(out bool isCorrect);
            if (!isCorrect) isValid = false;

            Gizmos.color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            foreach (Vector2 cell in cellsToBuild)
                Gizmos.DrawCube(gridCreator.GetCellWorldPosition(cell), new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
        }
        else
        {
            Vector2 gridPos = gridCreator.GetGridPosition();
            bool isOccupied = gridCreator.IsGridOccupied(gridPos);
            Gizmos.color = isOccupied ? new Color(1f, 0f, 0f, 0.4f) : new Color(0f, 1f, 0f, 0.4f);
            Gizmos.DrawCube(gridCreator.GetCellWorldPosition(gridPos), new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
        }
    }
}
