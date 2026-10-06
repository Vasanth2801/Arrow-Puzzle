using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public sealed class SettingsUI : MonoBehaviour
    {
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private GameObject privacyPanel;

        private GameManager manager;

        public void Bind(GameManager gameManager)
        {
            manager = gameManager;
            var save = manager.Save.Load();
            soundToggle.isOn = save.soundOn;
            musicToggle.isOn = save.musicOn;
            vibrationToggle.isOn = save.vibrationOn;
            if (languageDropdown != null)
            {
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new System.Collections.Generic.List<string> { "English", "தமிழ்" });
                languageDropdown.value = save.language == "ta" ? 1 : 0;
            }
        }

        public void SetSound(bool on)
        {
            manager.GetComponent<AudioManager>().SetSound(on);
            manager.Save.Mutate(s => s.soundOn = on);
        }
        public void SetMusic(bool on)
        {
            manager.GetComponent<AudioManager>().SetMusic(on);
            manager.Save.Mutate(s => s.musicOn = on);
        }
        public void SetVibration(bool on) => manager.Save.Mutate(s => s.vibrationOn = on);
        public void SetLanguage(int index) => manager.Localization.SetLanguage(index == 1 ? "ta" : "en");
        public void RestoreProgress() => manager.RestoreProgress();
        public void OpenPrivacy() { if (privacyPanel != null) privacyPanel.SetActive(true); }
        public void ClosePrivacy() { if (privacyPanel != null) privacyPanel.SetActive(false); }
    }
