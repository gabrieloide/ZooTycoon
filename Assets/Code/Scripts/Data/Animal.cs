using System;
using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Data;

public class Animal : MonoBehaviour
{
    public AnimalData data;
    public HabitatSpace habitat;

    public float currentAnnoyance;
    public bool hasEscaped;

    [HideInInspector] public float purchaseCost;
    [HideInInspector] public int buildSession;

    private static readonly List<Animal> escapedAnimals = new();
    public static IReadOnlyList<Animal> EscapedAnimals => escapedAnimals;

    public float GetSellRefund(float staledRate)
    {
        float rate = buildSession == BuildController.CurrentSession ? 1f : staledRate;
        return purchaseCost * rate;
    }

    public static event Action<Animal> OnAnimalEscaped;
    public static event Action<Animal> OnAnimalRecaptured;

    private void Update()
    {
        if (!hasEscaped)
            UpdateAnnoyance();
    }

    private void UpdateAnnoyance()
    {
        if (habitat == null || data == null) return;

        float stress = 0f;

        if (data.requiredBiome != null && habitat.biome != null && habitat.biome != data.requiredBiome)
            stress += data.wrongBiomeStressRate;

        float tension = habitat.CalculateCurrentTension();
        if (tension > 0f)
            stress += tension * data.stressAccumulationRate;

        if (stress > 0f)
            currentAnnoyance += stress * Time.deltaTime;
        else
            currentAnnoyance -= data.annoyanceCalmRate * Time.deltaTime;

        currentAnnoyance = Mathf.Clamp(currentAnnoyance, 0f, 100f);

        if (currentAnnoyance >= 100f)
            TriggerEscape();
    }

    private void TriggerEscape()
    {
        hasEscaped = true;
        transform.SetParent(null);
        habitat?.BreakFence();
        escapedAnimals.Add(this);
        OnAnimalEscaped?.Invoke(this);
    }

    public void Recapture()
    {
        currentAnnoyance = 0f;
        hasEscaped = false;
        escapedAnimals.Remove(this);
        if (habitat != null)
        {
            transform.SetParent(habitat.transform);
            transform.position = GetHabitatWorldCenter();
            if (!habitat.HasEscapedAnimals())
                habitat.RepairFence();
        }
        OnAnimalRecaptured?.Invoke(this);
    }

    private void OnDestroy()
    {
        escapedAnimals.Remove(this);
    }

    private Vector3 GetHabitatWorldCenter()
    {
        float cellSize = GridCreator.Instance != null ? GridCreator.Instance.cellSize : 1f;
        float cx = (habitat.xMin + habitat.xMax + 1) * 0.5f * cellSize;
        float cz = (habitat.yMin + habitat.yMax + 1) * 0.5f * cellSize;
        return new Vector3(cx, 0f, cz);
    }
}
