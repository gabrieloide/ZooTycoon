using UnityEngine;
using System.Collections.Generic;

public class GridCreator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Transform gridParent;
    private List<GridSpace> gridList = new List<GridSpace>();
    private Dictionary<Vector2, GridSpace> gridDictionary = new Dictionary<Vector2, GridSpace>();
    public static GridCreator Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridSpace newSpace = new GridSpace(false, x, y);
                gridList.Add(newSpace);
                gridDictionary.Add(new Vector2(x, y), newSpace);
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (UnityEngine.InputSystem.Mouse.current == null || Camera.main == null) return Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
    public Vector2 GetGridPosition()
    {
        Vector3 worldPos = GetMouseWorldPosition();
        Vector2 gridPosition = new Vector2(Mathf.FloorToInt(worldPos.x / cellSize), Mathf.FloorToInt(worldPos.z / cellSize));
        return gridPosition;
    }

    public bool IsGridOccupied(Vector2 gridPosition)
    {
        if (gridDictionary.ContainsKey(gridPosition))
        {
            return gridDictionary[gridPosition].IsOccupied();
        }
        return false;
    }

    public void SetGridOccupied(Vector2 gridPosition, bool isOccupied)
    {
        if (gridDictionary.ContainsKey(gridPosition))
        {
            gridDictionary[gridPosition].SetOccupied(isOccupied);
        }
    }

    public Vector3 GetCellWorldPosition(Vector2 gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize + cellSize / 2f, 0, gridPosition.y * cellSize + cellSize / 2f);
    }

    void OnDrawGizmos()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = GetCellWorldPosition(new Vector2(x, y));

                if (Application.isPlaying && IsGridOccupied(new Vector2(x, y)))
                {
                    Gizmos.color = Color.red; // El suelo construido se mantiene rojo
                    Gizmos.DrawCube(position, new Vector3(cellSize, 0.1f, cellSize));
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(position, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }

        if (ZooTycoon.Core.GameManager.Instance != null && ZooTycoon.Core.GameManager.Instance.isBuildMode)
        {
            Vector2 gridPosition = GetGridPosition();
            if (IsGridOccupied(gridPosition))
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireCube(GetCellWorldPosition(gridPosition), new Vector3(cellSize, 0.1f, cellSize));
        }
    }
}