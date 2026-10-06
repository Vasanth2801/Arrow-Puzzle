using UnityEngine;



    public sealed class UIActions : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AdService adService;
        [SerializeField] private string privacyPolicyUrl = "https://example.com/privacy";

        public void NextLevel() => gameManager.StartLevel(gameManager.CurrentLevel + 1);
        public void Replay() => gameManager.RestartLevel();
        public void Home() => gameManager.Home();
        public void Resume() => gameManager.ResumeGame();
        public void Pause() => gameManager.PauseGame();
        public void OpenLevelSelect() => uiManager.ShowLevelSelect();
        public void OpenThemes() => uiManager.ShowThemes();
        public void OpenDailyReward() => uiManager.ShowDailyReward();
        public void OpenMainMenu() => uiManager.ShowMainMenu();
        public void ReviveWithRewardedAd() => adService.ShowRewarded(gameManager.ReviveFromAd);
        public void RestoreProgress() => gameManager.RestoreProgress();
        public void OpenPrivacyPolicy() => Application.OpenURL(privacyPolicyUrl);
    }
