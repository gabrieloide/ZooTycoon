using UnityEngine;
using ZooTycoon.Data;

public class Animal : MonoBehaviour
{
    public AnimalData data;
    public HabitatSpace habitat;

    public float currentAnnoyance;

    public void UpdateAnnoyance()
    {
        if (habitat == null || data == null) return;

        float tension = habitat.CalculateCurrentTension();
        if (tension > 0)
            currentAnnoyance += tension * data.stressAccumulationRate * Time.deltaTime;
        else
            currentAnnoyance -= data.annoyanceCalmRate * Time.deltaTime;

        currentAnnoyance = Mathf.Clamp(currentAnnoyance, 0f, 100f);
    }

    private void Update()
    {
        UpdateAnnoyance();
    }
}
