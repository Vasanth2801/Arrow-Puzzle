using System;
using System.Collections.Generic;
using UnityEngine;

    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private PathView pathPrefab;
        [SerializeField] private Transform pathRoot;

        private GameManager gameManager;
        private GameConfigSO config;
        private LevelSolver solver;
        private ThemeManager themes;
        private readonly List<PathView> pool = new List<PathView>();
        private readonly HashSet<int> extracted = new HashSet<int>();
        private LevelData currentLevel;
        private float cellSize;
        private Vector3 boardOrigin;
        private bool inputLocked;

        public int RemainingPaths => currentLevel == null ? 0 : currentLevel.paths.Count - extracted.Count;
        public LevelData CurrentLevel => currentLevel;
        public float CellSize => cellSize;
        public int GridSize => currentLevel != null ? currentLevel.gridSize : 0;

        public void Initialize(GameManager manager, GameConfigSO gameConfig, LevelSolver levelSolver, ThemeManager themeManager)
        {
            gameManager = manager;
            config = gameConfig;
            solver = levelSolver;
            themes = themeManager;
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (pathRoot == null) pathRoot = transform;
            themes.ThemeChanged += ApplyTheme;
        }

        public void LoadLevel(LevelData level)
        {
            currentLevel = level;
            extracted.Clear();
            inputLocked = false;
            LayoutBoard();
            ReturnAllToPool();

            var palette = themes.Current;
            for (int i = 0; i < level.paths.Count; i++)
            {
                var view = GetView();
                view.transform.localPosition = Vector3.zero;
                view.Initialize(level.paths[i], this, config, cellSize,
                    palette.path, palette.highlightStart, palette.highlightEnd);
            }
        }

        public void ClearBoard()
        {
            currentLevel = null;
            extracted.Clear();
            inputLocked = true;
            ReturnAllToPool();
        }

        public Vector3 GridToLocalWorld(Vector2Int cell)
        {
            float x = (cell.x - (currentLevel.gridSize - 1) * 0.5f) * cellSize;
            float y = (cell.y - (currentLevel.gridSize - 1) * 0.5f) * cellSize;
            return new Vector3(x, y, 0f);
        }

        public bool TryExtract(PathView view)
        {
            if (inputLocked || currentLevel == null || view == null || view.IsExtracted) return false;
            if (extracted.Contains(view.Id)) return false;

            if (!solver.CanExtract(currentLevel, view.Data, extracted))
            {
                view.PlayWrongBump();
                gameManager.NotifyWrongTap();
                if (gameManager.GetComponent<LivesSystem>().TryLoseLife() == false) return false;
                return false;
            }

            inputLocked = true;
            view.PlayExtraction(() => OnExtractionFinished(view));
            return true;
        }

        public void SetAllInputEnabled(bool enabled)
        {
            inputLocked = !enabled;
            for (int i = 0; i < pool.Count; i++)
                if (pool[i].gameObject.activeSelf && !pool[i].IsExtracted) pool[i].SetInputEnabled(enabled);
        }

        public PathView FindView(int id)
        {
            for (int i = 0; i < pool.Count; i++)
                if (pool[i].gameObject.activeSelf && pool[i].Id == id) return pool[i];
            return null;
        }

        public void HighlightPath(int id, bool on)
        {
            var view = FindView(id);
            if (view != null) view.Highlight(on);
        }

        public List<int> GetRemainingIds()
        {
            var ids = new List<int>();
            if (currentLevel == null) return ids;
            for (int i = 0; i < currentLevel.paths.Count; i++)
                if (!extracted.Contains(currentLevel.paths[i].id)) ids.Add(currentLevel.paths[i].id);
            return ids;
        }

        private void OnExtractionFinished(PathView view)
        {
            extracted.Add(view.Id);
            inputLocked = false;
            if (RemainingPaths == 0)
            {
                inputLocked = true;
                gameManager.OnCorrectExtractionCompleted();
            }
        }

        private void LayoutBoard()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            Rect safe = Screen.safeArea;
            float targetPixels = Mathf.Min(safe.width * config.boardWidthSafeArea, safe.height * config.boardHeightSafeArea);
            float pixelsPerWorld = Screen.height / (gameplayCamera.orthographicSize * 2f);
            float boardWorld = targetPixels / pixelsPerWorld;
            cellSize = boardWorld / currentLevel.gridSize;

            Vector2 centerScreen = new Vector2(safe.center.x, safe.yMin + safe.height * 0.47f);
            Vector3 centerWorld = gameplayCamera.ScreenToWorldPoint(new Vector3(centerScreen.x, centerScreen.y, -gameplayCamera.transform.position.z));
            pathRoot.position = new Vector3(centerWorld.x, centerWorld.y, 0f);
            boardOrigin = pathRoot.position;
        }

        private PathView GetView()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf) return pool[i];
            }
            var created = Instantiate(pathPrefab, pathRoot);
            created.name = "PathView_Pooled";
            pool.Add(created);
            return created;
        }

        private void ReturnAllToPool()
        {
            for (int i = 0; i < pool.Count; i++) pool[i].DisableAndPool();
        }

        private void ApplyTheme(ThemeData theme)
        {
            for (int i = 0; i < pool.Count; i++)
                if (pool[i].gameObject.activeSelf) pool[i].SetTheme(theme.path, theme.highlightStart, theme.highlightEnd);
        }
    }
