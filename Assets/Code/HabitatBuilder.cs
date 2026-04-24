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

    [SerializeField] private string selectedHabitatType = "generic";

    public void SelectHabitatType(string type)
    {
        selectedHabitatType = type;
    }
    private void Start()
    {
        gridCreator = GridCreator.Instance;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isBuildMode) return;
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
        var sizeGrid = new Vector2(currentDragGridPos.x - startDragGridPos.x + 1, currentDragGridPos.y - startDragGridPos.y + 1);
        if (sizeGrid.x >= minToBuildXY && sizeGrid.y >= minToBuildXY && sizeGrid.x < maxToBuildXY && sizeGrid.y < maxToBuildXY)
        {
            isCorrect = true;
        }
        else
        {
            isCorrect = false;
        }
        return sizeGrid;
    }

    private void FinalizeBuild()
    {
        if (GameManager.Instance == null || GameManager.Instance.shopDetector.isOnShop) return;
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

        int minX = Mathf.Min((int)startDragGridPos.x, (int)currentDragGridPos.x);
        int maxX = Mathf.Max((int)startDragGridPos.x, (int)currentDragGridPos.x);
        int minY = Mathf.Min((int)startDragGridPos.y, (int)currentDragGridPos.y);
        int maxY = Mathf.Max((int)startDragGridPos.y, (int)currentDragGridPos.y);

        if (canBuild)
        {
            HabitadManager.AddHabitad(new HabitadSpace
            {
                id = HabitadManager.GetNextId(),
                type = "generic",
                x = minX,
                y = minY,
            });
            foreach (Vector2 cell in cellsToBuild)
            {
                gridCreator.SetGridOccupied(cell, true);
                EdgeBuilder(cell, minX, maxX, minY, maxY);
            }
        }
    }

    private List<Vector2> GetCellsInRect(Vector2 start, Vector2 end)
    {
        List<Vector2> cells = new();

        int minX = Mathf.Min((int)start.x, (int)end.x);
        int maxX = Mathf.Max((int)start.x, (int)end.x);
        int minY = Mathf.Min((int)start.y, (int)end.y);
        int maxY = Mathf.Max((int)start.y, (int)end.y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                cells.Add(new Vector2(x, y));
            }
        }

        return cells;
    }
    private void EdgeBuilder(Vector2 cell, int xMin, int xMax, int yMin, int yMax)
    {
        var fence = new GameObject("fence");
        Vector3 position = gridCreator.GetCellWorldPosition(cell);
        if (cell.x == xMin || cell.x == xMax || cell.y == yMin || cell.y == yMax)
        {
            var edgeCell = Instantiate(fence, position, Quaternion.identity);
            edgeCell.transform.parent = transform;
        }
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || gridCreator == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.isBuildMode) return;
        if (GameManager.Instance.shopDetector.isOnShop) return;

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
            if (!isCorrect)
            {
                isValid = false;
            }
            Gizmos.color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

            foreach (Vector2 cell in cellsToBuild)
            {
                Gizmos.DrawCube(gridCreator.GetCellWorldPosition(cell), new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
            }
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