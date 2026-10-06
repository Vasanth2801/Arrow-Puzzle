using System;
using UnityEngine;


    public sealed class AudioManager : MonoBehaviour
    {
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private SaveData save;
        private GameConfigSO config;
        private readonly System.Collections.Generic.Dictionary<string, AudioClip> clips = new System.Collections.Generic.Dictionary<string, AudioClip>();

        public void Initialize(GameConfigSO gameConfig, SaveData loaded)
        {
            config = gameConfig;
            save = loaded;
            musicSource = CreateSource("Music", true);
            sfxSource = CreateSource("SFX", false);
            CreateProceduralClips();
            ApplySettings();
        }

        public void ReloadFromSave(SaveData loaded)
        {
            save = loaded;
            ApplySettings();
        }

        public void SetSound(bool on) { save.soundOn = on; ApplySettings(); }
        public void SetMusic(bool on) { save.musicOn = on; ApplySettings(); }
        public bool SoundOn => save != null && save.soundOn;
        public bool MusicOn => save != null && save.musicOn;

        public void PlayTap() => Play("tap");
        public void PlaySlide() => Play("slide");
        public void PlayWrong() => Play("wrong");
        public void PlayWin() => Play("win");
        public void PlayLose() => Play("lose");
        public void PlayButton() => Play("button");

        private void Play(string key)
        {
            if (sfxSource == null || save == null || !save.soundOn) return;
            if (clips.TryGetValue(key, out var clip)) sfxSource.PlayOneShot(clip);
        }

        private AudioSource CreateSource(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        private void ApplySettings()
        {
            if (musicSource == null || sfxSource == null || save == null) return;
            musicSource.volume = save.musicOn ? 0.18f : 0f;
            sfxSource.volume = save.soundOn ? 0.7f : 0f;
            if (save.musicOn && musicSource.clip == null)
            {
                musicSource.clip = MakeTone("music", 4.0f, 110f, 0.12f);
                musicSource.Play();
            }
            else if (!save.musicOn) musicSource.Stop();
        }

        private void CreateProceduralClips()
        {
            clips["tap"] = MakeTone("tap", 0.06f, 420f, 0.16f);
            clips["slide"] = MakeTone("slide", 0.20f, 170f, 0.10f);
            clips["wrong"] = MakeTone("wrong", 0.14f, 95f, 0.16f);
            clips["win"] = MakeChord("win", new[] { 392f, 494f, 587f }, 0.5f);
            clips["lose"] = MakeTone("lose", 0.45f, 90f, 0.12f);
            clips["button"] = MakeTone("button", 0.05f, 620f, 0.10f);
        }

        private static AudioClip MakeTone(string name, float seconds, float frequency, float amplitude)
        {
            const int rate = 44100;
            int samples = Mathf.Max(1, Mathf.RoundToInt(seconds * rate));
            var clip = AudioClip.Create(name, samples, 1, rate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, i / (rate * 0.01f)) * Mathf.Min(1f, (samples - i) / (rate * 0.03f));
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeChord(string name, float[] frequencies, float seconds)
        {
            const int rate = 44100;
            int samples = Mathf.RoundToInt(seconds * rate);
            var clip = AudioClip.Create(name, samples, 1, rate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float sum = 0f;
                for (int j = 0; j < frequencies.Length; j++) sum += Mathf.Sin(2f * Mathf.PI * frequencies[j] * t);
                float env = Mathf.Min(1f, i / (rate * 0.02f)) * Mathf.Min(1f, (samples - i) / (rate * 0.08f));
                data[i] = sum / frequencies.Length * 0.14f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }