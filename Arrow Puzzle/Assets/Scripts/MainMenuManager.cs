using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject resetConfirmPanel;
    public Button resetProgress;

    public void Play()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneManager.LoadScene("Game");
    }

    public void OpenResetConfirm()
    {
        AudioManager.Instance?.PlayButtonClick();
        resetConfirmPanel.SetActive(true);
    }

    public void CancelResetConfirm()
    {
        AudioManager.Instance?.PlayButtonClick();
        resetConfirmPanel.SetActive(false);
    }

    // FIX: this used to call resetProgress.onClick.AddListener(...) every
    // time ConfirmReset() ran, which stacks a new listener on the button
    // each time instead of resetting progress once. It now resets progress
    // directly, the moment the user confirms.
    public void ConfirmReset()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameProgress.Instance?.ResetAllProgress();
        resetConfirmPanel.SetActive(false);
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        Application.Quit();
    }
}
