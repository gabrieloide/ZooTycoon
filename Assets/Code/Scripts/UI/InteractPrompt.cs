using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteractPrompt : MonoBehaviour
{
    public static InteractPrompt Instance { get; private set; }

    [SerializeField] private GameObject promptRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text actionLabel;
    [SerializeField] private float animDuration = 0.18f;

    private Coroutine animCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }

    public void Show(string text)
    {
        if (actionLabel != null) actionLabel.text = text;
        if (promptRoot != null) promptRoot.SetActive(true);
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimRoutine(true));
    }

    public void Hide()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimRoutine(false));
    }

    private IEnumerator AnimRoutine(bool show)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;
        Vector3 startScale = promptRoot != null ? promptRoot.transform.localScale : Vector3.one;
        Vector3 targetScale = show ? Vector3.one : Vector3.one * 0.8f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            if (promptRoot != null)
                promptRoot.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        if (promptRoot != null)
            promptRoot.transform.localScale = targetScale;

        if (!show && promptRoot != null)
            promptRoot.SetActive(false);
    }
}
