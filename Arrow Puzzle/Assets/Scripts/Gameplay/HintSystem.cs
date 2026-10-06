using System;
using System.Collections;
using UnityEngine;

    public sealed class HintSystem : MonoBehaviour
    {
        private GameManager gameManager;
        private BoardView board;
        private LevelSolver solver;
        private GameConfigSO config;
        private SaveSystem saveSystem;
        private SaveData save;
        private int activeHintId = -1;

        public int RemainingFreeHints => save != null ? save.hintsRemainingToday : 0;
        public event Action<int> HintsChanged;

        public void Initialize(GameManager manager, BoardView boardView, LevelSolver levelSolver, GameConfigSO gameConfig,
            SaveSystem saves, SaveData loaded)
        {
            gameManager = manager;
            board = boardView;
            solver = levelSolver;
            config = gameConfig;
            saveSystem = saves;
            save = loaded;
            RefreshDailyHints();
        }

        public void NotifyLevelStarted(int level) { activeHintId = -1; }

        public void ReloadFromSave(SaveData loaded)
        {
            save = loaded;
            RefreshDailyHints();
        }

        public bool RequestHint()
        {
            RefreshDailyHints();
            if (board.CurrentLevel == null) return false;
            if (save.hintsRemainingToday <= 0) return false;
            var remaining = board.GetRemainingIds();
            var removed = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < remaining.Count; i++) { }

            var ids = solver.GetCurrentlyExtractable(board.CurrentLevel, removed);
            int id = ids.Count > 0 ? ids[0] : remaining[0];
            save.hintsRemainingToday--;
            saveSystem.Save(save);
            HintsChanged?.Invoke(save.hintsRemainingToday);
            StartCoroutine(ShowHintRoutine(id));
            return true;
        }

        public void RewardedHintsGranted()
        {
            RefreshDailyHints();
            save.hintsRemainingToday += config.rewardedHintAmount;
            saveSystem.Save(save);
            HintsChanged?.Invoke(save.hintsRemainingToday);
        }

        private IEnumerator ShowHintRoutine(int id)
        {
            if (activeHintId >= 0) board.HighlightPath(activeHintId, false);
            activeHintId = id;
            board.HighlightPath(id, true);
            yield return new WaitForSecondsRealtime(config.hintDuration);
            board.HighlightPath(id, false);
            activeHintId = -1;
        }

        private void RefreshDailyHints()
        {
            string today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            if (save.hintResetDateUtc == today) return;
            save.hintResetDateUtc = today;
            save.hintsRemainingToday = config.freeHintsPerDay;
            saveSystem.Save(save);
            HintsChanged?.Invoke(save.hintsRemainingToday);
        }
    }