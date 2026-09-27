using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (FindFirstObjectByType<GameProgress>() == null)
        {
            GameObject go = new GameObject("GameProgress");
            go.AddComponent<GameProgress>();
        }

        if (FindFirstObjectByType<AudioManager>() == null)
        {
            GameObject go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }
    }
}
