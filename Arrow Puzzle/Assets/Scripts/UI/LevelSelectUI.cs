using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public sealed class LevelSelectUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Button levelButtonPrefab;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private int visibleLevels = 1000;

        public void Build(GameManager manager)
        {
            for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);
            var save = manager.Save.Load();
            int max = Mathf.Max(1, visibleLevels);
            for (int level = 1; level <= max; level++)
            {
                int captured = level;
                var button = Instantiate(levelButtonPrefab, content);
                button.name = $"Level_{level:0000}";
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    int stars = save.GetStars(level);
                    label.text = stars > 0 ? $"{level}\n{new string('★', stars)}" : level.ToString();
                }
                button.interactable = level <= save.unlockedLevel;
                button.onClick.AddListener(() => manager.StartLevel(captured));
            }
            if (titleText != null) titleText.text = "LEVEL SELECT";
        }
    }