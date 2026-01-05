using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class AchievementPanel : MonoBehaviour
{
    [Header("ScrollView")]
    [SerializeField] private Transform contentRoot;

    [Header("Dynamic Build")]
    [SerializeField] private AchievementsCatalog catalog;
    [SerializeField] private AchievementCard cardPrefab;
    [SerializeField] private bool rebuildOnEnable = true;

    private readonly List<AchievementCard> cards = new List<AchievementCard>();

    private void OnEnable()
    {
        AchievementsManager.Instance.Unlock("ach.first_open_achievements");

        if (rebuildOnEnable)
            RebuildFromCatalog();

        RefreshAll();
        StartCoroutine(RefreshNextFrame());

        var mgr = AchievementsManager.Instance;
        if (mgr != null)
        {
            mgr.OnUnlocked += HandleChanged;
            mgr.OnLocked += HandleChanged;
        }
    }

    private void OnDisable()
    {
        var mgr = AchievementsManager.Instance;
        if (mgr != null)
        {
            mgr.OnUnlocked -= HandleChanged;
            mgr.OnLocked -= HandleChanged;
        }
    }

    private IEnumerator RefreshNextFrame()
    {
        yield return null;
        RefreshAll();

        // 如果 ScrollView 有布局组件，让它下一帧也更新一下布局
        if (contentRoot is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    private void HandleChanged(string _)
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var c = cards[i];
            if (c == null) continue;
            c.Refresh();
        }
    }

    public void RebuildFromCatalog()
    {
        if (contentRoot == null)
        {
            Debug.LogWarning("[AchievementPanel] contentRoot is null, cannot build cards.");
            return;
        }
        if (catalog == null)
        {
            Debug.LogWarning("[AchievementPanel] catalog is null, cannot build cards.");
            return;
        }
        if (cardPrefab == null)
        {
            Debug.LogWarning("[AchievementPanel] cardPrefab is null, cannot build cards.");
            return;
        }

        // 清空旧子物体
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        cards.Clear();

        var ids = catalog.GetAllIdsSorted();
        if (ids.Count == 0)
        {
            Debug.LogWarning("[AchievementPanel] catalog has 0 ids.");
            return;
        }

        for (int i = 0; i < ids.Count; i++)
        {
            var card = Instantiate(cardPrefab, contentRoot);
            card.SetId(ids[i]);
            cards.Add(card);
        }
    }
}
