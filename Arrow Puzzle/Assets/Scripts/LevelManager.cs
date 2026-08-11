using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private Leveldata[] levels;

    private int currentLevelIndex = 0;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
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
}