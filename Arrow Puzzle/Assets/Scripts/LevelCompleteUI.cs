using TMPro;
using UnityEngine;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI levelText;
    private LevelManager levelManager;

    private void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();

        panel.SetActive(false);

        UpdateLevelText();
    }

    public void Show()
    {
        UpdateLevelText();

        panel.SetActive(true);
    }

    public void NextLevel()
    {
        panel.SetActive(false);
        if(levelManager != null)
        {
            levelManager.LoadNextLevel();

            UpdateLevelText();
        }
    }

    void UpdateLevelText()
    {
        if(levelManager == null)
        {
            return;
        }

        levelText.text = "Level 0 " + levelManager.GetCurrentLevelNumber();
    }

    public void RestartLevel()
    {
        panel.SetActive(false);
        levelManager.RestartLevel();
    }
}