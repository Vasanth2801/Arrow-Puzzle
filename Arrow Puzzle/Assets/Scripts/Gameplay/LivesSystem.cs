using System;
using UnityEngine;

    public sealed class LivesSystem : MonoBehaviour
    {
        public event Action<int> LivesChanged;
        public event Action LivesDepleted;
        public int CurrentLives { get; private set; }
        public int MaxLives { get; private set; }
        private SaveSystem saveSystem;

        public void Initialize(int maxLives, SaveData save)
        {
            saveSystem = new SaveSystem();
            MaxLives = Mathf.Max(1, maxLives);
            CurrentLives = Mathf.Clamp(save.lives <= 0 ? MaxLives : save.lives, 0, MaxLives);
            LivesChanged?.Invoke(CurrentLives);
        }

        public bool TryLoseLife()
        {
            if (CurrentLives <= 0) return false;
            CurrentLives--;
            LivesChanged?.Invoke(CurrentLives);
            saveSystem.Mutate(s => s.lives = CurrentLives);
            if (CurrentLives == 0) LivesDepleted?.Invoke();
            return true;
        }

        public void Reset(int amount)
        {
            CurrentLives = Mathf.Clamp(amount, 0, MaxLives);
            LivesChanged?.Invoke(CurrentLives);
            saveSystem.Mutate(s => s.lives = CurrentLives);
        }

        public void RestoreFull() => Reset(MaxLives);
    }
