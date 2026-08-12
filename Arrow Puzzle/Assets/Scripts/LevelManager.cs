using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Manual Level")]
    [SerializeField] private Leveldata[] levels;

    [Header("Generator")]
    [SerializeField] private LevelGenerator levelGenerator;

    private List<Leveldata> allLevels = new List<Leveldata>();

    private int currentLevelIndex = 0;

    private void Start()
    {
        GenerateLevels();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    void GenerateLevels()
    {
        allLevels.Clear();

        if(levels != null)
        {
            foreach(Leveldata level in levels)
            {
                allLevels.Add(level);
            }
        }

        if(levelGenerator != null)
        {
            List<Leveldata> generatedLevels = levelGenerator.GenerateLevels();

            allLevels.AddRange(generatedLevels);
        }

        Debug.Log("Total Levels: " + allLevels.Count);
    }

    public Leveldata GetCurrentLevel()
    {
        return levels[currentLevelIndex];
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;

        if(currentLevelIndex >= levels.Length)
        {
            Debug.Log("All levels completed!");
            currentLevelIndex = levels.Length - 1;
            return;
        }

        Debug.Log($"Loading Level {currentLevelIndex + 1}");

        GridManager gridManager = FindAnyObjectByType<GridManager>();

        gridManager.LoadNextLevel(GetCurrentLevel());
    }

    public void RestartLevel()
    {
        Debug.Log($"Resetting Level {currentLevelIndex + 1}");

        GridManager gridManager = FindAnyObjectByType<GridManager>();

        gridManager.LoadNextLevel(GetCurrentLevel());
    }

    public int GetCurrentLevelNumber()
    {
        return currentLevelIndex + 1;
    }

    public int GetTotallevelCount()
    {
        return allLevels.Count;
    }
}