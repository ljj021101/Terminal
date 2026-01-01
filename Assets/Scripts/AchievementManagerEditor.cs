using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AchievementsManager))]
public sealed class AchievementsManagerEditor : Editor
{
    private Vector2 scroll;
    private readonly List<AchievementCard> cards = new List<AchievementCard>();

    public override void OnInspectorGUI()
    {
        var mgr = (AchievementsManager)target;

        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rescan", GUILayout.Width(90)))
                    RescanCards();

                if (GUILayout.Button("Clear All", GUILayout.Width(90)))
                {
                    // No dialog, as requested
                    if (Application.isPlaying)
                    {
                        mgr.ResetAll();
                        RefreshAllPanels();
                    }
                    else
                    {
                        PlayerPrefs.DeleteKey("ACH_UNLOCKED_V1");
                        PlayerPrefs.Save();
                    }
                }
            }

            if (cards.Count == 0)
                RescanCards();

            // Snapshot unlocked ids for display and toggle behavior
            var unlockedSet = new HashSet<string>();
            if (Application.isPlaying && AchievementsManager.Instance != null)
            {
                var snap = AchievementsManager.Instance.GetUnlockedSnapshot();
                for (int i = 0; i < snap.Length; i++)
                    unlockedSet.Add(snap[i]);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(180));

            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i];
                if (c == null) continue;

                var id = c.Id ?? "";
                if (string.IsNullOrWhiteSpace(id)) continue;

                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    bool isUnlocked = Application.isPlaying && unlockedSet.Contains(id);
                    string label = isUnlocked ? $"✅ {id}" : $"⬜ {id}";
                    EditorGUILayout.LabelField(label, GUILayout.MinWidth(150));

                    GUI.enabled = Application.isPlaying;

                    if (GUILayout.Button("Toggle", GUILayout.Width(70)))
                    {
                        mgr.Toggle(id);

                        // keep UI accurate immediately
                        unlockedSet.Clear();
                        var snap = AchievementsManager.Instance.GetUnlockedSnapshot();
                        for (int k = 0; k < snap.Length; k++)
                            unlockedSet.Add(snap[k]);

                        RefreshAllPanels();
                    }

                    GUI.enabled = true;

                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                    {
                        EditorGUIUtility.PingObject(c.gameObject);
                        Selection.activeObject = c.gameObject;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void RescanCards()
    {
        cards.Clear();

        var found = Object.FindObjectsByType<AchievementCard>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (found == null) return;

        // De-dup by id, then sort
        var unique = new Dictionary<string, AchievementCard>();
        for (int i = 0; i < found.Length; i++)
        {
            var c = found[i];
            if (c == null) continue;

            var id = c.Id ?? "";
            if (string.IsNullOrWhiteSpace(id)) continue;

            if (!unique.ContainsKey(id))
                unique.Add(id, c);
        }

        cards.AddRange(unique.OrderBy(kv => kv.Key).Select(kv => kv.Value));
    }

    private static void RefreshAllPanels()
    {
        var panels = Object.FindObjectsByType<AchievementPanel>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < panels.Length; i++)
        {
            var p = panels[i];
            if (p == null) continue;
            p.RefreshAll();
        }
    }
}
