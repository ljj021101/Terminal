using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WindowManager))]
public sealed class WindowManagerEditor : Editor
{
    private string filter = "";
    private Vector2 scroll;

    public override void OnInspectorGUI()
    {
        var mgr = (WindowManager)target;

        DrawDefaultInspector();
        EditorGUILayout.Space(8);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rescan"))
                {
                    if (Application.isPlaying)
                    {
                        mgr.Rescan();
                    }
                    else
                    {
                        Undo.RecordObject(mgr, "Rescan Windows");
                        mgr.Rescan();
                        EditorUtility.SetDirty(mgr);
                    }
                }

                if (GUILayout.Button("Close All") && Application.isPlaying)
                {
                    var list = mgr.AllWindows;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var w = list[i];
                        if (w == null) continue;
                        if (w.IsOpen) w.Close();
                    }
                }
            }

            EditorGUILayout.Space(6);
            filter = EditorGUILayout.TextField("Filter", filter);

            var windows = mgr.AllWindows;
            EditorGUILayout.LabelField($"Windows: {windows.Count}");

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(260));

            for (int i = 0; i < windows.Count; i++)
            {
                var w = windows[i];
                if (w == null) continue;

                var id = w.WindowId;
                if (!string.IsNullOrEmpty(filter))
                {
                    var f = filter.ToLower();
                    var s = (id ?? "").ToLower();
                    if (!s.Contains(f)) continue;
                }

                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    EditorGUILayout.LabelField(id, GUILayout.Width(180));
                    EditorGUILayout.LabelField(w.IsOpen ? "Open" : "Closed", GUILayout.Width(60));

                    GUI.enabled = Application.isPlaying;

                    if (GUILayout.Button("Open", GUILayout.Width(60)))
                        mgr.Open(id);

                    if (GUILayout.Button("Toggle", GUILayout.Width(60)))
                        mgr.Toggle(id);

                    if (GUILayout.Button("Close", GUILayout.Width(60)))
                        mgr.Close(id);

                    if (GUILayout.Button("Focus", GUILayout.Width(60)))
                        mgr.BringToFront(w);

                    GUI.enabled = true;

                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                    {
                        EditorGUIUtility.PingObject(w.gameObject);
                        Selection.activeObject = w.gameObject;
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Open/Close/Toggle/Focus 需要在 Play Mode 下执行。编辑模式下可以 Rescan 并 Ping 定位对象。",
                    MessageType.Info
                );
            }
        }
    }
}
