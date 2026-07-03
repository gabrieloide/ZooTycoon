using UnityEngine;
using ZooTycoon.Core;

public class DisasterManager : MonoBehaviour
{
    public static DisasterManager Instance { get; private set; }

    [SerializeField] private float escapePenalty = 500f;
    [SerializeField] private int visitorsLostOnEscape = 5;
    [SerializeField, Range(0f, 1f)] private float recaptureVisitorRecoveryRate = 0.5f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        Animal.OnAnimalEscaped += HandleEscape;
        Animal.OnAnimalRecaptured += HandleRecapture;
    }

    private void OnDisable()
    {
        Animal.OnAnimalEscaped -= HandleEscape;
        Animal.OnAnimalRecaptured -= HandleRecapture;
    }

    private void HandleEscape(Animal animal)
    {
        Debug.LogWarning($"[Disaster] {animal.data?.displayName ?? animal.name} escaped!");

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.Spend(escapePenalty);
            EconomyManager.Instance.RegisterDisasterLoss(escapePenalty);
        }

        if (VisitorManager.Instance != null)
            VisitorManager.Instance.ApplyEscapePenalty(visitorsLostOnEscape);
    }

    private void HandleRecapture(Animal animal)
    {
        Debug.Log($"[Disaster] {animal.data?.displayName ?? animal.name} recaptured.");

        if (VisitorManager.Instance != null)
        {
            int recovered = Mathf.RoundToInt(visitorsLostOnEscape * recaptureVisitorRecoveryRate);
            VisitorManager.Instance.RemoveEscapePenalty(recovered);
        }
    }
}
