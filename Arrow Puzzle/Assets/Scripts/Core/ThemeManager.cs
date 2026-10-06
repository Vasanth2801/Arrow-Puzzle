using System;
using UnityEngine;

    public sealed class ThemeManager : MonoBehaviour
    {
        [SerializeField] private ThemeLibrarySO library;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private UnityEngine.UI.Image backgroundImage;

        private GameManager gameManager;
        private SaveSystem saveSystem;
        private SaveData save;

        public ThemeData Current { get; private set; }
        public event Action<ThemeData> ThemeChanged;

        public void Initialize(GameManager manager, SaveSystem saves, SaveData loaded)
        {
            gameManager = manager;
            saveSystem = saves;
            save = loaded;
            if (library == null) library = ThemeLibrarySO.CreateRuntimeDefault();
            Apply(save.selectedThemeId, false);
        }

        public ThemeData[] Themes => library.themes;

        public bool IsUnlocked(ThemeData theme) => theme != null && save.unlockedLevel >= theme.unlockAtLevel;

        public void ReloadFromSave(SaveData loaded)
        {
            save = loaded;
            Apply(save.selectedThemeId, false);
        }

        public bool SelectTheme(string id)
        {
            var theme = Find(id);
            if (theme == null || !IsUnlocked(theme)) return false;
            Apply(id, true);
            return true;
        }

        private void Apply(string id, bool saveSelection)
        {
            Current = Find(id) ?? library.themes[0];
            if (backgroundImage != null) backgroundImage.color = Current.background;
            if (gameplayCamera != null) gameplayCamera.backgroundColor = Current.background;
            if (saveSelection)
            {
                save.selectedThemeId = Current.id;
                saveSystem.Save(save);
            }
            ThemeChanged?.Invoke(Current);
        }

        private ThemeData Find(string id)
        {
            for (int i = 0; i < library.themes.Length; i++)
                if (library.themes[i].id == id) return library.themes[i];
            return null;
        }
    }