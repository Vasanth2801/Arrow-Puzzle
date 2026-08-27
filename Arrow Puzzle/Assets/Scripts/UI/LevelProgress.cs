using UnityEngine;

public class LevelProgress : MonoBehaviour
{
    public static LevelProgress Instance;
    [SerializeField] private int totalLevels = 100;

    private const string HIGHESTLEVELKEY = "HighestUnlockedLevel";

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public int GetHighestUnlockedLevel()
    {
        return PlayerPrefs.GetInt(HIGHESTLEVELKEY, 1);
    }

    public void UnlockLevel(int levelNumber)
    {
        if(levelNumber < 1)
        {
            return;
        }

        if(levelNumber > totalLevels)
        {
            return;
        }

        int highestUnlocked = GetHighestUnlockedLevel();

        if(levelNumber > highestUnlocked)
        {
            PlayerPrefs.SetInt(HIGHESTLEVELKEY, levelNumber);

            PlayerPrefs.Save();

            Debug.Log("UnLocked Level " + levelNumber);
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= GetHighestUnlockedLevel();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(HIGHESTLEVELKEY);

        PlayerPrefs.Save();

        Debug.Log("Level Progress reset");
    }
}
