using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip buttonClip;
    public AudioClip arrowClear;
    public AudioClip wrongMove;
    public AudioClip levelComplete;
    public AudioClip gameOver;

    private void Awake()    
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // THE FIX: this line was missing before, so Instance stayed null
        // forever and every AudioManager.Instance.PlayX() call anywhere
        // in the game would have silently failed (or thrown a
        // NullReferenceException, depending on how it was called).
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonClick() => PlaySFX(buttonClip);
    public void PlayArrowClear() => PlaySFX(arrowClear);
    public void PlayWrongMove() => PlaySFX(wrongMove);
    public void PlayLevelComplete() => PlaySFX(levelComplete);
    public void PlayGameOver() => PlaySFX(gameOver);
}
