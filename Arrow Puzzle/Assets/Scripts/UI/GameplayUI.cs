using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public sealed class GameplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text[] lifeDrops;
        [SerializeField] private TMP_Text hintCountText;
        [SerializeField] private Image[] lifeImages;
        [SerializeField] private Button backButton;
        [SerializeField] private Button hintButton;

        private GameManager manager;
        private int previousLives = -1;

        public void Bind(GameManager gameManager, LevelData level)
        {
            manager = gameManager;
            levelText.text = $"LEVEL {level.levelNumber}" + (level.hard ? "  •  HARD" : "");
            var lives = manager.GetComponent<LivesSystem>();
            lives.LivesChanged -= RefreshLives;
            lives.LivesChanged += RefreshLives;
            var hints = manager.GetComponent<HintSystem>();
            hints.HintsChanged -= RefreshHints;
            hints.HintsChanged += RefreshHints;
            previousLives = -1;
            RefreshLives(lives.CurrentLives);
            RefreshHints(hints.RemainingFreeHints);
        }

        public void OnBack() => manager.PauseGame();
        public void OnHint() => manager.GetComponent<UIManager>().OnHint();

        private void RefreshLives(int current)
        {
            int old = previousLives;
            previousLives = current;
            for (int i = 0; i < lifeImages.Length; i++) lifeImages[i].enabled = i < current;
            for (int i = 0; i < lifeDrops.Length; i++) lifeDrops[i].gameObject.SetActive(i < current);
            if (old > current && current >= 0 && current < lifeDrops.Length)
                StartCoroutine(PopLostLife(lifeDrops[current].rectTransform));
        }

        private System.Collections.IEnumerator PopLostLife(RectTransform target)
        {
            Vector3 start = target.localScale;
            target.localScale = start * 1.25f;
            float t = 0f;
            while (t < 0.14f)
            {
                t += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(start * 1.25f, Vector3.zero, Mathf.Clamp01(t / 0.14f));
                yield return null;
            }
            target.localScale = start;
        }

        private void RefreshHints(int count)
        {
            if (hintCountText != null) hintCountText.text = count.ToString();
        }
    }