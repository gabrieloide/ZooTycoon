using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class HabitatBuilder : MonoBehaviour
{
    [SerializeField] private CompatibilityMatrix globalMatrix;
    [SerializeField] private HabitatRuntimeSet habitatSet;

    public void TryBuild(Vector2 start, Vector2 end, BiomeDefinition biome)
    {
        var gridCreator = GridCreator.Instance;
        if (gridCreator == null || biome == null) return;

        int minX = Mathf.Min((int)start.x, Mathf.Abs((int)end.x));
        int maxX = Mathf.Max((int)start.x, Mathf.Abs((int)end.x));
        int minY = Mathf.Min((int)start.y, Mathf.Abs((int)end.y));
        int maxY = Mathf.Max((int)start.y, Mathf.Abs((int)end.y));

        var cellsToBuild = GetCellsInRect(start, end);
        foreach (var cell in cellsToBuild)
            if (gridCreator.IsGridOccupied(cell)) return;

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
        habitatSet?.Add(habitatData);
        HabitatManager.AddHabitat(habitatData);

        foreach (var cell in cellsToBuild)
        {
            gridCreator.SetGridOccupied(cell, true);
            EdgeBuilder(cell, minX, maxX, minY, maxY, habitatGO.transform);
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

    private void EdgeBuilder(Vector2 cell, int xMin, int xMax, int yMin, int yMax, Transform parent)
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
        }
    }
}
