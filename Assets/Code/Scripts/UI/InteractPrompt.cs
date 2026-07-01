using DG.Tweening;
using TMPro;
using UnityEngine;

public class InteractPrompt : MonoBehaviour
{
    public static InteractPrompt Instance { get; private set; }

    [SerializeField] private GameObject promptRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private float animDuration = 0.18f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, 0f);

    private Transform currentTarget;

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
        {
            promptRoot.transform.localScale = Vector3.one * 0.8f;
            promptRoot.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (promptRoot == null || !promptRoot.activeSelf || Camera.main == null) return;

        if (currentTarget != null)
            promptRoot.transform.position = currentTarget.position + offset;

        promptRoot.transform.rotation = Camera.main.transform.rotation;
    }

    public void Show(string text, Transform target)
    {
        currentTarget = target;

        if (actionLabel != null) actionLabel.text = text;
        if (promptRoot == null || canvasGroup == null) return;

        promptRoot.transform.position = target.position + offset;
        promptRoot.SetActive(true);

        DOTween.Kill(canvasGroup);
        DOTween.Kill(promptRoot.transform);

        canvasGroup.DOFade(1f, animDuration).SetEase(Ease.OutQuad);
        promptRoot.transform.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        currentTarget = null;
        if (promptRoot == null || canvasGroup == null) return;

        DOTween.Kill(canvasGroup);
        DOTween.Kill(promptRoot.transform);

        canvasGroup.DOFade(0f, animDuration).SetEase(Ease.InQuad);
        promptRoot.transform.DOScale(Vector3.one * 0.8f, animDuration).SetEase(Ease.InQuad)
            .OnComplete(() => promptRoot.SetActive(false));
    }
}
