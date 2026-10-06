using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


    public sealed class AppBootstrap : MonoBehaviour
    {
        [SerializeField] private string nextScene = "Main";
        [SerializeField] private float minimumSplashTime = 1.0f;

        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(minimumSplashTime);
            if (!string.IsNullOrWhiteSpace(nextScene))
                SceneManager.LoadScene(nextScene);
        }
    }
