using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public sealed class DailyRewardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text streakText;
        [SerializeField] private TMP_Text[] dayLabels;
        [SerializeField] private Button claimButton;

        private GameManager manager;

        public void Refresh(GameManager gameManager)
        {
            manager = gameManager;
            var save = manager.Save.Load();
            int streak = save.dailyStreak;
            if (streakText != null) streakText.text = $"7-DAY STREAK  •  {streak}/7";
            for (int i = 0; i < dayLabels.Length; i++)
                dayLabels[i].text = $"DAY {i + 1}\n{(i < streak ? "✓" : "•")}";
            if (claimButton != null) claimButton.interactable = !ClaimedToday(save.lastDailyClaimUtc);
        }

        public void Claim()
        {
            var save = manager.Save.Load();
            if (ClaimedToday(save.lastDailyClaimUtc)) return;
            int nextStreak = save.dailyStreak >= 7 ? 1 : save.dailyStreak + 1;
            int reward = nextStreak >= 7 ? 5 : 1 + nextStreak / 2;
            manager.CompleteDailyReward(reward);
            Refresh(manager);
        }

        private static bool ClaimedToday(string date)
            => date == DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
    }