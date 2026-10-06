using UnityEngine;


    public sealed class SceneWiring : MonoBehaviour
    {
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private SettingsUI settingsUI;
        [SerializeField] private GameManager gameManager;

        private void Start()
        {
            mainMenuUI?.Bind(gameManager);
            settingsUI?.Bind(gameManager);
        }
    }
