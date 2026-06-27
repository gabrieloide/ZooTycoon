using UnityEngine;
using UnityEngine.InputSystem;
using ZooTycoon.Core;

public class OfficeTrigger : MonoBehaviour
{
    [SerializeField] private InteractPrompt prompt;

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
        if (OfficeController.Instance != null)
            OfficeController.Instance.Show();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        prompt?.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        prompt?.Hide();
        if (OfficeController.Instance != null)
            OfficeController.Instance.Hide();
    }
}
