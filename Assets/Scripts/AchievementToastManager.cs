using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AchievementToastManager : MonoBehaviour
{
    [SerializeField] private AchievementToastUI toastUI;

    private readonly Queue<string> queue = new Queue<string>();
    private bool playing;

    private Coroutine subscribeRoutine;
    private AchievementsManager subscribedMgr;

    private struct PopupData
    {
        public Sprite icon;
        public string title;
        public string desc;
    }

    private readonly Dictionary<string, PopupData> dict = new Dictionary<string, PopupData>();

    private void Awake()
    {
        BuildDictionaryFromCards();

        // 可选：如果你热更新/运行时会新增卡片，也可以每次启用时再扫一次
        // BuildDictionaryFromCards();
    }

    private void OnEnable()
    {
        // 稳定订阅：等 Instance 出现后再订阅
        subscribeRoutine = StartCoroutine(SubscribeWhenReady());
    }

    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        if (subscribedMgr != null)
        {
            subscribedMgr.OnUnlocked -= HandleUnlocked;
            subscribedMgr = null;
        }
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (AchievementsManager.Instance == null)
            yield return null;

        subscribedMgr = AchievementsManager.Instance;
        subscribedMgr.OnUnlocked += HandleUnlocked;
    }

    private void HandleUnlocked(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        queue.Enqueue(id);
        if (!playing)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        playing = true;

        while (queue.Count > 0)
        {
            string id = queue.Dequeue();

            PopupData data;
            if (!dict.TryGetValue(id, out data))
            {
                // 找不到就给个兜底
                data.icon = null;
                data.title = "Achievement Unlocked";
                data.desc = id;
            }

            if (toastUI != null)
            {
                toastUI.Show(data.icon, data.title, data.desc);

                float duration = toastUI.TotalDuration;
                if (duration <= 0f) duration = 0.1f;

                // 稍微加一点缓冲，避免边界抖动
                yield return new WaitForSecondsRealtime(duration + 0.05f);
            }
            else
            {
                yield return null;
            }
        }

        playing = false;
    }

    private void BuildDictionaryFromCards()
    {
        dict.Clear();

        // 直接扫描场景里所有 AchievementCard（包括 inactive）
        var cards = FindObjectsByType<AchievementCard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            if (c == null) continue;

            var id = c.Id;
            if (string.IsNullOrWhiteSpace(id)) continue;
            if (dict.ContainsKey(id)) continue;

            dict[id] = new PopupData
            {
                icon = c.PopupIcon,
                title = c.PopupTitle,
                desc = c.PopupDesc
            };
        }
    }
}
