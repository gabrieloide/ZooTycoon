using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UIButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    // ── Configuration ──────────────────────────────────────────

    [Header("── Interactable ──")]
    [SerializeField] private bool interactable = true;

    [Header("── Target Graphic ──")]
    [Tooltip("Image/Text that receives color tinting. Auto-detected if empty.")]
    [SerializeField] private Graphic targetGraphic;

    [Header("── Scale Animation ──")]
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float scaleDuration = 0.15f;
    [SerializeField] private Ease pressEase = Ease.OutCubic;
    [SerializeField] private Ease releaseEase = Ease.OutBack;

    [Header("── Color Tint ──")]
    [SerializeField] private bool useColorTint = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
    [SerializeField] private Color selectedColor = new Color(0.85f, 0.9f, 1f, 1f);
    [SerializeField] private float colorDuration = 0.1f;

    [Header("── Disabled Shake ──")]
    [Tooltip("Shake strength when clicking a disabled button.")]
    [SerializeField] private float shakeStrength = 6f;
    [SerializeField] private float shakeDuration = 0.35f;

    [Header("── Audio ──")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip disabledClickSound;
    [Tooltip("Auto-detected on this GameObject if empty.")]
    [SerializeField] private AudioSource audioSource;

    [Header("── Cooldown ──")]
    [SerializeField] private bool useCooldown = false;
    [SerializeField] private float cooldownTime = 0.3f;

    [Header("── Events ──")]
    public UnityEvent OnClick;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;
    public UnityEvent OnDisabledClick;

    // ── Runtime State ──────────────────────────────────────────

    private bool isHovered;
    private bool isPressed;
    private bool isSelected;
    private bool isOnCooldown;
    private Vector3 originalScale;

    private Tween scaleTween;
    private Tween colorTween;
    private Tween shakeTween;
    private Tween cooldownTween;

    // ── Public Properties ──────────────────────────────────────

    /// <summary>
    /// Enable or disable the button. Automatically updates visuals.
    /// </summary>
    public bool Interactable
    {
        get => interactable;
        set
        {
            if (interactable == value) return;
            interactable = value;

            // Reset interaction flags when disabling
            if (!interactable)
            {
                isHovered = false;
                isPressed = false;
            }

            UpdateVisualState();
        }
    }

    /// <summary>
    /// Toggle-style selection state. Useful for tab bars or toggle groups.
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value) return;
            isSelected = value;
            UpdateVisualState();
        }
    }

    // ── Lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        originalScale = transform.localScale;

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        transform.localScale = originalScale;
        UpdateVisualState(instant: true);
    }

    private void OnDisable()
    {
        KillAllTweens();
        transform.localScale = originalScale;
        isHovered = false;
        isPressed = false;
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

    // ── Pointer Events ─────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (!interactable) return;

        PlaySound(hoverSound);
        OnHoverEnter?.Invoke();
        UpdateVisualState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        if (!interactable) return;

        OnHoverExit?.Invoke();
        UpdateVisualState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable)
        {
            HandleDisabledClick();
            return;
        }

        isPressed = true;
        UpdateVisualState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;

        // Only fire click if pointer is still over the button
        if (isPressed && isHovered)
            ExecuteClick();

        isPressed = false;
        UpdateVisualState();
    }

    // ── Core Logic ─────────────────────────────────────────────

    private void ExecuteClick()
    {
        if (useCooldown && isOnCooldown) return;

        PlaySound(clickSound);
        OnClick?.Invoke();

        if (useCooldown)
        {
            isOnCooldown = true;
            cooldownTween?.Kill();
            cooldownTween = DOVirtual.DelayedCall(cooldownTime, () => isOnCooldown = false)
                .SetUpdate(true);
        }
    }

    private void HandleDisabledClick()
    {
        PlaySound(disabledClickSound);
        OnDisabledClick?.Invoke();

        // Shake feedback so the player knows the button is disabled
        if (useScaleAnimation)
        {
            shakeTween?.Kill();
            shakeTween = transform
                .DOShakePosition(shakeDuration, shakeStrength, 15, 90f, false, true, ShakeRandomnessMode.Harmonic)
                .SetUpdate(true);
        }
    }

    // ── Visual State Machine ───────────────────────────────────

    private void UpdateVisualState(bool instant = false)
    {
        if (!interactable)
        {
            ApplyState(originalScale, disabledColor, instant);
            return;
        }

        if (isPressed)
        {
            ApplyState(originalScale * pressedScale, pressedColor, instant, pressEase);
            return;
        }

        if (isHovered)
        {
            ApplyState(originalScale * hoverScale, hoverColor, instant, releaseEase);
            return;
        }

        if (isSelected)
        {
            ApplyState(originalScale, selectedColor, instant, releaseEase);
            return;
        }

        // Normal
        ApplyState(originalScale, normalColor, instant, releaseEase);
    }

    private void ApplyState(Vector3 scale, Color color, bool instant, Ease ease = Ease.OutCubic)
    {
        // ── Scale ──
        if (useScaleAnimation)
        {
            scaleTween?.Kill();

            if (instant)
            {
                transform.localScale = scale;
            }
            else
            {
                scaleTween = transform
                    .DOScale(scale, scaleDuration)
                    .SetEase(ease)
                    .SetUpdate(true);
            }
        }

        // ── Color ──
        if (useColorTint && targetGraphic != null)
        {
            colorTween?.Kill();

            if (instant)
            {
                targetGraphic.color = color;
            }
            else
            {
                colorTween = targetGraphic
                    .DOColor(color, colorDuration)
                    .SetUpdate(true);
            }
        }
    }

    // ── Audio ──────────────────────────────────────────────────

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    // ── Tween Cleanup ──────────────────────────────────────────

    private void KillAllTweens()
    {
        scaleTween?.Kill();
        colorTween?.Kill();
        shakeTween?.Kill();
        cooldownTween?.Kill();

        scaleTween = null;
        colorTween = null;
        shakeTween = null;
        cooldownTween = null;
    }
}