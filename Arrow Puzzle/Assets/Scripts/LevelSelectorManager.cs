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
    }


}