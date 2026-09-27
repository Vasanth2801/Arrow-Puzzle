using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text heartsText;
    [SerializeField] private Text levelText;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button backButton;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Panel Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;

    private void Start()
    {
        if (BoardManager.Instance == null) return;

        BoardManager.Instance.OnHeartsChanged += UpdateHearts;
        BoardManager.Instance.OnLevelChanged += UpdateLevel;
        BoardManager.Instance.OnLevelComplete += ShowWin;
        BoardManager.Instance.OnGameOver += ShowLose;

        if (hintButton != null)
            hintButton.onClick.AddListener(() => BoardManager.Instance.ShowHint());

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(() =>
            {
                HidePanels();
                BoardManager.Instance.NextLevel();
            });

        if (retryButton != null)
            retryButton.onClick.AddListener(() =>
            {
                HidePanels();
                BoardManager.Instance.RestartLevel();
            });

        if (backButton != null)
            backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        HidePanels();
        UpdateHearts(BoardManager.Instance.Hearts);
        UpdateLevel(BoardManager.Instance.CurrentLevel);
    }

    private void UpdateHearts(int hearts)
    {
        if (heartsText != null)
            heartsText.text = "Hearts: " + hearts + "/3";
    }

    private void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = "Level " + level;
    }

    private void ShowWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
    }

    private void ShowLose()
    {
        if (losePanel != null) losePanel.SetActive(true);
    }

    private void HidePanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }
}
