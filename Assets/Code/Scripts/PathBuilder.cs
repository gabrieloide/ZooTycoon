using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class PathBuilder : MonoBehaviour
{
    public static PathBuilder Instance { get; private set; }

    [SerializeField] private PathConfig config;

    private Material pathMat;
    private Transform pathParent;
    private Vector2 lastPaintedCell = new Vector2(float.MinValue, float.MinValue);
    private readonly Dictionary<Vector2, GameObject> tileObjects = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        var temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
        pathMat = new Material(temp.GetComponent<Renderer>().sharedMaterial);
        Destroy(temp);
        pathMat.color = config != null ? config.pathColor : new Color(0.76f, 0.70f, 0.50f, 1f);

        pathParent = new GameObject("Paths").transform;
    }

    public void TryPaintCell(Vector2 gridPos)
    {
        if (gridPos == lastPaintedCell) return;
        lastPaintedCell = gridPos;

        if (PathManager.Has(gridPos)) return;

        var gridCreator = GridCreator.Instance;
        if (gridCreator == null) return;
        if (gridCreator.IsGridOccupied(gridPos)) return;

        float cost = config != null ? config.costPerTile : 10f;
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(cost)) return;
        EconomyManager.Instance.Spend(cost);

        float cs = gridCreator.cellSize;
        var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tile.name = "PathTile";
        tile.transform.position = new Vector3(gridPos.x * cs + cs * 0.5f, 0.02f, gridPos.y * cs + cs * 0.5f);
        tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        tile.transform.localScale = new Vector3(cs * 0.95f, cs * 0.95f, 1f);
        tile.transform.parent = pathParent;
        tile.GetComponent<Renderer>().sharedMaterial = pathMat;
        Destroy(tile.GetComponent<MeshCollider>());

        PathManager.Add(gridPos, cost, BuildController.CurrentSession);
        tileObjects[gridPos] = tile;
    }

    public void RebuildTile(Vector2 gridPos, float cost, int session)
    {
        var gridCreator = GridCreator.Instance;
        if (gridCreator == null) return;

        float cs = gridCreator.cellSize;
        var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tile.name = "PathTile";
        tile.transform.position = new Vector3(gridPos.x * cs + cs * 0.5f, 0.02f, gridPos.y * cs + cs * 0.5f);
        tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        tile.transform.localScale = new Vector3(cs * 0.95f, cs * 0.95f, 1f);
        tile.transform.parent = pathParent;
        tile.GetComponent<Renderer>().sharedMaterial = pathMat;
        Destroy(tile.GetComponent<MeshCollider>());

        PathManager.Add(gridPos, cost, session);
        tileObjects[gridPos] = tile;
    }

    public void DestroyTile(Vector2 gridPos)
    {
        if (tileObjects.TryGetValue(gridPos, out var go))
        {
            Destroy(go);
            tileObjects.Remove(gridPos);
        }
    }

    private void OnDestroy()
    {
        if (pathMat != null) Destroy(pathMat);
    }
}
