using UnityEngine;
using UnityEngine.InputSystem;
using ZooTycoon.Core;

public class BedTrigger : MonoBehaviour
{
    private bool playerInRange;

    private void OnEnable()
    {
        InputManager.Instance.actions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.actions.Player.Interact.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;
        if (SleepManager.Instance != null)
            SleepManager.Instance.TrySleep();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractPrompt.Instance?.Show("Sleep", transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractPrompt.Instance?.Hide();
    }
}
