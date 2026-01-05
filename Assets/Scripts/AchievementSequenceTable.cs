using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Achievements/Achievement Sequence Table", fileName = "AchievementSequenceTable")]
public sealed class AchievementSequenceTable : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        [Tooltip("AchievementsManager 的 id，比如 ach.click_star")]
        public string achievementId;

        [Tooltip("一个成就可由多条序列触发，每条序列用空格分隔 token，例如：UP A B LEFT")]
        public List<string> sequences = new List<string>();
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    // key: 规范化后的序列，如 "UP UP A B"
    // val: achievementId
    private Dictionary<string, string> map;

    private void OnEnable() => Build();
    private void OnValidate() => Build();

    private void Build()
    {
        map = new Dictionary<string, string>(StringComparer.Ordinal);

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e == null) continue;
            if (string.IsNullOrWhiteSpace(e.achievementId)) continue;
            if (e.sequences == null) continue;

            for (int k = 0; k < e.sequences.Count; k++)
            {
                var raw = e.sequences[k];
                var key = NormalizeSequence(raw);
                if (string.IsNullOrEmpty(key)) continue;

                if (map.TryGetValue(key, out var existed) && existed != e.achievementId)
                {
                    Debug.LogWarning(
                        $"[AchievementSequenceTable] 序列冲突: \"{key}\" 同时映射到 {existed} 和 {e.achievementId}",
                        this
                    );
                    continue;
                }

                map[key] = e.achievementId;
            }
        }
    }

    public bool TryResolve(IReadOnlyList<string> tokensCanonical, out string achievementId)
    {
        achievementId = null;
        if (tokensCanonical == null || tokensCanonical.Count == 0) return false;

        if (map == null) Build();
        if (map == null) return false;

        var key = string.Join(" ", tokensCanonical);
        return map.TryGetValue(key, out achievementId);
    }

    public static string NormalizeToken(string t)
    {
        if (string.IsNullOrWhiteSpace(t)) return "";
        t = t.Trim().ToUpperInvariant();

        // 允许一些别名
        if (t == "ARROWUP") return "UP";
        if (t == "ARROWDOWN") return "DOWN";
        if (t == "ARROWLEFT") return "LEFT";
        if (t == "ARROWRIGHT") return "RIGHT";

        if (t == "ENTER") return "ENTER";
        if (t == "RETURN") return "ENTER";
        if (t == "BACKSPACE") return "BACKSPACE";
        if (t == "BKSP") return "BACKSPACE";
        if (t == "SPACE") return "SPACE";

        return t;
    }

    public static string NormalizeSequence(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";

        // 支持逗号/空格混写
        raw = raw.Replace(",", " ");
        var parts = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var list = new List<string>(parts.Length);
        for (int i = 0; i < parts.Length; i++)
        {
            var tok = NormalizeToken(parts[i]);
            if (!string.IsNullOrEmpty(tok))
                list.Add(tok);
        }

        return list.Count == 0 ? "" : string.Join(" ", list);
    }
}
