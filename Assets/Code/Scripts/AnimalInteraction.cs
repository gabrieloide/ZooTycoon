using UnityEngine;

public class AnimalInteraction : MonoBehaviour
{
    private Animal animal;
    private SphereCollider trigger;
    private bool playerInRange;
    private float calmTimer;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        animal = GetComponent<Animal>();
        trigger = GetComponent<SphereCollider>();
        if (trigger == null)
            trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = animal != null && animal.data != null ? animal.data.interactionRadius : 2f;
    }

    private void Update()
    {
        if (!playerInRange || animal == null || animal.hasEscaped) return;
        if (animal.currentAnnoyance <= 10f) { calmTimer = 0f; return; }

        calmTimer += Time.deltaTime;
        float required = animal.data != null ? animal.data.calmDurationRequired : 2f;
        if (calmTimer >= required)
        {
            calmTimer = 0f;
            PerformCalm();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        playerMovement = other.GetComponent<PlayerMovement>();

        if (animal != null && animal.hasEscaped)
            PerformRecapture();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        calmTimer = 0f;
    }

    private void PerformRecapture()
    {
        if (animal.data != null && playerMovement != null)
            playerMovement.stamina = Mathf.Max(0f, playerMovement.stamina - animal.data.calmStaminaCost);
        animal.Recapture();
    }

    private void PerformCalm()
    {
        if (animal.data == null) return;
        animal.currentAnnoyance = Mathf.Max(0f, animal.currentAnnoyance - animal.data.calmStressReduction);
        if (playerMovement != null)
            playerMovement.stamina = Mathf.Max(0f, playerMovement.stamina - animal.data.calmStaminaCost * 0.5f);
    }
}
