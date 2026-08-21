using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private int levelNumber;

    public void Setup(int level)
    {
        levelNumber = level;

        if(button == null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(OpenLevel);
    }

    private void OpenLevel()
    {
        if(LevelManager.instance == null)
        {
            Debug.LogError("LevelManager not found");
            return;
        }

        LevelManager.instance.LoadLevel(levelNumber);
    }
}
