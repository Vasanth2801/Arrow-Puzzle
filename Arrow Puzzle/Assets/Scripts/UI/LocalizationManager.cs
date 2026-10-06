using System;
using System.Collections.Generic;
using UnityEngine;

    [Serializable]
    public sealed class LocalizationEntry
    {
        public string key;
        public string english;
        public string secondLanguage;
    }

    [CreateAssetMenu(menuName = "Pathbound/Localization Table", fileName = "LocalizationTable")]
    public sealed class LocalizationTableSO : ScriptableObject
    {
        public string secondLanguageCode = "ta";
        public LocalizationEntry[] entries;

        public static LocalizationTableSO CreateRuntimeDefault()
        {
            var table = CreateInstance<LocalizationTableSO>();
            table.entries = new[]
            {
                new LocalizationEntry { key="play", english="PLAY", secondLanguage="விளையாடு" },
                new LocalizationEntry { key="continue", english="CONTINUE", secondLanguage="தொடர்க" },
                new LocalizationEntry { key="level_select", english="LEVEL SELECT", secondLanguage="நிலை தேர்வு" },
                new LocalizationEntry { key="themes", english="THEMES", secondLanguage="தீம்கள்" },
                new LocalizationEntry { key="settings", english="SETTINGS", secondLanguage="அமைப்புகள்" },
                new LocalizationEntry { key="daily_reward", english="DAILY REWARD", secondLanguage="தினசரி பரிசு" },
                new LocalizationEntry { key="retry", english="RETRY", secondLanguage="மீண்டும்" },
                new LocalizationEntry { key="next", english="NEXT", secondLanguage="அடுத்து" },
                new LocalizationEntry { key="home", english="HOME", secondLanguage="முகப்பு" },
                new LocalizationEntry { key="sound", english="Sound", secondLanguage="ஒலி" },
                new LocalizationEntry { key="music", english="Music", secondLanguage="இசை" },
                new LocalizationEntry { key="vibration", english="Vibration", secondLanguage="அதிர்வு" },
                new LocalizationEntry { key="hint", english="HINT", secondLanguage="குறிப்பு" },
                new LocalizationEntry { key="hard", english="HARD", secondLanguage="கடினம்" }
            };
            return table;
        }
    }

    public sealed class LocalizationManager : MonoBehaviour
    {
        [SerializeField] private LocalizationTableSO table;
        private SaveSystem saveSystem;
        private SaveData save;
        private readonly Dictionary<string, LocalizationEntry> lookup = new Dictionary<string, LocalizationEntry>();
        public event Action LanguageChanged;
        public string Language => save != null ? save.language : "en";

        public void Initialize(GameManager manager, SaveSystem saves, SaveData loaded)
        {
            saveSystem = saves;
            save = loaded;
            if (table == null) table = LocalizationTableSO.CreateRuntimeDefault();
            lookup.Clear();
            foreach (var entry in table.entries) if (entry != null) lookup[entry.key] = entry;
        }

        public string Get(string key)
        {
            if (!lookup.TryGetValue(key, out var e)) return key;
            return save.language == table.secondLanguageCode ? e.secondLanguage : e.english;
        }

        public void ReloadFromSave(SaveData loaded)
        {
            save = loaded;
            LanguageChanged?.Invoke();
        }

        public void SetLanguage(string code)
        {
            save.language = code == table.secondLanguageCode ? code : "en";
            saveSystem.Save(save);
            LanguageChanged?.Invoke();
        }
    }
