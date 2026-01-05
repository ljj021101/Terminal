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

    private void OnEnable()
    {
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
            var data = ResolvePopupData(id);

            if (toastUI != null)
            {
                toastUI.Show(data.icon, data.title, data.desc);

                float duration = toastUI.TotalDuration;
                if (duration <= 0f) duration = 0.1f;

                yield return new WaitForSecondsRealtime(duration + 0.05f);
            }
            else
            {
                yield return null;
            }
        }

        playing = false;
    }

    private PopupData ResolvePopupData(string id)
    {
        var mgr = AchievementsManager.Instance;
        if (mgr != null && mgr.TryGetDefinition(id, out var def) && def != null)
        {
            return new PopupData
            {
                icon = def.icon,
                title = string.IsNullOrWhiteSpace(def.title) ? "Achievement Unlocked" : def.title,
                desc = string.IsNullOrWhiteSpace(def.description) ? id : def.description
            };
        }

        return new PopupData
        {
            icon = null,
            title = "Achievement Unlocked",
            desc = id
        };
    }
}
