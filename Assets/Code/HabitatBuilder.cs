using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;
using ZooTycoon.Save;

public class HabitatBuilder : MonoBehaviour
{
    [SerializeField] private CompatibilityMatrix globalMatrix;
    [SerializeField] private HabitatRuntimeSet habitatSet;
    [SerializeField] private PathConfig pathConfig;

    public HabitatSpace RebuildHabitat(HabitatSaveData data, BiomeDefinition biome)
    {
        var gridCreator = GridCreator.Instance;
        if (gridCreator == null || biome == null) return null;

        var habitatGO = new GameObject($"Habitat_{biome.biomeID}_{data.id}");
        var habitatData = habitatGO.AddComponent<HabitatSpace>();

        habitatData.id = data.id;
        habitatData.biome = biome;
        habitatData.xMin = data.xMin;
        habitatData.xMax = data.xMax;
        habitatData.yMin = data.yMin;
        habitatData.yMax = data.yMax;
        habitatData.maxOcupation = data.maxOcupation;
        habitatData.globalMatrix = globalMatrix;
        habitatData.habitatSet = habitatSet;
        habitatData.totalBuildCost = data.totalBuildCost;
        habitatData.buildSession = data.buildSession;

        habitatSet?.Add(habitatData);
        HabitatManager.AddHabitat(habitatData);

        var fences = new List<GameObject>();
        var cells = GetCellsInRect(new Vector2(data.xMin, data.yMin), new Vector2(data.xMax, data.yMax));
        foreach (var cell in cells)
        {
            gridCreator.SetGridOccupied(cell, true);
            var fence = EdgeBuilder(cell, data.xMin, data.xMax, data.yMin, data.yMax, habitatGO.transform);
            if (fence != null) fences.Add(fence);
        }
        habitatData.SetFences(fences);

        return habitatData;
    }

    public void TryBuild(Vector2 start, Vector2 end, BiomeDefinition biome)
    {
        var gridCreator = GridCreator.Instance;
        if (gridCreator == null || biome == null) return;

        int minX = Mathf.Min((int)start.x, (int)end.x);
        int maxX = Mathf.Max((int)start.x, (int)end.x);
        int minY = Mathf.Min((int)start.y, (int)end.y);
        int maxY = Mathf.Max((int)start.y, (int)end.y);

        var cellsToBuild = GetCellsInRect(start, end);
        foreach (var cell in cellsToBuild)
            if (gridCreator.IsGridOccupied(cell)) return;

        if (globalMatrix != null && HabitatManager.IsTooCloseToAnyHabitat(minX, maxX, minY, maxY, globalMatrix.minHabitatDistance))
            return;

        int totalTiles = (maxX - minX + 1) * (maxY - minY + 1);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(biome.buildCost * totalTiles)) return;

        EconomyManager.Instance.Spend(biome.buildCost * totalTiles);

        int newId = HabitatManager.GetNextId();
        var habitatGO = new GameObject($"Habitat_{biome.biomeID}_{newId}");
        var habitatData = habitatGO.AddComponent<HabitatSpace>();

        habitatData.id = newId;
        habitatData.biome = biome;
        habitatData.xMin = minX;
        habitatData.xMax = maxX;
        habitatData.yMin = minY;
        habitatData.yMax = maxY;
        habitatData.maxOcupation = Mathf.Max(1, totalTiles / 4);
        habitatData.globalMatrix = globalMatrix;
        habitatData.habitatSet = habitatSet;
        habitatData.totalBuildCost = biome.buildCost * totalTiles;
        habitatData.buildSession = BuildController.CurrentSession;
        habitatSet?.Add(habitatData);
        HabitatManager.AddHabitat(habitatData);

        foreach (var h in HabitatManager.GetAllHabitats())
            h.RefreshNeighborTension();

        var fences = new List<GameObject>();
        foreach (var cell in cellsToBuild)
        {
            gridCreator.SetGridOccupied(cell, true);
            var fence = EdgeBuilder(cell, minX, maxX, minY, maxY, habitatGO.transform);
            if (fence != null) fences.Add(fence);
        }
        habitatData.SetFences(fences);

        BuildPathConnector(minX, maxX, minY, maxY);
    }

    private void BuildPathConnector(int minX, int maxX, int minY, int maxY)
    {
        var gridCreator = GridCreator.Instance;
        if (gridCreator == null) return;

        var candidates = new List<Vector2>();
        foreach (var cell in PathManager.GetPerimeterCells(minX, maxX, minY, maxY))
            if (gridCreator.IsInBounds(cell) && !gridCreator.IsGridOccupied(cell))
                candidates.Add(cell);
        if (candidates.Count == 0) return;

        Vector2? entranceCell = VisitorSpawner.Instance != null && VisitorSpawner.Instance.SpawnPoint != null
            ? gridCreator.WorldToGridPosition(VisitorSpawner.Instance.SpawnPoint.position)
            : (Vector2?)null;

        var route = PathManager.FindConnectorRoute(candidates, gridCreator.IsGridOccupied, gridCreator.width, gridCreator.height, entranceCell);
        if (route == null || PathBuilder.Instance == null) return;

        float costPerTile = pathConfig != null ? pathConfig.costPerTile : 10f;
        foreach (var cell in route)
        {
            if (PathManager.Has(cell)) continue;
            if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(costPerTile)) break;
            EconomyManager.Instance.Spend(costPerTile);
            PathBuilder.Instance.RebuildTile(cell, costPerTile, BuildController.CurrentSession);
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

    private GameObject EdgeBuilder(Vector2 cell, int xMin, int xMax, int yMin, int yMax, Transform parent)
    {
        var gridCreator = GridCreator.Instance;
        Vector3 position = gridCreator.GetCellWorldPosition(cell);
        if (cell.x == xMin || cell.x == xMax || cell.y == yMin || cell.y == yMax)
        {
            var edgeCell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edgeCell.name = "fence";
            edgeCell.GetComponent<Renderer>().material.color = Color.red;
            edgeCell.transform.position = position;
            edgeCell.transform.parent = parent;

            if (cell.x == xMin && cell.x == xMax && cell.y == yMin && cell.y == yMax)
                edgeCell.transform.localScale = new Vector3(gridCreator.cellSize, 1.5f, 0.1f);
            else if ((cell.x == xMin && cell.y == yMin) || (cell.x == xMax && cell.y == yMin) ||
                     (cell.x == xMin && cell.y == yMax) || (cell.x == xMax && cell.y == yMax))
                edgeCell.transform.localScale = new Vector3(gridCreator.cellSize, 1.5f, gridCreator.cellSize);

            return edgeCell;
        }
        return null;
    }
}
