using System.Collections;
using UnityEngine;

public sealed class TimeTravelAchievement : MonoBehaviour
{
    [Header("Achievement")]
    [SerializeField] private string achievementId = "ach.covid";

    [Header("Century Range")]
    [Tooltip("Inclusive")]
    [SerializeField] private int minYear = 1900;

    [Tooltip("Inclusive")]
    [SerializeField] private int maxYear = 1999;

    [Header("Behavior")]
    [SerializeField] private bool checkOnlyOncePerLaunch = true;

    private bool checkedThisLaunch;

    private void OnEnable()
    {
        if (checkOnlyOncePerLaunch && checkedThisLaunch) return;
        StartCoroutine(CheckWhenReady());
    }

    private IEnumerator CheckWhenReady()
    {
        while (AchievementsManager.Instance == null)
            yield return null;

        // 关键：让出一帧，给 AchievementToastManager 订阅 OnUnlocked 的机会
        yield return null;

        var mgr = AchievementsManager.Instance;
        if (mgr == null) yield break;

        if (mgr.IsUnlocked(achievementId)) yield break;

        int year = System.DateTime.Now.Year;
        if (year >= minYear && year <= maxYear)
            mgr.Unlock(achievementId);
    }
}
