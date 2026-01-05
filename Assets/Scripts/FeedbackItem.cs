using TMPro;
using UnityEngine;

public sealed class SequenceFeedbackItem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text text;

    [Header("Common")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private RectTransform canvasRect; // 用来判断飞出屏幕后销毁
    [SerializeField] private float killPadding = 120f;

    [Header("Success Fly")]
    [SerializeField] private float successUpSpeed = 520f;
    [SerializeField] private float successFadePerSecond = 1.2f;

    [Header("Fail Toss")]
    [SerializeField] private float failLaunchSpeed = 520f;
    [Tooltip("角度以 +X 轴为 0°，90°为正上。比如 60~120 就是扇形向上喷")]
    [SerializeField] private float failAngleMin = 60f;
    [SerializeField] private float failAngleMax = 120f;
    [SerializeField] private float gravity = 1600f;
    [SerializeField] private float failFadePerSecond = 0.35f;

    private bool success;
    private Vector2 velocity;

    public void Play(
        string message,
        Vector2 anchoredStartPos,
        bool isSuccess,
        RectTransform canvasRectOverride = null
    )
    {
        if (rect == null) rect = transform as RectTransform;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);

        canvasRect = canvasRectOverride != null ? canvasRectOverride : canvasRect;

        if (text != null) text.text = message ?? "";
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        rect.anchoredPosition = anchoredStartPos;

        success = isSuccess;
        if (success)
        {
            velocity = Vector2.up * successUpSpeed;
        }
        else
        {
            float a0 = Mathf.Min(failAngleMin, failAngleMax);
            float a1 = Mathf.Max(failAngleMin, failAngleMax);
            float ang = Random.Range(a0, a1) * Mathf.Deg2Rad;

            velocity = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * failLaunchSpeed;
        }
    }

    private void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        if (rect == null) return;

        if (success)
        {
            rect.anchoredPosition += velocity * dt;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - successFadePerSecond * dt);
            }
        }
        else
        {
            velocity.y -= gravity * dt;
            rect.anchoredPosition += velocity * dt;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - failFadePerSecond * dt);
            }
        }

        if (IsOffscreenOrInvisible())
            Destroy(gameObject);
    }

    private bool IsOffscreenOrInvisible()
    {
        if (canvasGroup != null && canvasGroup.alpha <= 0.01f) return true;
        if (canvasRect == null || rect == null) return false;

        var c = canvasRect.rect;
        var p = rect.anchoredPosition;

        if (p.x < c.xMin - killPadding) return true;
        if (p.x > c.xMax + killPadding) return true;
        if (p.y < c.yMin - killPadding) return true;
        if (p.y > c.yMax + killPadding) return true;

        return false;
    }
}
