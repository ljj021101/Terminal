using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class DesktopIconButton : MonoBehaviour
{
    [SerializeField] private string windowId;
    [SerializeField] private WindowManager manager;

    private void Awake()
    {
        if (manager == null) manager = WindowManager.Instance;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            if (manager == null) return;
            if (string.IsNullOrWhiteSpace(windowId)) return;
            manager.OpenOrFocus(windowId);
        });
    }
}
