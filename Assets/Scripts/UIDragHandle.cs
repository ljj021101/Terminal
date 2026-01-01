using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UIDragHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform targetWindow;

    private UIWindow owner;
    private RectTransform parentRect;
    private Vector2 pointerOffset;

    public void Bind(UIWindow window)
    {
        owner = window;
        if (targetWindow == null) targetWindow = window.transform as RectTransform;
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
}
