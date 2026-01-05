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
    [Tooltip("Max width for the text area (not including padding).")]
    [SerializeField] private float maxWidth = 360f;

    [Tooltip("Total padding, x = left+right, y = top+bottom.")]
    [SerializeField] private Vector2 padding = new Vector2(24f, 16f);

    [Tooltip("Tooltip offset relative to the mouse position in canvas local space.")]
    [SerializeField] private Vector2 screenOffset = new Vector2(-12f, 12f);

    [Header("Clamp")]
    [SerializeField] private bool clampToCanvas = true;
    [SerializeField] private Vector2 clampPadding = new Vector2(8f, 8f);

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

        // 先激活，避免 ForceMeshUpdate / preferred size 为 0
        panelRect.gameObject.SetActive(true);
        visible = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        tmp.enableWordWrapping = true;
        tmp.text = message ?? "";

        UpdateSizeToText();
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

        float textMax = Mathf.Max(1f, maxWidth);

        // 关键：限制 TMP 文本容器宽度，让“实际渲染换行”和“GetPreferredValues 计算”一致
        tmp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textMax);

        tmp.ForceMeshUpdate();

        // 在 maxWidth 限制下计算内容理想尺寸
        Vector2 pref = tmp.GetPreferredValues(tmp.text, textMax, 0f);

        float contentW = Mathf.Min(pref.x, textMax);
        float panelW = contentW + padding.x;
        float panelH = pref.y + padding.y;

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelW);
        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelH);

        // 再把文本容器宽度设置为 panel 内部可用宽度
        float innerW = Mathf.Max(1f, panelW - padding.x);
        float innerH = Mathf.Max(1f, panelH - padding.y);

        tmp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, innerW);
        tmp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, innerH);
    }

    private void ClampInsideCanvas()
    {
        var canvas = canvasRect.rect;

        // 用 panel 的 pivot 做通用夹紧
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
