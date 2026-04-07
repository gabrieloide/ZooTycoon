using UnityEngine;
using UnityEngine.UI;

namespace ZooTycoon.UI
{
    /// <summary>
    /// Automatically creates the stamina ring UI in World Space next to the player.
    /// Attach this to the Player GameObject (alongside PlayerMovement).
    /// The ring will always face the camera (billboard).
    /// </summary>
    public class StaminaUISetup : MonoBehaviour
    {
        [Header("Ring Appearance")]
        [Tooltip("Outer radius of the ring in world units (scale of the canvas)")]
        public float ringWorldSize = 0.4f;

        [Tooltip("Thickness of the ring texture in pixels")]
        public int ringThickness = 10;

        [Tooltip("Resolution of the generated ring texture")]
        public int ringTextureSize = 128;

        [Header("Position")]
        [Tooltip("Offset from the player's pivot in local space")]
        public Vector3 localOffset = new Vector3(0.6f, 1.8f, 0f);

        [Header("Optional: Custom Ring Sprite")]
        [Tooltip("If assigned, uses this sprite instead of generating one")]
        public Sprite customRingSprite;

        private void Start()
        {
            CreateStaminaUI();
        }

        private void CreateStaminaUI()
        {
            Sprite ringSprite = customRingSprite != null ? customRingSprite : GenerateRingSprite(ringTextureSize, ringThickness);

            // --- Create World Space Canvas as child of player ---
            GameObject canvasGO = new GameObject("StaminaWorldCanvas");
            canvasGO.transform.SetParent(transform, false);
            canvasGO.transform.localPosition = localOffset;
            canvasGO.transform.localRotation = Quaternion.identity;

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(100f, 100f);
            canvasRect.localScale = Vector3.one * (ringWorldSize / 100f);

            // --- Background ring ---
            GameObject bgGO = new GameObject("StaminaBackground");
            bgGO.transform.SetParent(canvasGO.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.sprite = ringSprite;
            bgImage.color = new Color(1f, 1f, 1f, 0.15f);
            bgImage.type = Image.Type.Filled;
            bgImage.fillMethod = Image.FillMethod.Radial360;
            bgImage.fillAmount = 1f;
            bgImage.raycastTarget = false;
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // --- Fill ring ---
            GameObject fillGO = new GameObject("StaminaFill");
            fillGO.transform.SetParent(canvasGO.transform, false);
            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.sprite = ringSprite;
            fillImage.color = Color.white;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
            fillImage.fillClockwise = true;
            fillImage.fillAmount = 1f;
            fillImage.raycastTarget = false;
            RectTransform fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // --- Sprint icon in center ---
            GameObject iconGO = new GameObject("SprintIcon");
            iconGO.transform.SetParent(canvasGO.transform, false);
            Image iconImage = iconGO.AddComponent<Image>();
            Texture2D iconTex = GenerateSprintIcon(64);
            iconImage.sprite = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
            iconImage.color = new Color(1f, 1f, 1f, 0.7f);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.25f, 0.25f);
            iconRect.anchorMax = new Vector2(0.75f, 0.75f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // --- Attach StaminaUI component ---
            StaminaUI staminaUI = canvasGO.AddComponent<StaminaUI>();
            staminaUI.staminaFillImage = fillImage;
            staminaUI.staminaBackgroundImage = bgImage;
            staminaUI.playerMovement = GetComponent<PlayerMovement>();
        }

        /// <summary>
        /// Generates a procedural anti-aliased ring texture.
        /// </summary>
        private Sprite GenerateRingSprite(int size, int thickness)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size / 2f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius - thickness;
            float edgeSmooth = 1.5f;

            Color32[] pixels = new Color32[size * size];
            Color32 clear = new Color32(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float outerAlpha = Mathf.Clamp01((outerRadius - dist) / edgeSmooth);
                    float innerAlpha = Mathf.Clamp01((dist - innerRadius) / edgeSmooth);
                    float alpha = outerAlpha * innerAlpha;

                    pixels[y * size + x] = alpha > 0f
                        ? new Color32(255, 255, 255, (byte)(alpha * 255))
                        : clear;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Generates a simple lightning bolt icon.
        /// </summary>
        private Texture2D GenerateSprintIcon(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32 clear = new Color32(0, 0, 0, 0);
            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Vector2[] boltPoints = new Vector2[]
            {
                new Vector2(0.55f, 1.0f),
                new Vector2(0.35f, 0.55f),
                new Vector2(0.55f, 0.58f),
                new Vector2(0.40f, 0.0f),
                new Vector2(0.65f, 0.45f),
                new Vector2(0.45f, 0.42f),
            };

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / size;
                float leftX = GetBoltEdge(boltPoints, ny, true);
                float rightX = GetBoltEdge(boltPoints, ny, false);

                if (leftX < rightX)
                {
                    int x0 = Mathf.Clamp(Mathf.FloorToInt(leftX * size), 0, size - 1);
                    int x1 = Mathf.Clamp(Mathf.CeilToInt(rightX * size), 0, size - 1);
                    for (int x = x0; x <= x1; x++)
                        pixels[y * size + x] = new Color32(255, 255, 255, 255);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private float GetBoltEdge(Vector2[] points, float y, bool isLeft)
        {
            if (y >= 0.55f)
            {
                if (isLeft)
                {
                    float t = Mathf.InverseLerp(1.0f, 0.55f, y);
                    return Mathf.Lerp(points[0].x, points[1].x, t);
                }
                else
                {
                    float t = Mathf.InverseLerp(1.0f, 0.42f, y);
                    return Mathf.Lerp(points[0].x, points[5].x, t);
                }
            }
            else if (y >= 0.42f)
            {
                if (isLeft)
                {
                    float t = Mathf.InverseLerp(0.55f, 0.58f, y);
                    return Mathf.Lerp(points[1].x, points[2].x, Mathf.Clamp01(t));
                }
                else
                {
                    float t = Mathf.InverseLerp(0.45f, 0.42f, y);
                    return Mathf.Lerp(points[4].x, points[5].x, Mathf.Clamp01(t));
                }
            }
            else
            {
                if (isLeft)
                {
                    float t = Mathf.InverseLerp(0.58f, 0.0f, y);
                    return Mathf.Lerp(points[2].x, points[3].x, t);
                }
                else
                {
                    float t = Mathf.InverseLerp(0.45f, 0.0f, y);
                    return Mathf.Lerp(points[4].x, points[3].x, t);
                }
            }
        }
    }
}
