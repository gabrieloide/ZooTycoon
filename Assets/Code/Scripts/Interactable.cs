using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using ZooTycoon.Core;

public class Interactable : MonoBehaviour
{
    [SerializeField] private string label = "Interact [E]";
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private UnityEvent onExitRange;

    private bool playerInRange;

    private void OnEnable()
    {
        InputManager.Instance.actions.Player.Interact.performed += OnInteractPressed;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.actions.Player.Interact.performed -= OnInteractPressed;
    }

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;
        onInteract?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractPrompt.Instance?.Show(label, transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractPrompt.Instance?.Hide();
        onExitRange?.Invoke();
    }
}
