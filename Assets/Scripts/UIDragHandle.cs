using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UIDragHandle : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform targetWindow;

    [Header("Achievement")]
    [SerializeField] private bool enableExileAchievement = true;
    [SerializeField] private string exileAchievementId = "ach.fangzhu";

    [Header("Titlebar Clamp")]
    [Tooltip("If null, will use this handle's RectTransform as titlebar rect")]
    [SerializeField] private RectTransform titleBarRect;
    [Tooltip("How many pixels of the titlebar should remain visible at the top")]
    [SerializeField] private float titleBarMinVisiblePixels = 24f;

    private UIWindow owner;
    private RectTransform parentRect;
    private Vector2 pointerOffset;

    private static readonly Vector3[] worldCorners = new Vector3[4];

    public void Bind(UIWindow window)
    {
        owner = window;
        if (targetWindow == null) targetWindow = window.transform as RectTransform;
    }

    private void Awake()
    {
        if (titleBarRect == null)
            titleBarRect = transform as RectTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (owner != null) owner.RequestFocus();
        CacheOffset(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CacheOffset(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetWindow == null) return;

        if (parentRect == null)
        {
            parentRect = targetWindow.parent as RectTransform;
            if (parentRect == null) return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint
            )) return;

        targetWindow.anchoredPosition = localPoint - pointerOffset;

        // ✅ 关键：拖动过程中就保证 titlebar 还能抓住
        ClampTitleBarToTop(eventData.pressEventCamera);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (targetWindow == null) return;

        // 结束时再 clamp 一次，防止边界抖动
        ClampTitleBarToTop(eventData.pressEventCamera);

        if (!enableExileAchievement) return;

        if (IsFullyOutsideScreen(targetWindow, eventData.pressEventCamera))
        {
            var mgr = AchievementsManager.Instance;
            if (mgr != null && !string.IsNullOrWhiteSpace(exileAchievementId))
                mgr.Unlock(exileAchievementId);
        }
    }

    private void CacheOffset(PointerEventData eventData)
    {
        if (targetWindow == null) return;

        parentRect = targetWindow.parent as RectTransform;
        if (parentRect == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint
            )) return;

        pointerOffset = localPoint - targetWindow.anchoredPosition;
    }

    private void ClampTitleBarToTop(Camera eventCamera)
    {
        if (targetWindow == null) return;
        if (titleBarRect == null) return;

        // 计算 titlebar 在屏幕空间的最高/最低 y
        titleBarRect.GetWorldCorners(worldCorners);

        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[i]);
            if (sp.y < minY) minY = sp.y;
            if (sp.y > maxY) maxY = sp.y;
        }

        // 想要保证 titlebar 至少露出 titleBarMinVisiblePixels
        float allowedMaxY = Screen.height;                      // 顶边
        float desiredMinY = Screen.height - titleBarMinVisiblePixels;

        // 如果 titlebar 整体被顶出上边缘（或露出不足），往下推
        // 当 minY > desiredMinY 时，说明 titlebar底边太高（看不见/露出太少）
        if (minY > desiredMinY)
        {
            float deltaScreenY = minY - desiredMinY;           // 需要往下移多少屏幕像素

            // 把屏幕像素 delta 转成 parentRect 的 local delta
            // 用两个屏幕点映射到 parent 的 local 点，取差值
            if (parentRect == null)
                parentRect = targetWindow.parent as RectTransform;

            if (parentRect == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, Vector2.zero, eventCamera, out var local0
            );
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, new Vector2(0f, deltaScreenY), eventCamera, out var local1
            );

            float deltaLocalY = (local1 - local0).y;

            // 往下移动窗口
            targetWindow.anchoredPosition -= new Vector2(0f, deltaLocalY);
        }
    }

    private static bool IsFullyOutsideScreen(RectTransform rt, Camera eventCamera)
    {
        rt.GetWorldCorners(worldCorners);

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[i]);
            if (sp.x < minX) minX = sp.x;
            if (sp.x > maxX) maxX = sp.x;
            if (sp.y < minY) minY = sp.y;
            if (sp.y > maxY) maxY = sp.y;
        }

        if (maxX < 0f) return true;
        if (minX > Screen.width) return true;
        if (maxY < 0f) return true;
        if (minY > Screen.height) return true;

        return false;
    }
}
