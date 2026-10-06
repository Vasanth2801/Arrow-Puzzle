using System;
using UnityEngine;


    public sealed class AdService : MonoBehaviour
    {
        public void ShowRewarded(Action reward)
        {
            // Integration point for Unity LevelPlay/AdMob/Unity Ads later.
            Debug.Log("Rewarded ad stub: reward granted immediately in development build.");
            reward?.Invoke();
        }
    }
