using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AchievementToastUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;

    [Header("Animation")]
    [SerializeField] private float fadeInSeconds = 0.25f;
    [SerializeField] private float holdSeconds = 2.5f;
    [SerializeField] private float fadeOutSeconds = 0.35f;

    [Tooltip("Slide from offset to 0 during fade in, and back during fade out")]
    [SerializeField] private Vector2 slideOffset = new Vector2(40f, 0f);

    private Coroutine routine;

    public float TotalDuration => Mathf.Max(0f, fadeInSeconds) + Mathf.Max(0f, holdSeconds) + Mathf.Max(0f, fadeOutSeconds);

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = transform as RectTransform;
    }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (rect == null) rect = transform as RectTransform;

        // 只把透明度设为 0，别在这里 SetActive(false)
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
    public void Show(Sprite icon, string title, string desc)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Play(icon, title, desc));
    }

    private IEnumerator Play(Sprite icon, string title, string desc)
    {
        if (canvasGroup == null) yield break;
        if (rect == null) rect = transform as RectTransform;

        if (iconImage != null) iconImage.sprite = icon;
        if (titleText != null) titleText.text = title;
        if (descText != null) descText.text = desc;

        float t = 0f;
        canvasGroup.alpha = 0f;

        Vector2 endPos = rect.anchoredPosition;
        Vector2 startPos = endPos + slideOffset;
        rect.anchoredPosition = startPos;

        // Fade in
        while (t < fadeInSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = fadeInSeconds <= 0f ? 1f : Mathf.Clamp01(t / fadeInSeconds);

            canvasGroup.alpha = k;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, k);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rect.anchoredPosition = endPos;

        // Hold
        float wait = holdSeconds;
        while (wait > 0f)
        {
            wait -= Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < fadeOutSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = fadeOutSeconds <= 0f ? 1f : Mathf.Clamp01(t / fadeOutSeconds);

            canvasGroup.alpha = 1f - k;
            rect.anchoredPosition = Vector2.Lerp(endPos, startPos, k);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        rect.anchoredPosition = endPos;

        gameObject.SetActive(false);
        routine = null;
    }
}
