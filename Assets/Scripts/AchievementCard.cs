using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class AchievementCard : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerMoveHandler,
    IPointerClickHandler
{
    [Header("Runtime")]
    [SerializeField] private string achievementId;

    [Header("Refs")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;

    [Header("Locked Style")]
    [SerializeField] private string lockedName = "???";
    [SerializeField] private Color lockedIconTint = Color.gray;

    [Header("Hover Tip")]
    [SerializeField] private bool showHoverOnlyWhenLocked = true;
    [SerializeField] private bool followMouse = true;

    public string Id => achievementId;

    private bool hovering;

    public void SetId(string id)
    {
        achievementId = id;
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        hovering = false;
        if (AchievementHoverTooltip.Instance != null)
            AchievementHoverTooltip.Instance.Hide();
    }

    public void Refresh()
    {
        var mgr = AchievementsManager.Instance;
        bool unlocked = mgr != null && mgr.IsUnlocked(achievementId);

        AchievementDefinition def = null;
        if (mgr != null)
            mgr.TryGetDefinition(achievementId, out def);

        Apply(unlocked, def);

        if (hovering)
            TryShowOrUpdateTip(unlocked, def);
    }

    private void Apply(bool unlocked, AchievementDefinition def)
    {
        var icon = def != null ? def.icon : null;
        var title = def != null && !string.IsNullOrWhiteSpace(def.title) ? def.title : achievementId;
        var desc = def != null ? def.description : "";

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = unlocked ? Color.white : lockedIconTint;
        }

        if (nameText != null)
        {
            string lockedTitle = lockedName;
            if (def != null && !string.IsNullOrWhiteSpace(def.lockedTitleOverride))
                lockedTitle = def.lockedTitleOverride;

            nameText.text = unlocked ? title : lockedTitle;
        }

        if (descText != null)
        {
            descText.text = unlocked ? desc : "";
            descText.gameObject.SetActive(unlocked && !string.IsNullOrEmpty(desc));
        }
    }

    private void ApplyFallback(bool unlocked)
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = unlocked ? Color.white : lockedIconTint;
        }

        if (nameText != null)
            nameText.text = unlocked ? achievementId : lockedName;

        if (descText != null)
        {
            descText.text = "";
            descText.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        var mgr = AchievementsManager.Instance;
        bool unlocked = mgr != null && mgr.IsUnlocked(achievementId);

        AchievementDefinition def = null;
        if (mgr != null)
            mgr.TryGetDefinition(achievementId, out def);

        TryShowOrUpdateTip(unlocked, def, eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!hovering) return;
        if (!followMouse) return;

        var tip = AchievementHoverTooltip.Instance;
        if (tip == null) return;

        tip.SetPosition(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;

        var tip = AchievementHoverTooltip.Instance;
        if (tip != null)
            tip.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        var mgr = AchievementsManager.Instance;
        if (mgr == null) return;

        if (mgr.IsUnlocked(achievementId)) return;

        AchievementDefinition def = null;
        if (!mgr.TryGetDefinition(achievementId, out def) || def == null)
            return;

        if (!def.clickToUnlockWhenLocked)
            return;

        if (mgr.Unlock(achievementId))
        {
            Refresh();

            if (AchievementHoverTooltip.Instance != null)
                AchievementHoverTooltip.Instance.Hide();
        }
    }

    private void TryShowOrUpdateTip(bool unlocked, AchievementDefinition def, Vector2? screenPosOverride = null)
    {
        var tip = AchievementHoverTooltip.Instance;
        if (tip == null) return;

        if (showHoverOnlyWhenLocked && unlocked)
        {
            tip.Hide();
            return;
        }

        string msg = "";
        if (!unlocked && def != null)
            msg = def.lockedHoverHint;

        if (string.IsNullOrWhiteSpace(msg))
        {
            tip.Hide();
            return;
        }

        Vector2 screenPos = screenPosOverride ?? (Vector2)Input.mousePosition;
        tip.Show(msg, screenPos);
    }
}
