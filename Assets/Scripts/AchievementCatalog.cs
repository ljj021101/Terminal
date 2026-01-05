using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Achievements/Achievements Catalog", fileName = "AchievementsCatalog")]
public sealed class AchievementsCatalog : ScriptableObject
{
    [SerializeField] private List<AchievementDefinition> entries = new List<AchievementDefinition>();

    private Dictionary<string, AchievementDefinition> dict;

    private void OnEnable()
    {
        Build();
    }

    private void OnValidate()
    {
        Build();
    }

    private void Build()
    {
        dict = new Dictionary<string, AchievementDefinition>(StringComparer.Ordinal);

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e == null) continue;

            var id = e.id;
            if (string.IsNullOrWhiteSpace(id)) continue;

            if (!dict.ContainsKey(id))
                dict.Add(id, e);
        }
    }

    public bool TryGet(string id, out AchievementDefinition entry)
    {
        entry = null;

        if (string.IsNullOrWhiteSpace(id)) return false;

        if (dict == null) Build();
        if (dict == null) return false;

        return dict.TryGetValue(id, out entry);
    }

    public List<string> GetAllIdsSorted()
    {
        var ids = new List<string>(entries.Count);

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e == null) continue;
            if (string.IsNullOrWhiteSpace(e.id)) continue;
            ids.Add(e.id);
        }

        ids.Sort(StringComparer.Ordinal);
        return ids;
    }
}
