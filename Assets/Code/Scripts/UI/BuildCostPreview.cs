using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildCostPreview : MonoBehaviour
{
    [SerializeField] private float animDuration = 0.12f;
    [SerializeField] private Vector2 mouseOffset = new Vector2(16f, 16f);

    private static readonly Color colorAfford = new Color(1f, 1f, 1f);
    private static readonly Color colorCantAfford = new Color(1f, 0.25f, 0.25f);

    private VisualElement root;
    private VisualElement container;
    private Label costLabel;
    private bool wasVisible;
    private Tweener fadeTween;

    private void Start()
    {
        var doc = FindAnyObjectByType<UIDocument>();
        if (doc == null) return;
        root = doc.rootVisualElement;
        BuildElement();
    }

    private void BuildElement()
    {
        container = new VisualElement();
        container.style.position = Position.Absolute;
        container.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.82f);
        container.style.borderTopLeftRadius = 5;
        container.style.borderTopRightRadius = 5;
        container.style.borderBottomLeftRadius = 5;
        container.style.borderBottomRightRadius = 5;
        container.style.paddingTop = 4;
        container.style.paddingBottom = 4;
        container.style.paddingLeft = 8;
        container.style.paddingRight = 8;
        container.style.opacity = 0f;
        container.pickingMode = PickingMode.Ignore;

        costLabel = new Label();
        costLabel.style.fontSize = 13;
        costLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        costLabel.pickingMode = PickingMode.Ignore;

        container.Add(costLabel);
        root.Add(container);
    }

    private void Update()
    {
        if (container == null) return;

        if (BuildController.Instance == null || !BuildController.Instance.IsDraggingBiome)
        {
            if (wasVisible) FadeTo(0f);
            return;
        }

        var (cost, canAfford, isValid) = BuildController.Instance.GetBuildCostPreview();

        if (!wasVisible) FadeTo(1f);

        costLabel.text = $"${Mathf.RoundToInt(cost)}";
        costLabel.style.color = (canAfford && isValid) ? colorAfford : colorCantAfford;

        Vector2 mousePos = UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
        float x = mousePos.x + mouseOffset.x;
        float y = Screen.height - mousePos.y + mouseOffset.y;
        container.style.left = x;
        container.style.top = y;
    }

    private void FadeTo(float target)
    {
        wasVisible = target > 0f;
        fadeTween?.Kill();
        float current = container.style.opacity.value;
        fadeTween = DOTween.To(() => current, v =>
        {
            current = v;
            container.style.opacity = v;
        }, target, animDuration).SetEase(target > 0f ? Ease.OutQuad : Ease.InQuad);
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
        container?.RemoveFromHierarchy();
    }
}
