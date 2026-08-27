using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Level Settings")]
    [SerializeField] private string levelFolder = "Levels/Generated";
    [SerializeField] private int currentLevel = 1;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    private Leveldata currentLevelData;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        currentLevel = 1;
        LoadLevel(1);
    }

    public void LoadLevel(int levelNumber)
    {
        if(levelNumber < 1)
        {
            levelNumber = 1;
        }

        string levelName = "level_" + levelNumber.ToString("000");
        string resourcePath = levelFolder + "/ " + levelName;

        Debug.Log("Trying to load: Resources/ " + resourcePath);

        Leveldata[] allLevels = Resources.LoadAll<Leveldata>("Levels/Generated");

        Debug.Log("Level Data assets found by Resources: " + allLevels.Length);

        foreach(Leveldata level in allLevels)
        {
            Debug.Log("Found Level: " + level.name);
        }

        Leveldata levelData = Resources.Load<Leveldata>(resourcePath);
        
        if(levelData == null)
        {
            Debug.LogError("Could not load: " + resourcePath);
            return;
        }

        currentLevel = levelNumber;
        currentLevelData = levelData;

        Debug.Log("Success: Loaded " + levelData.name);


        if(gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        if(gridManager == null)
        {
            Debug.LogError("GridManager not found");
            return;
        }

        gridManager.LoadLevel(currentLevelData);
    }

    public void LoadNextLevel()
    {
        int nextLevel = currentLevel + 1;
    }

    public void LoadPreviousLevel()
    {
        if(currentLevel > 1)
        {
            LoadLevel(currentLevel - 1);
        }
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public Leveldata GetCurrentLeveldata()
    {
        return currentLevelData;
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevel);
    }
}