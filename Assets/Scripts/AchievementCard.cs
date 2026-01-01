using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AchievementCard : MonoBehaviour
{
    [Header("Achievement Data")]
    [SerializeField] private string achievementId;
    [SerializeField] private Sprite icon;
    [SerializeField] private string unlockedName;
    [TextArea] [SerializeField] private string unlockedDescription;

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;

    [Header("Locked Style")]
    [SerializeField] private string lockedName = "???";
    [SerializeField] private Color lockedIconTint = Color.gray;

    public Sprite PopupIcon => icon;
    public string PopupTitle => unlockedName;
    public string PopupDesc => unlockedDescription;

    public string Id => achievementId;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var mgr = AchievementsManager.Instance;
        bool unlocked = mgr != null && mgr.IsUnlocked(achievementId);
        Apply(unlocked);
    }

    public void Apply(bool unlocked)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = unlocked ? Color.white : lockedIconTint;
        }

        if (nameText != null)
            nameText.text = unlocked ? unlockedName : lockedName;

        if (descText != null)
        {
            descText.text = unlocked ? unlockedDescription : "";
            descText.gameObject.SetActive(unlocked);
        }
    }
}
