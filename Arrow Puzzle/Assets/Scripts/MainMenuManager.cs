using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject resetConfirmPanel;
    public Button resetProgress;

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenResetConfirm()
    {
        resetConfirmPanel.SetActive(true);
    }

    public void CancelResetConfirm()
    {
        resetConfirmPanel.SetActive(false);
    }

    public void ConfirmReset()
    {
        resetProgress.onClick.AddListener(() =>
        {
            GameProgress.Instance.ResetAllProgress();
        });

        resetConfirmPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}