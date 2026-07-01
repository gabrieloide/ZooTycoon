using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class HabitatSpace : MonoBehaviour
{
    [Header("Identification")]
    public int id;
    public BiomeDefinition biome;

    [Header("SO Architecture")]
    [SerializeField] public HabitatRuntimeSet habitatSet;

    [Header("Grid Dimensions")]
    public int xMin;
    public int xMax;
    public int yMin;
    public int yMax;

    [Header("Occupants")]
    public int currentOcupation;
    public int maxOcupation;
    public float generalAnnoyance;
    public CompatibilityMatrix globalMatrix;

    [HideInInspector] public float totalBuildCost;
    [HideInInspector] public int buildSession;

    public float GetSellRefund(float staledRate)
    {
        float rate = buildSession == BuildController.CurrentSession ? 1f : staledRate;
        float total = totalBuildCost * rate;
        foreach (var animal in animals)
            if (animal != null) total += animal.GetSellRefund(staledRate);
        return total;
    }

    private List<Animal> animals = new();
    private float cachedNeighborTension;

    private List<GameObject> fenceSegments = new();
    private GameObject brokenFence;
    private Quaternion brokenFenceOriginalRotation;
    private Vector3 brokenFenceOriginalScale;
    private Color brokenFenceOriginalColor;

    private void OnDisable() => habitatSet?.Remove(this);

    public void SetFences(List<GameObject> fences)
    {
        fenceSegments = fences;
    }

    public bool HasEscapedAnimals()
    {
        foreach (var animal in animals)
            if (animal != null && animal.hasEscaped) return true;
        return false;
    }

    public void BreakFence()
    {
        if (brokenFence != null) return;

        var candidates = new List<GameObject>();
        foreach (var fence in fenceSegments)
            if (fence != null) candidates.Add(fence);
        if (candidates.Count == 0) return;

        brokenFence = candidates[Random.Range(0, candidates.Count)];
        var renderer = brokenFence.GetComponent<Renderer>();

        brokenFenceOriginalRotation = brokenFence.transform.rotation;
        brokenFenceOriginalScale = brokenFence.transform.localScale;
        brokenFenceOriginalColor = renderer != null ? renderer.material.color : Color.red;

        brokenFence.transform.rotation = brokenFenceOriginalRotation * Quaternion.Euler(0f, 0f, 75f);
        brokenFence.transform.localScale = new Vector3(brokenFenceOriginalScale.x, brokenFenceOriginalScale.y * 0.4f, brokenFenceOriginalScale.z);
        if (renderer != null) renderer.material.color = new Color(0.3f, 0.2f, 0.1f);
    }

    public void RepairFence()
    {
        if (brokenFence == null) return;

        brokenFence.transform.rotation = brokenFenceOriginalRotation;
        brokenFence.transform.localScale = brokenFenceOriginalScale;
        var renderer = brokenFence.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = brokenFenceOriginalColor;

        brokenFence = null;
    }

    public void AddAnimal(AnimalData animal)
    {
        if (currentOcupation < maxOcupation)
        {
            var instance = Instantiate(animal.animalPrefab, transform);
            var animalComponent = instance.GetComponent<Animal>();
            if (animalComponent == null)
            {
                Debug.LogError($"Prefab '{animal.animalPrefab.name}' is missing an Animal component.", instance);
                Destroy(instance);
                return;
            }
            animalComponent.data = animal;
            animalComponent.habitat = this;
            animals.Add(animalComponent);
            currentOcupation = animals.Count;

            float cellSize = GridCreator.Instance != null ? GridCreator.Instance.cellSize : 1f;
            float cx = (xMin + xMax + 1) * 0.5f * cellSize;
            float cz = (yMin + yMax + 1) * 0.5f * cellSize;
            instance.transform.position = new Vector3(cx, 0f, cz);

            if (instance.GetComponent<AnimalStressBar>() == null)
                instance.AddComponent<AnimalStressBar>();
            if (instance.GetComponent<AnimalWandering>() == null)
                instance.AddComponent<AnimalWandering>();
            if (instance.GetComponent<AnimalInteraction>() == null)
                instance.AddComponent<AnimalInteraction>();

            UpdateTemperament();
        }
    }

    public void RemoveAnimal(Animal animal)
    {
        if (animals.Contains(animal))
        {
            animals.Remove(animal);
            currentOcupation = animals.Count;
            Destroy(animal.gameObject);
            UpdateTemperament();
        }
    }

    public void RemoveAnimal()
    {
        if (animals.Count > 0)
        {
            Animal animal = animals[animals.Count - 1];
            animals.RemoveAt(animals.Count - 1);
            currentOcupation = animals.Count;
            Destroy(animal.gameObject);
            UpdateTemperament();
        }
    }

    public int GetTotalRequiredSpace()
    {
        int space = 0;
        foreach (var animal in animals)
        {
            if (animal != null && animal.data != null)
                space += animal.data.requiredSpacePerAnimal;
        }
        return space;
    }

    public int GetTotalTiles()
    {
        return (xMax - xMin + 1) * (yMax - yMin + 1);
    }

    public float CalculateCurrentTension()
    {
        if (globalMatrix == null) return 0f;

        float totalTension = 0f;
        int totalTiles = GetTotalTiles();
        int requiredSpace = GetTotalRequiredSpace();

        if (requiredSpace > totalTiles)
            totalTension += (requiredSpace - totalTiles) * globalMatrix.overcrowdingPenaltyPerTile;

        for (int i = 0; i < animals.Count; i++)
        {
            for (int j = i + 1; j < animals.Count; j++)
            {
                if (animals[i] == null || animals[j] == null) continue;
                AnimalData a = animals[i].data;
                AnimalData b = animals[j].data;
                if (a == null || b == null) continue;

                if (a.specificEnemies.Contains(b) || b.specificEnemies.Contains(a))
                    totalTension += globalMatrix.specificEnemyTension;
                else
                    totalTension += globalMatrix.GetTension(a.family, b.family);
            }
        }

        totalTension += cachedNeighborTension;

        return totalTension;
    }

    public void RefreshNeighborTension()
    {
        cachedNeighborTension = 0f;
        if (habitatSet == null || globalMatrix == null) return;
        foreach (var other in habitatSet.Items)
        {
            if (other == this || other == null) continue;
            if (!AreWithinRange(other, globalMatrix.neighborInfluenceRadius)) continue;
            cachedNeighborTension += globalMatrix.GetBiomeTension(biome, other.biome);
        }
    }

    private bool AreWithinRange(HabitatSpace other, int maxGap)
    {
        int gapX = Mathf.Max(0, Mathf.Max(xMin - other.xMax - 1, other.xMin - xMax - 1));
        int gapY = Mathf.Max(0, Mathf.Max(yMin - other.yMax - 1, other.yMin - yMax - 1));
        return gapX <= maxGap && gapY <= maxGap;
    }

    public void ResetAnimalStress()
    {
        foreach (var animal in animals)
            if (animal != null) animal.currentAnnoyance = 0f;
    }

    public void AddStressToAll(float amount)
    {
        foreach (var animal in animals)
            if (animal != null)
                animal.currentAnnoyance = Mathf.Clamp(animal.currentAnnoyance + amount, 0f, 100f);
    }

    public System.Collections.Generic.IReadOnlyList<Animal> GetAnimals() => animals;

    public Vector2? GetNearestViewingCell()
    {
        var candidates = PathManager.GetViewingCells(xMin, xMax, yMin, yMax);
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    public Vector3 GetWorldCenter()
    {
        float cs = GridCreator.Instance != null ? GridCreator.Instance.cellSize : 1f;
        return new Vector3((xMin + xMax + 1) * 0.5f * cs, 0f, (yMin + yMax + 1) * 0.5f * cs);
    }

    private void Update()
    {
        UpdateTemperament();
    }

    public void UpdateTemperament()
    {
        generalAnnoyance = CalculateAnnoyance();
    }

    private float CalculateAnnoyance()
    {
        if (animals.Count == 0) return 0f;

        float totalAnnoyance = 0;
        foreach (var animal in animals)
        {
            if (animal != null)
                totalAnnoyance += animal.currentAnnoyance;
        }
        return totalAnnoyance / animals.Count;
    }
}
