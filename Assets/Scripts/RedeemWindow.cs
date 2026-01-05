using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AchievementSequenceRedeemWindow : MonoBehaviour
{
    [Header("Focus")]
    [SerializeField] private UIWindow ownerWindow;

    [Header("Mapping")]
    [SerializeField] private AchievementSequenceTable sequenceTable;

    [Header("Cells")]
    [SerializeField] private Transform cellsRoot;           // 放 10 个格子的父物体
    [SerializeField] private List<TMP_Text> cellTexts = new List<TMP_Text>();
    [SerializeField] private int maxTokens = 10;

    [Header("Buttons")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backspaceButton;
    [SerializeField] private Button submitButton;

    private readonly List<string> tokensCanonical = new List<string>(10);
    private readonly List<string> tokensDisplay = new List<string>(10);

    private void Awake()
    {
        if (ownerWindow == null)
            ownerWindow = GetComponentInParent<UIWindow>(true);

        AutoBindCellsIfNeeded();

        if (resetButton != null) resetButton.onClick.AddListener(ResetInput);
        if (backspaceButton != null) backspaceButton.onClick.AddListener(Backspace);
        if (submitButton != null) submitButton.onClick.AddListener(Submit);

        RefreshCells();
    }

    private void AutoBindCellsIfNeeded()
    {
        if (cellTexts.Count > 0) return;
        if (cellsRoot == null) return;

        cellTexts.Clear();

        // 按 children 顺序取每格里的 TMP_Text
        for (int i = 0; i < cellsRoot.childCount; i++)
        {
            var t = cellsRoot.GetChild(i).GetComponentInChildren<TMP_Text>(true);
            if (t != null) cellTexts.Add(t);
        }
    }

    private void Update()
    {
        if (!IsActiveForKeyboard()) return;

        // 退格 / 提交
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Backspace();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Submit();
            return;
        }

        // 方向键 token
        if (Input.GetKeyDown(KeyCode.UpArrow))    { PushToken("UP", "↑"); return; }
        if (Input.GetKeyDown(KeyCode.DownArrow))  { PushToken("DOWN", "↓"); return; }
        if (Input.GetKeyDown(KeyCode.LeftArrow))  { PushToken("LEFT", "←"); return; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { PushToken("RIGHT", "→"); return; }

        // 普通字符输入
        var s = Input.inputString;
        if (string.IsNullOrEmpty(s)) return;

        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];

            if (ch == '\b')
            {
                Backspace();
                continue;
            }

            if (ch == '\n' || ch == '\r')
            {
                Submit();
                continue;
            }

            if (tokensCanonical.Count >= maxTokens) continue;

            // 统一转大写更像“密码”
            string disp = char.ToUpperInvariant(ch).ToString();

            // 空格也给个可见符号
            if (ch == ' ') { PushToken("SPACE", "␣"); continue; }

            PushToken(disp, disp);
        }
    }

    private bool IsActiveForKeyboard()
    {
        if (!isActiveAndEnabled) return false;
        if (ownerWindow == null) return true;

        // 必须窗口打开且聚焦
        return ownerWindow.IsOpen && ownerWindow.IsFocused;
    }

    private void PushToken(string canonical, string display)
    {
        if (tokensCanonical.Count >= maxTokens) return;

        canonical = AchievementSequenceTable.NormalizeToken(canonical);

        tokensCanonical.Add(canonical);
        tokensDisplay.Add(display);

        RefreshCells();
    }

    public void ResetInput()
    {
        tokensCanonical.Clear();
        tokensDisplay.Clear();
        RefreshCells();
    }

    public void Backspace()
    {
        if (tokensCanonical.Count == 0) return;

        int last = tokensCanonical.Count - 1;
        tokensCanonical.RemoveAt(last);
        tokensDisplay.RemoveAt(last);

        RefreshCells();
    }

    public void Submit()
    {
        if (tokensCanonical.Count == 0)
        {
            var mgr0 = AchievementsManager.Instance;
            if (mgr0 != null)
                mgr0.Unlock("ach.kongxulie");

            return;
        }

        string key = string.Join(" ", tokensCanonical);

        if (sequenceTable == null)
        {
            Debug.LogWarning($"[Redeem] sequenceTable is NULL, input = {key}");
            ResetInput();
            return;
        }

        bool ok = sequenceTable.TryResolve(tokensCanonical, out string achievementId);
        Debug.Log($"[Redeem] input = {key}, ok = {ok}, id = {achievementId}");

        if (!ok)
        {
            FindObjectOfType<DesktopDiagnosticNote>(true)?.EnsureOnDesktop();
            ResetInput();
            return;
        }

        if (ok && !string.IsNullOrEmpty(achievementId))
        {
            var mgr = AchievementsManager.Instance;
            if (mgr == null)
            {
                Debug.LogWarning("[Redeem] AchievementsManager.Instance is NULL");
            }
            else
            {
                bool unlockedNow = mgr.Unlock(achievementId);
                Debug.Log($"[Redeem] Unlock() returned {unlockedNow}, IsUnlocked = {mgr.IsUnlocked(achievementId)}");
            }
        }

        ResetInput();
    }

    private void RefreshCells()
    {
        if (cellTexts == null || cellTexts.Count == 0) return;

        int n = Mathf.Min(cellTexts.Count, maxTokens);

        for (int i = 0; i < n; i++)
        {
            var t = cellTexts[i];
            if (t == null) continue;

            if (i < tokensDisplay.Count)
            {
                t.text = tokensDisplay[i];
                t.gameObject.SetActive(true);
            }
            else
            {
                t.text = "";
                t.gameObject.SetActive(true);
            }
        }

        // 如果你场景里格子数量 > maxTokens，多余的隐藏
        for (int i = n; i < cellTexts.Count; i++)
        {
            var t = cellTexts[i];
            if (t != null) t.gameObject.SetActive(false);
        }
    }
}
