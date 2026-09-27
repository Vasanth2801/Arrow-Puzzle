using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private AudioClip arrowClearClip;
    [SerializeField] private AudioClip wrongMoveClip;
    [SerializeField] private AudioClip levelCompleteClip;
    [SerializeField] private AudioClip gameOverClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayButton() => Play(buttonClip);
    public void PlayClear() => Play(arrowClearClip);
    public void PlayWrong() => Play(wrongMoveClip);
    public void PlayLevelComplete() => Play(levelCompleteClip);
    public void PlayGameOver() => Play(gameOverClip);

    private void Play(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }
}
