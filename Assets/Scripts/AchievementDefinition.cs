using System;
using UnityEngine;

[Serializable]
public sealed class AchievementDefinition
{
    [Header("Core")]
    public string id;

    [Header("Presentation")]
    public Sprite icon;
    public string title;

    [Header("Locked Presentation")]
    public string lockedTitleOverride;

    [Header("Special Behavior")]
    public bool clickToUnlockWhenLocked;

    [TextArea]
    public string description;

    [TextArea]
    public string lockedHoverHint;
}
