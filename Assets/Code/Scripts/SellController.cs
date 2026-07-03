using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ZooTycoon.Core;
using ZooTycoon.Data;
using ZooTycoon.UI;

public class SellController : MonoBehaviour
{
    public static SellController Instance { get; private set; }

    [SerializeField] private SellConfig config;

    private enum HoverType { None, Animal, Habitat, Path }

    private HoverType hoverType;
    private Animal hoveredAnimal;
    private HabitatSpace hoveredHabitat;
    private Vector2 hoveredPathCell;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        hoverType = HoverType.None;
        hoveredAnimal = null;
        hoveredHabitat = null;

        if (BuildController.Instance == null || !BuildController.Instance.IsSellMode) return;
        if (ShopDetector.IsOverShop || UIButton.AnyButtonHovered) return;

        var gridCreator = GridCreator.Instance;
        if (gridCreator == null) return;

        if (Camera.main != null && Mouse.current != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var animal = hit.collider.GetComponentInParent<Animal>();
                if (animal != null) { hoverType = HoverType.Animal; hoveredAnimal = animal; return; }
            }
        }

        Vector2 gridPos = gridCreator.GetGridPosition();
        var habitat = HabitatManager.GetHabitatAtPosition(gridPos);
        if (habitat != null) { hoverType = HoverType.Habitat; hoveredHabitat = habitat; return; }

        if (PathManager.Has(gridPos)) { hoverType = HoverType.Path; hoveredPathCell = gridPos; }
    }

    public void TrySell(Vector2 gridPos)
    {
        if (config == null) return;

        if (Camera.main != null && Mouse.current != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var animal = hit.collider.GetComponentInParent<Animal>();
                if (animal != null) { SellAnimal(animal); return; }
            }
        }

        var habitat = HabitatManager.GetHabitatAtPosition(gridPos);
        if (habitat != null) { SellHabitat(habitat); return; }

        if (PathManager.Has(gridPos)) SellPath(gridPos);
    }

    private void SellAnimal(Animal animal)
    {
        float refund = animal.GetSellRefund(config.staledRefundRate);

        if (animal.habitat != null)
        {
            animal.habitat.RemoveAnimal(animal);
        }
        else
        {
            if (animal.isQuarantined) QuarantineManager.Instance?.Release(animal);
            Destroy(animal.gameObject);
        }

        if (EconomyManager.Instance != null) EconomyManager.Instance.Earn(refund);
    }

    private void SellHabitat(HabitatSpace habitat)
    {
        float refund = habitat.GetSellRefund(config.staledRefundRate);

        var escapedAnimals = new List<Animal>();
        foreach (var animal in habitat.GetAnimals())
            if (animal != null && animal.hasEscaped)
                escapedAnimals.Add(animal);
        foreach (var animal in escapedAnimals)
            Destroy(animal.gameObject);

        var gridCreator = GridCreator.Instance;
        if (gridCreator != null)
        {
            for (int x = habitat.xMin; x <= habitat.xMax; x++)
                for (int y = habitat.yMin; y <= habitat.yMax; y++)
                    gridCreator.SetGridOccupied(new Vector2(x, y), false);
        }

        habitat.habitatSet?.Remove(habitat);
        HabitatManager.RemoveHabitat(habitat.id);

        foreach (var h in HabitatManager.GetAllHabitats())
            if (h != null) h.RefreshNeighborTension();

        if (EconomyManager.Instance != null) EconomyManager.Instance.Earn(refund);

        Destroy(habitat.gameObject);
    }

    private void SellPath(Vector2 gridPos)
    {
        float refund = PathManager.GetRefund(gridPos, config.staledRefundRate, BuildController.CurrentSession);
        PathBuilder.Instance?.DestroyTile(gridPos);
        PathManager.Remove(gridPos);
        if (EconomyManager.Instance != null) EconomyManager.Instance.Earn(refund);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        var gridCreator = GridCreator.Instance;
        if (gridCreator == null) return;

        if (hoverType == HoverType.Habitat && hoveredHabitat != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
            for (int x = hoveredHabitat.xMin; x <= hoveredHabitat.xMax; x++)
                for (int y = hoveredHabitat.yMin; y <= hoveredHabitat.yMax; y++)
                    Gizmos.DrawCube(gridCreator.GetCellWorldPosition(new Vector2(x, y)),
                        new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
        }
        else if (hoverType == HoverType.Path)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawCube(gridCreator.GetCellWorldPosition(hoveredPathCell),
                new Vector3(gridCreator.cellSize, 0.2f, gridCreator.cellSize));
        }
        else if (hoverType == HoverType.Animal && hoveredAnimal != null)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(hoveredAnimal.transform.position, 0.6f);
        }
    }
}
