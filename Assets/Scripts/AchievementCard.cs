using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AchievementCard : MonoBehaviour
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

    public string Id => achievementId;

    public void SetId(string id)
    {
        achievementId = id;
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var mgr = AchievementsManager.Instance;
        bool unlocked = mgr != null && mgr.IsUnlocked(achievementId);

        AchievementDefinition def = null;
        if (mgr != null)
            mgr.TryGetDefinition(achievementId, out def);

        Apply(unlocked, def);
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
            nameText.text = unlocked ? title : lockedName;

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
}
