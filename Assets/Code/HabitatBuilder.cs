using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using ZooTycoon.Core;

public class HabitatBuilder : MonoBehaviour
{
    private GridCreator gridCreator;

    private bool isDragging = false;
    private Vector2 startDragGridPos;
    private Vector2 currentDragGridPos;

    private void Start()
    {
        gridCreator = GridCreator.Instance;
    }

    private void Update()
    {
        if (ZooTycoon.Core.GameManager.Instance == null || !ZooTycoon.Core.GameManager.Instance.isBuildMode) return;
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

    private void FinalizeBuild()
    {
        List<Vector2> cellsToBuild = GetCellsInRect(startDragGridPos, currentDragGridPos);

        bool canBuild = true;
        foreach (Vector2 cell in cellsToBuild)
        {
            if (gridCreator.IsGridOccupied(cell))
            {
                canBuild = false;
                break;
            }
        }

        if (canBuild)
        {
            foreach (Vector2 cell in cellsToBuild)
            {
                gridCreator.SetGridOccupied(cell, true);
            }
        }
    }

    private List<Vector2> GetCellsInRect(Vector2 start, Vector2 end)
    {
        List<Vector2> cells = new List<Vector2>();

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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || gridCreator == null) return;
        if (ZooTycoon.Core.GameManager.Instance == null || !ZooTycoon.Core.GameManager.Instance.isBuildMode) return;

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

            Gizmos.color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

            foreach (Vector2 cell in cellsToBuild)
            {
                Vector3 position = new Vector3(cell.x * gridCreator.cellSize + gridCreator.cellSize / 2f, 0, cell.y * gridCreator.cellSize + gridCreator.cellSize / 2f);
                Gizmos.DrawCube(position, new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
            }
        }
        else
        {
            Vector2 gridPos = gridCreator.GetGridPosition();
            bool isOccupied = gridCreator.IsGridOccupied(gridPos);
            Gizmos.color = isOccupied ? new Color(1f, 0f, 0f, 0.4f) : new Color(0f, 1f, 0f, 0.4f);
            
            Vector3 position = new Vector3(gridPos.x * gridCreator.cellSize + gridCreator.cellSize / 2f, 0, gridPos.y * gridCreator.cellSize + gridCreator.cellSize / 2f);
            Gizmos.DrawCube(position, new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
        }
    }
}