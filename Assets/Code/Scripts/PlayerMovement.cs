using UnityEngine;
using UnityEngine.InputSystem;
using ZooTycoon.Core;
using ZooTycoon.UI;

[RequireComponent(typeof(Rigidbody), typeof(StaminaUISetup))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 15f;
    private float currentSpeed;
    [Header("Components")]
    private Rigidbody rb;

    [Header("Stamina")]
    public float stamina = 100f;
    public float maxStamina = 100f;
    public float staminaDrain = 10f;
    public float staminaRegen = 10f;
    [Tooltip("Multiplier for stamina regen when the player is completely still")]
    public float idleRegenMultiplier = 2f;
    private bool tired = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = moveSpeed;
        stamina = maxStamina;
    }
    private void OnMove()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("PlayerMovement: GameManager is missing from the scene!");
            return;
        }

        if (GameManager.Instance.isBuildMode)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        if (InputManager.Instance == null || InputManager.Instance.actions == null)
        {
            Debug.LogError("PlayerMovement: InputManager is missing or actions are not initialized!");
            return;
        }

        Vector2 moveInput = InputManager.Instance.actions.Player.Move.ReadValue<Vector2>().normalized;

        OnRunning(ref currentSpeed, moveInput.magnitude);

        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

        rb.linearVelocity = new Vector3(movement.x * currentSpeed, rb.linearVelocity.y, movement.z * currentSpeed);

        if (movement.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }
    void OnRunning(ref float speed, float moveInput)
    {
        float sprintInput = InputManager.Instance.actions.Player.Sprint.ReadValue<float>();
        bool isTryingToSprint = sprintInput > 0.1f;
        bool isMoving = moveInput > 0.1f;

        if (tired && stamina >= maxStamina)
        {
            tired = false;
        }

        bool isActuallySprinting = false;

        if (isTryingToSprint && isMoving && !tired && stamina > 0f)
        {
            stamina -= staminaDrain * Time.deltaTime;
            isActuallySprinting = true;

            if (stamina <= 0f)
            {
                tired = true;
                isActuallySprinting = false;
            }
        }
        else
        {
            float currentRegen = tired ? (staminaRegen / 2f) : staminaRegen;

            // Regen faster when completely still
            if (!isMoving)
                currentRegen *= idleRegenMultiplier;

            stamina += currentRegen * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        if (isActuallySprinting)
        {
            speed = moveSpeed + sprintSpeed;
        }
        else if (tired)
        {
            speed = moveSpeed * 0.7f;
        }
        else
        {
            speed = moveSpeed;
        }
    }
    void FixedUpdate()
    {
        OnMove();
    }
}
