using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AchievementPanel : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    private readonly List<AchievementCard> cards = new List<AchievementCard>();

    private void Awake()
    {
        CacheCards();
    }

    private void OnEnable()
    {
        AchievementsManager.Instance.Unlock("ach.first_open_achievements");
        CacheCards();
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
    }

    public void CacheCards()
    {
        cards.Clear();
        if (contentRoot == null) return;
        contentRoot.GetComponentsInChildren(true, cards);
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

    private void HandleChanged(string _)
    {
        RefreshAll();
    }
}
