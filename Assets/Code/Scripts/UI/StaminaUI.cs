using UnityEngine;
using UnityEngine.UI;

namespace ZooTycoon.UI
{
    /// <summary>
    /// Radial 360° stamina indicator in World Space.
    /// Always faces the camera (billboard).
    /// Color transitions: Green → Yellow → Red.
    /// Fades in when sprinting, fades out when full.
    /// Pulses when stamina is critically low.
    /// </summary>
    public class StaminaUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The radial-filled Image that represents current stamina")]
        public Image staminaFillImage;

        [Tooltip("The background ring image (for glow effects)")]
        public Image staminaBackgroundImage;

        [Tooltip("Reference to the PlayerMovement script")]
        public PlayerMovement playerMovement;

        [Header("Color Settings")]
        [Tooltip("Color when stamina is full")]
        public Color fullColor = new Color(0.2f, 0.9f, 0.4f, 1f);

        [Tooltip("Color when stamina is at mid level")]
        public Color midColor = new Color(1f, 0.8f, 0.1f, 1f);

        [Tooltip("Color when stamina is low")]
        public Color lowColor = new Color(1f, 0.2f, 0.2f, 1f);

        [Tooltip("Stamina percentage threshold for 'low' state (0-1)")]
        [Range(0f, 1f)]
        public float lowThreshold = 0.3f;

        [Tooltip("Stamina percentage threshold for 'mid' state (0-1)")]
        [Range(0f, 1f)]
        public float midThreshold = 0.6f;

        [Header("Animation Settings")]
        [Tooltip("How fast the fill bar lerps to the target value")]
        public float fillLerpSpeed = 8f;

        [Tooltip("How fast the color transitions")]
        public float colorLerpSpeed = 6f;

        [Header("Visibility Settings")]
        [Tooltip("How fast the indicator fades in/out")]
        public float fadeLerpSpeed = 5f;

        [Tooltip("Delay before fading out after stamina is full")]
        public float fadeOutDelay = 1.5f;

        [Header("Pulse Animation")]
        [Tooltip("Enable pulse animation when stamina is low")]
        public bool enableLowPulse = true;

        [Tooltip("Speed of the pulse animation")]
        public float pulseSpeed = 3f;

        [Tooltip("Intensity of the pulse (scale multiplier)")]
        public float pulseIntensity = 0.08f;

        [Header("Glow Effect")]
        [Tooltip("Enable glow on the background ring")]
        public bool enableGlow = true;

        [Tooltip("Glow alpha intensity")]
        [Range(0f, 1f)]
        public float glowIntensity = 0.3f;

        // Internal state
        private float currentFillAmount;
        private float targetFillAmount;
        private Color currentColor;
        private Color targetColor;
        private float currentAlpha;
        private float targetAlpha;
        private float fullStaminaTimer;
        private CanvasGroup canvasGroup;
        private Vector3 originalScale;
        private Camera mainCam;

        void Start()
        {
            if (playerMovement == null)
            {
                playerMovement = GetComponentInParent<PlayerMovement>();
                if (playerMovement == null)
                {
                    Debug.LogError("StaminaUI: No PlayerMovement found!");
                    enabled = false;
                    return;
                }
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (staminaFillImage != null)
            {
                staminaFillImage.type = Image.Type.Filled;
                staminaFillImage.fillMethod = Image.FillMethod.Radial360;
                staminaFillImage.fillOrigin = (int)Image.Origin360.Top;
                staminaFillImage.fillClockwise = true;
            }

            currentFillAmount = 1f;
            targetFillAmount = 1f;
            currentColor = fullColor;
            targetColor = fullColor;
            currentAlpha = 0f;
            targetAlpha = 0f;
            canvasGroup.alpha = 0f;
            originalScale = transform.localScale;
            fullStaminaTimer = 0f;

            mainCam = Camera.main;
        }

        void LateUpdate()
        {
            if (playerMovement == null || staminaFillImage == null) return;

            // --- Billboard: always face the camera ---
            FaceCamera();

            // --- Calculate stamina ---
            float staminaPercent = playerMovement.stamina / playerMovement.maxStamina;
            targetFillAmount = staminaPercent;

            // --- Target color ---
            if (staminaPercent <= lowThreshold)
            {
                targetColor = lowColor;
            }
            else if (staminaPercent <= midThreshold)
            {
                float t = Mathf.InverseLerp(lowThreshold, midThreshold, staminaPercent);
                targetColor = Color.Lerp(lowColor, midColor, t);
            }
            else
            {
                float t = Mathf.InverseLerp(midThreshold, 1f, staminaPercent);
                targetColor = Color.Lerp(midColor, fullColor, t);
            }

            // --- Visibility ---
            bool staminaNotFull = staminaPercent < 0.999f;

            if (staminaNotFull)
            {
                targetAlpha = 1f;
                fullStaminaTimer = 0f;
            }
            else
            {
                fullStaminaTimer += Time.deltaTime;
                if (fullStaminaTimer >= fadeOutDelay)
                    targetAlpha = 0f;
            }

            // --- Smooth lerps ---
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * fillLerpSpeed);
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorLerpSpeed);
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeLerpSpeed);

            // --- Apply ---
            staminaFillImage.fillAmount = currentFillAmount;
            staminaFillImage.color = currentColor;

            if (enableGlow && staminaBackgroundImage != null)
            {
                Color glowColor = currentColor;
                glowColor.a = glowIntensity * currentAlpha;
                staminaBackgroundImage.color = glowColor;
            }

            canvasGroup.alpha = currentAlpha;

            // --- Pulse when low ---
            if (enableLowPulse && staminaPercent <= lowThreshold && currentAlpha > 0.1f)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * pulseIntensity;
                transform.localScale = originalScale * pulse;
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 10f);
            }
        }

        /// <summary>
        /// Makes the canvas always face the active camera.
        /// </summary>
        private void FaceCamera()
        {
            if (mainCam == null)
            {
                mainCam = Camera.main;
                if (mainCam == null) return;
            }

            // Look at camera but keep the same "up" so it doesn't tilt sideways
            transform.forward = mainCam.transform.forward;
        }
    }
}
