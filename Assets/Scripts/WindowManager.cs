using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class WindowManager : MonoBehaviour
{
    public static WindowManager Instance { get; private set; }

    [Header("Scene References")]
    [SerializeField] private RectTransform windowsRoot;

    [Header("Focus")]
    [SerializeField] private float unfocusedAlpha = 0.85f;

    private readonly Dictionary<string, UIWindow> map = new Dictionary<string, UIWindow>();
    private readonly List<UIWindow> all = new List<UIWindow>();
    private UIWindow focused;

    // For global click-to-focus
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    public RectTransform WindowsRoot => windowsRoot;
    public float UnfocusedAlpha => unfocusedAlpha;
    public IReadOnlyList<UIWindow> AllWindows => all;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Rescan();
    }

    private void Update()
    {
        // Click anywhere on a window to bring it to front
        if (!Application.isPlaying) return;
        if (!Input.GetMouseButtonDown(0)) return;

        var es = EventSystem.current;
        if (es == null) return;

        raycastResults.Clear();
        var ped = new PointerEventData(es)
        {
            position = Input.mousePosition
        };

        es.RaycastAll(ped, raycastResults);
        if (raycastResults.Count == 0) return;

        for (int i = 0; i < raycastResults.Count; i++)
        {
            var go = raycastResults[i].gameObject;
            if (go == null) continue;

            var win = go.GetComponentInParent<UIWindow>();
            if (win == null) continue;

            if (!win.IsOpen) return;

            BringToFront(win);
            return;
        }
    }

    public void Rescan()
    {
        map.Clear();
        all.Clear();

        var wins = windowsRoot != null
            ? windowsRoot.GetComponentsInChildren<UIWindow>(true)
            : FindObjectsOfType<UIWindow>(true);

        for (int i = 0; i < wins.Length; i++)
        {
            var w = wins[i];
            if (w == null) continue;

            if (string.IsNullOrWhiteSpace(w.WindowId)) continue;

            w.Initialize(this);

            // If ids collide, later one wins (you can also Debug.LogWarning here)
            map[w.WindowId] = w;
            all.Add(w);
        }

        // Restore focus to the top-most open window if any
        focused = null;
        for (int i = all.Count - 1; i >= 0; i--)
        {
            if (all[i] != null && all[i].IsOpen)
            {
                SetFocused(all[i]);
                break;
            }
        }
    }

    public bool TryGet(string id, out UIWindow win) => map.TryGetValue(id, out win);

    public bool IsOpen(string id)
    {
        if (!map.TryGetValue(id, out var w) || w == null) return false;
        return w.IsOpen;
    }

    public UIWindow Open(string id)
    {
        if (!map.TryGetValue(id, out var w) || w == null) return null;

        w.Open();
        BringToFront(w);
        return w;
    }

    public void Close(string id)
    {
        if (!map.TryGetValue(id, out var w) || w == null) return;

        w.Close();

        if (focused == w)
        {
            focused = null;

            // 仅修改这里：按当前层级找“上一个窗口”（关闭窗口下面那一层）
            UIWindow next = null;
            int bestSibling = int.MinValue;

            for (int k = 0; k < all.Count; k++)
            {
                var it = all[k];
                if (it == null) continue;
                if (!it.IsOpen) continue;
                if (it == w) continue;

                int sib = it.transform.GetSiblingIndex();
                if (sib > bestSibling)
                {
                    bestSibling = sib;
                    next = it;
                }
            }

            if (next != null) SetFocused(next);
        }
    }

    public void Toggle(string id)
    {
        if (!map.TryGetValue(id, out var w) || w == null) return;

        if (w.IsOpen) Close(id);
        else Open(id);
    }

    public void BringToFront(UIWindow w)
    {
        if (w == null) return;

        w.transform.SetAsLastSibling();
        SetFocused(w);
    }

    public void OpenOrFocus(string id)
    {
        if (!map.TryGetValue(id, out var w) || w == null) return;

        if (w.IsOpen) BringToFront(w);
        else Open(id);
    }

    public void SetFocused(UIWindow w)
    {
        if (w == null) return;
        if (focused == w) return;

        focused = w;

        for (int i = 0; i < all.Count; i++)
        {
            var it = all[i];
            if (it == null) continue;
            if (!it.IsOpen) continue;

            it.SetFocused(it == focused);
        }
    }
}
