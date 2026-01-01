using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ClickStar : MonoBehaviour, IPointerClickHandler
{
    [Header("Achievement")]
    [SerializeField] private string achievementId = "ach.click_star";

    [Header("Blink")]
    [SerializeField] private bool blinkWhenLocked = true;
    [SerializeField] private float blinkSpeed = 2.0f;
    [SerializeField] private float minAlpha = 0.35f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Target UI")]
    [SerializeField] private Graphic targetGraphic;

    private Coroutine blinkRoutine;
    private Coroutine subscribeRoutine;
    private AchievementsManager subscribedMgr;

    private void Awake()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        // 先停掉旧订阅流程，避免重复
        if (subscribeRoutine != null) StopCoroutine(subscribeRoutine);
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

        StopBlink();
    }

    private IEnumerator SubscribeWhenReady()
    {
        // 等 manager 初始化并完成 Load
        while (AchievementsManager.Instance == null)
            yield return null;

        subscribedMgr = AchievementsManager.Instance;
        subscribedMgr.OnUnlocked += HandleUnlocked;

        // 订阅成功后再刷新一次，保证“已解锁就不闪”
        UpdateBlinkState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
            return;

        AchievementsManager.Instance?.Unlock(achievementId);
        UpdateBlinkState();
    }

    private void HandleUnlocked(string id)
    {
        if (id != achievementId) return;
        UpdateBlinkState();
    }

    private void UpdateBlinkState()
    {
        if (!blinkWhenLocked)
        {
            StopBlink();
            SetAlpha(maxAlpha);
            return;
        }

        bool unlocked = AchievementsManager.Instance != null && AchievementsManager.Instance.IsUnlocked(achievementId);

        if (unlocked)
        {
            StopBlink();
            SetAlpha(maxAlpha);
        }
        else
        {
            StartBlink();
        }
    }

    private void StartBlink()
    {
        if (targetGraphic == null) return;
        if (blinkRoutine != null) return;

        blinkRoutine = StartCoroutine(BlinkLoop());
    }

    private void StopBlink()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }
    }

    private IEnumerator BlinkLoop()
    {
        SetAlpha(maxAlpha);

        while (true)
        {
            float t = useUnscaledTime ? Time.unscaledTime : Time.time;
            float s = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f * blinkSpeed);
            float a = Mathf.Lerp(minAlpha, maxAlpha, s);
            SetAlpha(a);
            yield return null;
        }
    }

    private void SetAlpha(float a)
    {
        if (targetGraphic == null) return;

        Color c = targetGraphic.color;
        c.a = a;
        targetGraphic.color = c;
    }
}
