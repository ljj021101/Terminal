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

    [TextArea]
    public string description;

    [TextArea]
    public string lockedHoverHint;
}
