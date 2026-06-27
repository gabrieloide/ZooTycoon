using UnityEngine;

public class AnimalStressBar : MonoBehaviour
{
    [SerializeField] private float heightOffset = 1.5f;
    [SerializeField] private float barWidth = 0.8f;
    [SerializeField] private float barHeight = 0.1f;

    private static readonly Color ColorLow = Color.green;
    private static readonly Color ColorMid = Color.yellow;
    private static readonly Color ColorHigh = Color.red;

    private Animal animal;
    private Transform barPivot;
    private Transform fillTransform;
    private Material fillMaterial;

    private void Awake()
    {
        animal = GetComponent<Animal>();
        BuildBar();
    }

    private void BuildBar()
    {
        var container = new GameObject("StressBar");
        container.transform.SetParent(transform, false);
        container.transform.localPosition = Vector3.up * heightOffset;
        barPivot = container.transform;

        var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "BG";
        bg.transform.SetParent(barPivot, false);
        bg.transform.localScale = new Vector3(barWidth, barHeight, 0.02f);
        bg.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);
        Destroy(bg.GetComponent<Collider>());

        var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "Fill";
        fill.transform.SetParent(barPivot, false);
        fill.transform.localScale = new Vector3(0f, barHeight * 0.8f, 0.025f);
        fill.transform.localPosition = new Vector3(-barWidth * 0.5f, 0f, -0.015f);
        fillTransform = fill.transform;
        fillMaterial = fill.GetComponent<Renderer>().material;
        fillMaterial.color = ColorLow;
        Destroy(fill.GetComponent<Collider>());
    }

    private void LateUpdate()
    {
        if (animal == null || barPivot == null) return;

        if (Camera.main != null)
            barPivot.rotation = Camera.main.transform.rotation;

        float t = Mathf.Clamp01(animal.currentAnnoyance / 100f);

        float width = barWidth * t;
        fillTransform.localScale = new Vector3(width, barHeight * 0.8f, 0.025f);
        fillTransform.localPosition = new Vector3(-barWidth * 0.5f + width * 0.5f, 0f, -0.015f);

        Color barColor = t < 0.5f
            ? Color.Lerp(ColorLow, ColorMid, t * 2f)
            : Color.Lerp(ColorMid, ColorHigh, (t - 0.5f) * 2f);

        fillMaterial.color = barColor;
    }

    private void OnDestroy()
    {
        if (fillMaterial != null)
            Destroy(fillMaterial);
    }
}
