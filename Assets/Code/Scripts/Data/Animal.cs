using UnityEngine;
using ZooTycoon.Data;

public class Animal : MonoBehaviour
{
    public AnimalData data;
    public HabitatSpace habitat;

    public float currentAnnoyance;

    public void UpdateAnnoyance()
    {
        currentAnnoyance += Random.Range(0f, 0.1f);
    }

    private void Update()
    {
        UpdateAnnoyance();
    }
}