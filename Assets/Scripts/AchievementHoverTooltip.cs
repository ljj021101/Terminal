using TMPro;
using UnityEngine;

public sealed class AchievementHoverTooltip : MonoBehaviour
{
    public static AchievementHoverTooltip Instance { get; private set; }

    [Header("Wiring")]
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Layout")]
    [SerializeField] private float maxWidth = 420f;
    [SerializeField] private Vector2 padding = new Vector2(24f, 16f); // x=左右总和, y=上下总和
    [SerializeField] private Vector2 screenOffset = new Vector2(-12f, 12f);

    [Header("Clamp")]
    [SerializeField] private bool clampToCanvas = true;
    [SerializeField] private Vector2 clampPadding = new Vector2(0f, 0f);

    private Canvas rootCanvas;
    private Camera uiCamera;
    private bool visible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        uiCamera = rootCanvas != null ? rootCanvas.worldCamera : null;

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        Hide();
    }

    public void Show(string message, Vector2 screenPos)
    {
        if (panelRect == null || canvasRect == null || tmp == null) return;

        tmp.enableWordWrapping = true;
        tmp.text = message ?? "";

        UpdateSizeToText();

        panelRect.gameObject.SetActive(true);
        visible = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        SetPosition(screenPos);
    }

    public void Hide()
    {
        visible = false;

        if (panelRect != null)
            panelRect.gameObject.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void SetPosition(Vector2 screenPos)
    {
        if (!visible) return;
        if (panelRect == null || canvasRect == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                uiCamera,
                out var localPoint
            )) return;

        panelRect.anchoredPosition = localPoint + screenOffset;

        if (clampToCanvas)
            ClampInsideCanvas();
    }

    private void UpdateSizeToText()
    {
        if (panelRect == null || tmp == null) return;

        tmp.ForceMeshUpdate();

        var pref = tmp.GetPreferredValues(tmp.text, maxWidth, 0f);
        float w = Mathf.Min(pref.x, maxWidth) + padding.x;
        float h = pref.y + padding.y;

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    private void ClampInsideCanvas()
    {
        var canvas = canvasRect.rect;
        var tip = panelRect.rect;
        var pivot = panelRect.pivot;

        float left = panelRect.anchoredPosition.x - tip.width * pivot.x;
        float right = left + tip.width;
        float bottom = panelRect.anchoredPosition.y - tip.height * pivot.y;
        float top = bottom + tip.height;

        float minX = canvas.xMin + clampPadding.x;
        float maxX = canvas.xMax - clampPadding.x;
        float minY = canvas.yMin + clampPadding.y;
        float maxY = canvas.yMax - clampPadding.y;

        float dx = 0f;
        float dy = 0f;

        if (left < minX) dx = minX - left;
        else if (right > maxX) dx = maxX - right;

        if (bottom < minY) dy = minY - bottom;
        else if (top > maxY) dy = maxY - top;

        panelRect.anchoredPosition += new Vector2(dx, dy);
    }
}
