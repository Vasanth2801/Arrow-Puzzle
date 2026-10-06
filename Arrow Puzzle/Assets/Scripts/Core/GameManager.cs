using System;
using System.Collections.Generic;
using UnityEngine;


    public enum GameState { Boot, MainMenu, Playing, Paused, Win, Lose }

    public sealed class GameManager : MonoBehaviour
    {
        [Header("Composition Root")]
        [SerializeField] private GameConfigSO config;
        [SerializeField] private BoardView boardView;
        [SerializeField] private LivesSystem livesSystem;
        [SerializeField] private HintSystem hintSystem;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private ThemeManager themeManager;
        [SerializeField] private LocalizationManager localizationManager;

        private SaveSystem saveSystem;
        private LevelGenerator generator;
        private LevelSolver solver;
        private readonly Dictionary<int, LevelData> levelCache = new Dictionary<int, LevelData>();
        private bool initialized;

        public event Action<GameState> StateChanged;
        public event Action<LevelData> LevelLoaded;
        public event Action<int, int> LevelWon;
        public event Action<int> LevelLost;

        public GameState State { get; private set; } = GameState.Boot;
        public int CurrentLevel { get; private set; }
        public GameConfigSO Config => config;
        public SaveSystem Save => saveSystem;
        public LevelSolver Solver => solver;
        public ThemeManager Themes => themeManager;
        public LocalizationManager Localization => localizationManager;
        public int Lives => livesSystem != null ? livesSystem.CurrentLives : 0;

        private void Awake()
        {
            if (initialized) return;
            initialized = true;
            if (config == null) config = GameConfigSO.CreateRuntimeDefault();

            saveSystem = new SaveSystem();
            var save = saveSystem.Load();
            generator = new LevelGenerator(config);
            solver = new LevelSolver();

            themeManager.Initialize(this, saveSystem, save);
            localizationManager.Initialize(this, saveSystem, save);
            audioManager.Initialize(config, save);
            livesSystem.Initialize(config.startingLives, save);
            livesSystem.LivesDepleted += OnLifeDepleted;
            hintSystem.Initialize(this, boardView, solver, config, saveSystem, save);
            boardView.Initialize(this, config, solver, themeManager);

            CurrentLevel = Mathf.Max(1, save.currentLevel);
            State = GameState.MainMenu;
            StateChanged?.Invoke(State);

            CacheLevel(CurrentLevel);
            CacheLevel(CurrentLevel + 1);
            CacheLevel(CurrentLevel + 2);
            uiManager.Initialize(this);
            uiManager.ShowMainMenu();
        }

        public void ContinueGame()
        {
            LoadLevel(CurrentLevel);
        }

        public void StartLevel(int level)
        {
            if (level < 1 || level > saveSystem.Load().unlockedLevel) return;
            CurrentLevel = level;
            LoadLevel(level);
        }

        public void RestartLevel()
        {
            LoadLevel(CurrentLevel);
        }

        public void LoadLevel(int level)
        {
            CurrentLevel = Mathf.Max(1, level);
            var data = GetLevel(CurrentLevel);
            livesSystem.Reset(config.startingLives);
            hintSystem.NotifyLevelStarted(CurrentLevel);
            boardView.LoadLevel(data);
            SetState(GameState.Playing);
            uiManager.ShowGameplay(data);
            saveSystem.Mutate(s => s.currentLevel = CurrentLevel);
            LevelLoaded?.Invoke(data);

            // Generation is deterministic, so caching only improves perceived load time.
            CacheLevel(CurrentLevel + 1);
            CacheLevel(CurrentLevel + 2);
        }

        public void PauseGame()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Paused);
            uiManager.ShowPause();
        }

        public void ResumeGame()
        {
            if (State != GameState.Paused) return;
            SetState(GameState.Playing);
            uiManager.HidePopup();
        }

        public void Home()
        {
            boardView.ClearBoard();
            SetState(GameState.MainMenu);
            uiManager.ShowMainMenu();
        }

        public void OnCorrectExtractionCompleted()
        {
            if (boardView.RemainingPaths > 0) return;
            int stars = Mathf.Clamp(livesSystem.CurrentLives, 1, 3);
            int previousStars = saveSystem.Load().GetStars(CurrentLevel);
            if (stars > previousStars) saveSystem.SetStars(CurrentLevel, stars);
            saveSystem.Mutate(s => s.unlockedLevel = Mathf.Max(s.unlockedLevel, CurrentLevel + 1));
            audioManager.PlayWin();
            if (saveSystem.Load().vibrationOn) Haptics.Win();
            SetState(GameState.Win);
            uiManager.ShowWin(CurrentLevel, stars);
            LevelWon?.Invoke(CurrentLevel, stars);
        }

        public void NotifyWrongTap()
        {
            audioManager.PlayWrong();
            if (saveSystem.Load().vibrationOn) Haptics.Wrong();
        }

        public void OnLifeDepleted()
        {
            audioManager.PlayLose();
            SetState(GameState.Lose);
            uiManager.ShowLose(CurrentLevel);
            LevelLost?.Invoke(CurrentLevel);
        }

        public void ReviveFromAd()
        {
            if (State != GameState.Lose) return;
            livesSystem.Reset(config.startingLives);
            SetState(GameState.Playing);
            uiManager.HidePopup();
        }

        public void CompleteDailyReward(int hintsAwarded)
        {
            saveSystem.Mutate(s =>
            {
                string today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
                DateTime previous;
                bool consecutive = DateTime.TryParse(s.lastDailyClaimUtc, out previous) && previous.Date == DateTime.UtcNow.Date.AddDays(-1);
                s.dailyStreak = consecutive ? (s.dailyStreak >= 7 ? 1 : s.dailyStreak + 1) : 1;
                s.hintsRemainingToday += hintsAwarded;
                s.lastDailyClaimUtc = today;
            });
        }

        public void RestoreProgress()
        {
            saveSystem.ResetProgress();
            var fresh = saveSystem.Load();
            CurrentLevel = 1;
            themeManager.ReloadFromSave(fresh);
            localizationManager.ReloadFromSave(fresh);
            audioManager.ReloadFromSave(fresh);
            hintSystem.ReloadFromSave(fresh);
            livesSystem.Reset(config.startingLives);
            Home();
        }

        private LevelData GetLevel(int level)
        {
            if (!levelCache.TryGetValue(level, out var data))
            {
                data = generator.Generate(level);
                levelCache[level] = data;
            }
            return data;
        }

        private void CacheLevel(int level)
        {
            if (level < 1 || level > 100000 || levelCache.ContainsKey(level)) return;
            levelCache[level] = generator.Generate(level);
        }

        private void SetState(GameState newState)
        {
            State = newState;
            Time.timeScale = newState == GameState.Paused ? 0f : 1f;
            StateChanged?.Invoke(newState);
        }
    }