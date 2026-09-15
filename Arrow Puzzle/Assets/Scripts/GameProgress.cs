using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class GameProgress : MonoBehaviour
{
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private const string CLEARED_PREFIX = "Level_";
    private const string CLEARED_SUFFIX = "_Cleared";

    public static GameProgress Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SaveLevel(int levelNumber)
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, levelNumber);
        PlayerPrefs.Save();

        Debug.Log($"[SAVE] Level Saved: {levelNumber}");
    }

    public int GetSavedLevel()
    {
        int savedLevel = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 1);

        Debug.Log($"[Load] Saved Level: {savedLevel}");

        return savedLevel;
    }

    public bool HasSavedProgress()
    {
        return PlayerPrefs.HasKey(CURRENT_LEVEL_KEY);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CURRENT_LEVEL_KEY);

        PlayerPrefs.Save();

        Debug.Log("[Save] Progress Reset");
    }

    private string GetClearedKey(int levelNumber)
    {
        return $"Level_{levelNumber}_Cleared";
    }

    public void SaveClearedPath(int levelNumber, int pathId)
    {
        string current = PlayerPrefs.GetString(GetClearedKey(levelNumber), "");

        if (string.IsNullOrEmpty(current))
        {
            current = pathId.ToString();
        }
        else
        {
            string[] parts = current.Split(',');
            bool exists = false;

            foreach (string part in parts)
            {
                if (int.TryParse(part, out int id) && id == pathId)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                current += "," + pathId;
            }
        }

        PlayerPrefs.SetString(GetClearedKey(levelNumber), current);

        Debug.Log($"[Save] Level {levelNumber} cleared path: {pathId}");
    }

    public string GetClearedPaths(int levelNumber)
    {
        return PlayerPrefs.GetString(GetClearedKey(levelNumber));
    }

    public void ClearClearedPaths(int levelNumber)
    {
        PlayerPrefs.DeleteKey(GetClearedKey(levelNumber));

        PlayerPrefs.Save();
    }    
}