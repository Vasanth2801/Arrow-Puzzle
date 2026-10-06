using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public sealed class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject themesPanel;
        [SerializeField] private GameObject dailyPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("Gameplay")]
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private GameplayUI gameplayUI;
        [SerializeField] private LevelSelectUI levelSelectUI;
        [SerializeField] private ThemeUI themeUI;
        [SerializeField] private DailyRewardUI dailyRewardUI;
        [SerializeField] private TMP_Text winLevelText;
        [SerializeField] private TMP_Text winStarsText;
        [SerializeField] private TMP_Text loseLevelText;

        [Header("Services")]
        [SerializeField] private AdService adService;

        private GameManager gameManager;

        public void Initialize(GameManager manager)
        {
            gameManager = manager;
            HideAll();
        }

        public void ShowMainMenu()
        {
            HideAll();
            mainMenuPanel.SetActive(true);
            if (mainMenuUI != null) mainMenuUI.Bind(gameManager);
        }

        public void ShowGameplay(LevelData level)
        {
            HideAll();
            gameplayPanel.SetActive(true);
            gameplayUI.Bind(gameManager, level);
        }

        public void ShowLevelSelect()
        {
            HideAll();
            levelSelectPanel.SetActive(true);
            levelSelectUI.Build(gameManager);
        }

        public void ShowThemes()
        {
            HideAll();
            themesPanel.SetActive(true);
            themeUI.Build(gameManager);
        }

        public void ShowDailyReward()
        {
            HideAll();
            dailyPanel.SetActive(true);
            dailyRewardUI.Refresh(gameManager);
        }

        public void ShowPause() => pausePanel.SetActive(true);
        public void ShowWin(int level, int stars)
        {
            winLevelText.text = $"LEVEL {level}";
            winStarsText.text = new string('★', stars) + new string('☆', 3 - stars);
            winPanel.SetActive(true);
        }
        public void ShowLose(int level)
        {
            loseLevelText.text = $"LEVEL {level}";
            losePanel.SetActive(true);
        }
        public void HidePopup()
        {
            pausePanel.SetActive(false);
            winPanel.SetActive(false);
            losePanel.SetActive(false);
        }

        public void OnHint()
        {
            if (!gameManager.GetComponent<HintSystem>().RequestHint())
            {
                adService.ShowRewarded(() => gameManager.GetComponent<HintSystem>().RewardedHintsGranted());
            }
        }

        public void OnWrongTapFeedback()
        {
            gameManager.GetComponent<AudioManager>().PlayWrong();
            Haptics.Wrong();
        }

        private void HideAll()
        {
            mainMenuPanel.SetActive(false);
            gameplayPanel.SetActive(false);
            levelSelectPanel.SetActive(false);
            themesPanel.SetActive(false);
            dailyPanel.SetActive(false);
            pausePanel.SetActive(false);
            winPanel.SetActive(false);
            losePanel.SetActive(false);
        }
    }