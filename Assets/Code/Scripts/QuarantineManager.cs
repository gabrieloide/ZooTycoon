using System.Collections.Generic;
using UnityEngine;

public class QuarantineManager : MonoBehaviour
{
    public static QuarantineManager Instance { get; private set; }

    [SerializeField] private Transform pen;
    [SerializeField] private float penScatterRadius = 1.5f;

    private readonly List<Animal> quarantinedAnimals = new();
    public IReadOnlyList<Animal> QuarantinedAnimals => quarantinedAnimals;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SendToQuarantine(Animal animal)
    {
        if (animal == null || pen == null) return;

        if (animal.habitat != null)
        {
            var habitat = animal.habitat;
            habitat.RemoveAnimalKeepAlive(animal);
            if (!habitat.HasEscapedAnimals())
                habitat.RepairFence();
        }

        animal.habitat = null;
        animal.isQuarantined = true;

        var wandering = animal.GetComponent<AnimalWandering>();
        if (wandering != null) wandering.enabled = false;

        var interaction = animal.GetComponent<AnimalInteraction>();
        if (interaction != null) interaction.enabled = false;

        Vector2 offset = Random.insideUnitCircle * penScatterRadius;
        animal.transform.SetParent(pen);
        animal.transform.position = pen.position + new Vector3(offset.x, 0f, offset.y);

        quarantinedAnimals.Add(animal);
    }

    public void Release(Animal animal)
    {
        quarantinedAnimals.Remove(animal);
    }
}
