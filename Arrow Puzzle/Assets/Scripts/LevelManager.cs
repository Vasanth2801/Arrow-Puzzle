using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Level Settings")]
    [SerializeField] private string levelFolder = "Levels/Generatoed";
    [SerializeField] private int currentLevel = 0;

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
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int levelNumber)
    {
        string levelName = "Level " + levelNumber.ToString("000");

        Leveldata level = Resources.Load<Leveldata>(levelFolder + "/ " + levelName);

        if(level == null)
        {
            Debug.LogError("Could not load level " + levelName);

            return;
        }

        currentLevel = levelNumber;

        currentLevelData = level;

        Debug.Log("Loading Level " + currentLevel);

        if(gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        if(gridManager == null)
        {
            Debug.LogError("GridManager not found ");

            return;
        }

        gridManager.LoadLevel(currentLevelData);
    }

    public void LoadNextLevel()
    {
        int nextLevel = currentLevel + 1;

        Leveldata next = Resources.Load<Leveldata>(levelFolder + "/Level_ " + nextLevel.ToString("000"));

        if(next == null)
        {
            Debug.Log("No more Levels Avaialble");

            return;
        }

        LoadLevel(nextLevel);
    }

    public void LoadPreviousLevel()
    {
        if(currentLevel <= 1)
        {
            return;
        }

        LoadLevel(currentLevel - 1);
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public Leveldata GetCurrentLeveldata()
    {
        return currentLevelData;
    }
}