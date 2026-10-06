using System;
using UnityEngine;


    [Serializable]
    public sealed class SaveData
    {
        public int currentLevel = 1;
        public int unlockedLevel = 1;
        public int[] stars = new int[1001];
        public int lives = 3;
        public string selectedThemeId = "midnight";
        public int hintsRemainingToday = 3;
        public string hintResetDateUtc = "";
        public int dailyStreak = 0;
        public string lastDailyClaimUtc = "";
        public bool soundOn = true;
        public bool musicOn = true;
        public bool vibrationOn = true;
        public string language = "en";

        public int GetStars(int level)
        {
            if (level < 1) return 0;
            if (level >= stars.Length) Array.Resize(ref stars, level + 1);
            return stars[level];
        }
    }
