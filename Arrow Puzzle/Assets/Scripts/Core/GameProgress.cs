using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    private const string LEVEL_KEY = "ArrowPuzzle_CurrentLevel";

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

    public int GetLevel()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt(LEVEL_KEY, 1));
    }

    public void SaveLevel(int level)
    {
        PlayerPrefs.SetInt(LEVEL_KEY, Mathf.Max(1, level));
        PlayerPrefs.Save();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
