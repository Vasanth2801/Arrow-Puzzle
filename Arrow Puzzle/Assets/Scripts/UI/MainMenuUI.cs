using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentLevelText;
        [SerializeField] private Button continueButton;
        [SerializeField] private UIManager uiManager;

        private GameManager manager;

        public void Bind(GameManager gameManager)
        {
            manager = gameManager;
            int level = manager.Save.Load().currentLevel;
            currentLevelText.text = $"LEVEL {level}";
            continueButton.interactable = level >= 1;
        }

        public void Play() => manager.ContinueGame();
        public void LevelSelect() => uiManager.ShowLevelSelect();
        public void Themes() => uiManager.ShowThemes();
        public void DailyReward() => uiManager.ShowDailyReward();
    }