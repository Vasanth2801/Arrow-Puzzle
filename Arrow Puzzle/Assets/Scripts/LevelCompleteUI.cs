using System;
using UnityEngine;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    private LevelManager levelManager;

    private void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();

        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void NextLevel()
    {
        panel.SetActive(false);
        levelManager.LoadNextLevel();
    }

    public void RestartLevel()
    {
        panel.SetActive(false);
        levelManager.RestartLevel();
    }
}