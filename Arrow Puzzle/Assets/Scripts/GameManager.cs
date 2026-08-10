using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [Header("Win condition")]
    [SerializeField] private GameObject winScreen;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void ShowWinScreen()
    {
        winScreen.SetActive(true);
    }


    public void Restart()
    {
        winScreen.SetActive(false);
        SceneManager.LoadScene(0);
    }
}
