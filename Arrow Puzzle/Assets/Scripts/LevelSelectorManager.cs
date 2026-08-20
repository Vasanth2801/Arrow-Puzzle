using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class LevelSelectorManager : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private Button[] levelButtons;

    [Header("Page Settings")]
    [SerializeField] private int levelPerPage = 20;
    private int currentPage = 0;
    private int totalLevels;

    private void Start()
    {
        LevelManager levelManager = FindAnyObjectByType<LevelManager>();

        if(levelManager != null )
        {
            totalLevels = levelManager.GetTotallevelCount();
        }

        RefreshPage();
    }

    private void RefreshPage()
    {
        int startLevel = currentPage * levelPerPage;

        for(int i =0; i < levelButtons.Length; i++)
        {
            int levelNumber = startLevel + i + 1;

            Button button = levelButtons[i];

            if(levelNumber > totalLevels)
            {
                button.gameObject.SetActive(false);
                continue;
            }

            button.gameObject.SetActive(true);

            button.interactable = levelNumber <= GetUnlockedLevel();

            int selectedLevel = levelNumber;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => LoadLevel(selectedLevel));
        }
    }

    private int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt("unlockedLevel", 1);
    }

    private void LoadLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("selectedLevel", levelNumber);

        PlayerPrefs.Save();

        Debug.Log("Selected Level: " + levelNumber);
    }

    public void NextPage()
    {
        int maxPage = Mathf.CeilToInt((float)totalLevels/ levelPerPage) -1;

        if(currentPage < maxPage)
        {
            currentPage++;

            RefreshPage();
        }
    }

    public void PreviousPage()
    {
        if(currentPage > 0)
        {
            currentPage--;

            RefreshPage();
        }
    }
}