using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelButton levelButtonPrefab;
    [SerializeField] private Transform levelButtonParent;

    [Header("Settings")]
    [SerializeField] private int totalLevels = 100;

    private void Start()
    {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        if(levelButtonPrefab == null)
        {
            Debug.LogError("Level Button prefab is missing");

            return;
        }

        if(levelButtonParent == null)
        {
            Debug.LogError("Level Button Parent is missing");

            return;
        }

        for(int i =0; i<= totalLevels; i++)
        {
            LevelButton button = Instantiate(levelButtonPrefab, levelButtonParent);

            button.Setup(i);
        }
    }

}