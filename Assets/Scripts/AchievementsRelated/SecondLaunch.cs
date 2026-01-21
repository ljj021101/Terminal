using System.Collections;
using UnityEngine;

public sealed class SecondLaunchAchievement : MonoBehaviour
{
    [SerializeField] private string achievementId = "ach.second_launch";
    [SerializeField] private string launchCountKey = "meta.launch_count";

    private void Awake()
    {
        // 启动就加 1
        int count = PlayerPrefs.GetInt(launchCountKey, 0);
        count += 1;
        PlayerPrefs.SetInt(launchCountKey, count);
        PlayerPrefs.Save();
    }

    private void OnEnable()
    {
        StartCoroutine(TryUnlock());
    }

    private IEnumerator TryUnlock()
    {
        while (AchievementsManager.Instance == null)
            yield return null;

        // 让出一帧，避免 toast 订阅抢跑
        yield return null;

        var mgr = AchievementsManager.Instance;
        if (mgr == null) yield break;

        if (mgr.IsUnlocked(achievementId)) yield break;

        int count = PlayerPrefs.GetInt(launchCountKey, 0);
        if (count == 2)
            mgr.Unlock(achievementId);
    }
}
