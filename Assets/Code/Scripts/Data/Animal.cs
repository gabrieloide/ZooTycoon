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
    public int escapeCount;
    public bool isQuarantined;

    private float recaptureGraceTimer;

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

        if (recaptureGraceTimer > 0f)
        {
            recaptureGraceTimer -= Time.deltaTime;
            currentAnnoyance = Mathf.Max(0f, currentAnnoyance - data.annoyanceCalmRate * Time.deltaTime);
            return;
        }

        float traumaMultiplier = 1f + escapeCount * data.traumaStressRatePerEscape;
        float stress = 0f;

        if (data.requiredBiome != null && habitat.biome != null && habitat.biome != data.requiredBiome)
            stress += data.wrongBiomeStressRate * traumaMultiplier;

        float tension = habitat.CalculateCurrentTension();
        if (tension > 0f)
            stress += tension * data.stressAccumulationRate * traumaMultiplier;

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
        escapeCount++;
        transform.SetParent(null);
        habitat?.BreakFence();
        escapedAnimals.Add(this);
        OnAnimalEscaped?.Invoke(this);
    }

    public void Recapture()
    {
        currentAnnoyance = 0f;
        hasEscaped = false;
        recaptureGraceTimer = data != null ? data.postRecaptureGraceDuration : 0f;
        escapedAnimals.Remove(this);

        if (data != null && escapeCount >= data.wildAfterEscapeCount)
            QuarantineManager.Instance?.SendToQuarantine(this);
        else
            ReturnToHabitat();

        OnAnimalRecaptured?.Invoke(this);
    }

    private void ReturnToHabitat()
    {
        if (habitat == null) return;
        transform.SetParent(habitat.transform);
        transform.position = GetHabitatWorldCenter();
        if (!habitat.HasEscapedAnimals())
            habitat.RepairFence();
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
