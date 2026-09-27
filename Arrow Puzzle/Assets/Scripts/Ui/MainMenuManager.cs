using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject resetConfirmPanel;
    [SerializeField] private Button confirmResetButton;
    [SerializeField] private Button cancelResetButton;

    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(() => SceneManager.LoadScene("Game"));

        if (resetButton != null)
            resetButton.onClick.AddListener(ShowResetConfirm);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (confirmResetButton != null)
            confirmResetButton.onClick.AddListener(ResetProgress);

        if (cancelResetButton != null)
            cancelResetButton.onClick.AddListener(HideResetConfirm);

        HideResetConfirm();
    }

    private void ShowResetConfirm()
    {
        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(true);
    }

    private void HideResetConfirm()
    {
        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);
    }

    private void ResetProgress()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.ResetProgress();

        HideResetConfirm();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
