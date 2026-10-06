using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public sealed class ThemeUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Button themeButtonPrefab;
        [SerializeField] private TMP_Text titleText;

        public void Build(GameManager manager)
        {
            for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);
            foreach (var theme in manager.Themes.Themes)
            {
                var t = theme;
                var button = Instantiate(themeButtonPrefab, content);
                var label = button.GetComponentInChildren<TMP_Text>();
                bool unlocked = manager.Themes.IsUnlocked(t);
                if (label != null) label.text = unlocked ? t.displayName : $"{t.displayName}\nUnlock Lv.{t.unlockAtLevel}";
                button.interactable = unlocked;
                button.onClick.AddListener(() => manager.Themes.SelectTheme(t.id));
            }
            if (titleText != null) titleText.text = "THEMES";
        }
    }
