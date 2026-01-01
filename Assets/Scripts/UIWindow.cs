using UnityEngine;
using UnityEngine.UI;

public sealed class UIWindow : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string windowId;

    [Header("Wiring")]
    [SerializeField] private UIDragHandle dragHandle;
    [SerializeField] private Button closeButton;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Focus Style")]
    [SerializeField] private float unfocusedAlpha = 0.85f;

    [Header("Layout")]
    [SerializeField] private bool applyLayout = true;

    [Tooltip("If left empty, will use this GameObject's RectTransform")]
    [SerializeField] private RectTransform windowRect;

    [SerializeField] private RectTransform titleBarRect;
    [SerializeField] private RectTransform contentRootRect;

    [SerializeField] private Vector2 windowSize = new Vector2(760f, 520f);
    [SerializeField] private float titleBarHeight = 56f;

    [Tooltip("x=Left, y=Right, z=Bottom, w=Top")]
    [SerializeField] private Vector4 contentPadding = new Vector4(12f, 12f, 12f, 12f);

    [Header("Title Bar Content")]
    [SerializeField] private RectTransform titleTextRect;
    [SerializeField] private RectTransform closeButtonRect;

    [SerializeField] private float titleLeftPadding = 14f;
    [SerializeField] private float titleRightPadding = 10f;

    [SerializeField] private Vector2 closeButtonSize = new Vector2(36f, 36f);
    [SerializeField] private Vector2 closeButtonPadding = new Vector2(10f, 10f);

    private WindowManager manager;

    public string WindowId => windowId;
    public bool IsOpen { get; private set; }

    private void Reset()
    {
        windowRect = transform as RectTransform;
    }

    private void OnValidate()
    {
        if (!applyLayout) return;
        ApplyLayoutNow();
    }

    public void Initialize(WindowManager mgr)
    {
        manager = mgr;

        if (windowRect == null) windowRect = transform as RectTransform;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (dragHandle != null) dragHandle.Bind(this);

        // 重要：如果你已经做了“关闭回到上一个窗口”的修复
        // 就应该走 manager.Close(windowId)，而不是直接 Close()
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                if (manager != null && !string.IsNullOrWhiteSpace(windowId))
                    manager.Close(windowId);
                else
                    Close();
            });
        }

        if (applyLayout) ApplyLayoutNow();

        if (gameObject.activeSelf)
        {
            IsOpen = true;
            SetFocused(false);
        }
        else
        {
            IsOpen = false;
        }
    }

    public void Open()
    {
        IsOpen = true;
        gameObject.SetActive(true);

        if (applyLayout) ApplyLayoutNow();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Close()
    {
        IsOpen = false;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void RequestFocus()
    {
        if (manager != null) manager.BringToFront(this);
    }

    public void SetFocused(bool focused)
    {
        if (!IsOpen) return;
        canvasGroup.alpha = focused ? 1f : unfocusedAlpha;
    }

    private void ApplyLayoutNow()
    {
        if (windowRect == null) windowRect = transform as RectTransform;
        if (windowRect == null) return;

        // 1) Window size from Inspector
        windowRect.sizeDelta = windowSize;

        // 2) TitleBar: stick to top, stretch width, fixed height
        if (titleBarRect != null)
        {
            titleBarRect.anchorMin = new Vector2(0f, 1f);
            titleBarRect.anchorMax = new Vector2(1f, 1f);
            titleBarRect.pivot = new Vector2(0.5f, 1f);

            titleBarRect.anchoredPosition = Vector2.zero;
            titleBarRect.sizeDelta = new Vector2(0f, titleBarHeight);
        }

        // 3) ContentRoot: fill remaining space under title bar, with padding
        if (contentRootRect != null)
        {
            contentRootRect.anchorMin = new Vector2(0f, 0f);
            contentRootRect.anchorMax = new Vector2(1f, 1f);
            contentRootRect.pivot = new Vector2(0.5f, 0.5f);

            float left = contentPadding.x;
            float right = contentPadding.y;
            float bottom = contentPadding.z;
            float top = contentPadding.w;

            contentRootRect.offsetMin = new Vector2(left, bottom);
            contentRootRect.offsetMax = new Vector2(-right, -(titleBarHeight + top));
        }

        // 4) CloseButton: top-right, fixed size with padding
        if (closeButtonRect != null && titleBarRect != null)
        {
            closeButtonRect.anchorMin = new Vector2(1f, 1f);
            closeButtonRect.anchorMax = new Vector2(1f, 1f);
            closeButtonRect.pivot = new Vector2(1f, 1f);

            closeButtonRect.sizeDelta = closeButtonSize;
            closeButtonRect.anchoredPosition = new Vector2(-closeButtonPadding.x, -closeButtonPadding.y);
        }

        // 5) TitleText: stretch inside title bar, leave space for close button
        if (titleTextRect != null && titleBarRect != null)
        {
            titleTextRect.anchorMin = new Vector2(0f, 0f);
            titleTextRect.anchorMax = new Vector2(1f, 1f);
            titleTextRect.pivot = new Vector2(0f, 0.5f);

            float reservedRight = titleRightPadding;
            if (closeButtonRect != null)
                reservedRight += closeButtonSize.x + closeButtonPadding.x;

            // offsetMin = (left, bottom), offsetMax = (-right, -top)
            titleTextRect.offsetMin = new Vector2(titleLeftPadding, 0f);
            titleTextRect.offsetMax = new Vector2(-reservedRight, 0f);
        }
    }
}
