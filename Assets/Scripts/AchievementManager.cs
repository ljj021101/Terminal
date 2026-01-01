using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class AchievementsManager : MonoBehaviour
{
    public static AchievementsManager Instance { get; private set; }

    public event Action<string> OnUnlocked;
    public event Action<string> OnLocked;

    [SerializeField] private bool dontDestroyOnLoad = true;

    private readonly HashSet<string> unlocked = new HashSet<string>();

    private const string SaveKey = "ACH_UNLOCKED_V1";
    private const char Sep = '|';

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        Load();
    }

    public bool IsUnlocked(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        return unlocked.Contains(id);
    }

    public bool Unlock(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        if (!unlocked.Add(id)) return false;

        Save();
        OnUnlocked?.Invoke(id);
        return true;
    }

    public bool Lock(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        if (!unlocked.Remove(id)) return false;

        Save();
        OnLocked?.Invoke(id);
        return true;
    }

    public bool Toggle(string id)
    {
        if (IsUnlocked(id)) return Lock(id);
        return Unlock(id);
    }

    public void ResetAll()
    {
        unlocked.Clear();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    public string[] GetUnlockedSnapshot()
    {
        return unlocked.OrderBy(x => x).ToArray();
    }

    public void DebugReloadFromDisk()
    {
        Load();
    }

    private void Save()
    {
        var s = string.Join(Sep.ToString(), unlocked);
        PlayerPrefs.SetString(SaveKey, s);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        unlocked.Clear();

        var s = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(s)) return;

        var parts = s.Split(Sep);
        for (int i = 0; i < parts.Length; i++)
        {
            var id = parts[i];
            if (!string.IsNullOrWhiteSpace(id))
                unlocked.Add(id);
        }
    }
}
