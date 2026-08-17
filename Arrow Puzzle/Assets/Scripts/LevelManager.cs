using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Manual Level")]
    [SerializeField] private Leveldata[] levels;
    private int currentLevelIndex = 0;

    private void Awake()
    {
        levels = Resources.LoadAll<Leveldata>("Levels/Generated");

        if(levels == null || levels.Length == 0)
        {
            Debug.LogError("No generated Levels Found");
        }

        SortLevel();

        Debug.Log("Loaded " + levels.Length + " levels");
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
            Debug.Log("All Levels Completed");

            currentLevelIndex = levels.Length - 1;

            return;
        }

        Debug.Log("Loading Level " + currentLevelIndex + 1);

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
        return levels.Length;
    }

    private void SortLevel()
    {
        System.Array.Sort(levels, (a, b) => string.Compare(a.name, b.name));
    }
}