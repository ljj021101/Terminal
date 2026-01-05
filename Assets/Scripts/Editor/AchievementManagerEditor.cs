using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AchievementsManager))]
public sealed class AchievementsManagerEditor : Editor
{
    private Vector2 scroll;
    private readonly List<string> ids = new List<string>();

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
                    RescanFromCatalog(mgr);

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

            if (ids.Count == 0)
                RescanFromCatalog(mgr);

            // Snapshot unlocked ids for display and toggle behavior
            var unlockedSet = new HashSet<string>();
            if (Application.isPlaying && AchievementsManager.Instance != null)
            {
                var snap = AchievementsManager.Instance.GetUnlockedSnapshot();
                for (int i = 0; i < snap.Length; i++)
                    unlockedSet.Add(snap[i]);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(180));

            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
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

                    if (GUILayout.Button("Ping Catalog", GUILayout.Width(90)))
                    {
                        var cat = mgr.Catalog;
                        if (cat != null)
                        {
                            EditorGUIUtility.PingObject(cat);
                            Selection.activeObject = cat;
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void RescanFromCatalog(AchievementsManager mgr)
    {
        ids.Clear();

        var catalog = mgr != null ? mgr.Catalog : null;
        if (catalog == null)
        {
            EditorGUILayout.HelpBox("Catalog is null, cannot list achievements from ScriptableObject", MessageType.Warning);
            return;
        }

        ids.AddRange(catalog.GetAllIdsSorted());
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
