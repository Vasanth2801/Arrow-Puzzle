using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public Text heartsText;
    public Text levelText;
    public Button hintButton;
    public Button resetProgress;

    [Header("Win Panel")]
    public GameObject winPanel;
    public Button nextLevelButton;

    [Header("Lose Panel")]
    public GameObject losePanel;
    public Button retryButton;

    private void Start()
    {
        BoardManager.Instance.OnHeartsChanged += UpdateHearts;
        BoardManager.Instance.OnLevelChanged += UpdateLevel;
        BoardManager.Instance.OnLevelComplete += ShowWinPanel;
        BoardManager.Instance.OnGameOver += ShowLosePanel;

        hintButton.onClick.AddListener(() => BoardManager.Instance.ShowHint());
        nextLevelButton.onClick.AddListener(() =>
        {
            HideAllPanels();

            int nextLevel = BoardManager.Instance.CurrentLevel + 1;

            GameProgress.Instance.SaveLevel(nextLevel);

            BoardManager.Instance.NextLevel();
        });
        retryButton.onClick.AddListener(() =>
        {
            HideAllPanels();
            BoardManager.Instance.RestartLevel();
        });

       

        HideAllPanels();
        UpdateHearts(BoardManager.Instance.MaxHearts);
        UpdateLevel(BoardManager.Instance.CurrentLevel);
    }

    private void UpdateHearts(int hearts)
    {
        heartsText.text = $"Hearts: {Mathf.Max(hearts, 0)}/{BoardManager.Instance.MaxHearts}";
    }

    private void UpdateLevel(int level)
    {
        levelText.text = $"Level {level}";
    }

    private void ShowWinPanel() => winPanel.SetActive(true);

    private void ShowLosePanel() => losePanel.SetActive(true);

    private void HideAllPanels()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }
}
