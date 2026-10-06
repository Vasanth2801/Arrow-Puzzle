using System;
using System.IO;
using UnityEngine;


    public sealed class SaveSystem
    {
        private const string FileName = "pathbound_save.json";
        private string PathName => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public SaveData Load()
        {
            try
            {
                if (!File.Exists(PathName)) return new SaveData();
                var json = File.ReadAllText(PathName);
                var data = JsonUtility.FromJson<SaveData>(json);
                return data ?? new SaveData();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Save load failed; using defaults. {ex.Message}");
                return new SaveData();
            }
        }

        public void Save(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string temp = PathName + ".tmp";
                File.WriteAllText(temp, json);
                if (File.Exists(PathName)) File.Delete(PathName);
                File.Move(temp, PathName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save failed: {ex.Message}");
            }
        }

        public void Mutate(Action<SaveData> mutation)
        {
            var data = Load();
            mutation?.Invoke(data);
            Save(data);
        }

        public void SetStars(int level, int stars)
        {
            var data = Load();
            if (level >= data.stars.Length) Array.Resize(ref data.stars, level + 1);
            data.stars[level] = Mathf.Max(data.stars[level], stars);
            Save(data);
        }

        public void ResetProgress()
        {
            Save(new SaveData());
        }
    }