using System;
using UnityEngine;


    [Serializable]
    public sealed class ThemeData
    {
        public string id = "midnight";
        public string displayName = "Midnight";
        public int unlockAtLevel = 1;
        public Color background = new Color(0.055f, 0.09f, 0.11f);
        public Color path = new Color(0.48f, 0.57f, 0.69f);
        public Color highlightStart = new Color(1f, 0.55f, 0.08f);
        public Color highlightEnd = new Color(1f, 0.78f, 0.20f);
        public Color accent = new Color(0.25f, 0.42f, 0.95f);
        public Color text = Color.white;
    }

    [CreateAssetMenu(menuName = "Pathbound/Theme Library", fileName = "ThemeLibrary")]
    public sealed class ThemeLibrarySO : ScriptableObject
    {
        public ThemeData[] themes;

        public static ThemeLibrarySO CreateRuntimeDefault()
        {
            var lib = CreateInstance<ThemeLibrarySO>();
            lib.themes = new[]
            {
                new ThemeData { id="midnight", displayName="Midnight", unlockAtLevel=1,
                    background=new Color32(24,34,42,255), path=new Color32(142,161,188,255), highlightStart=new Color32(255,174,45,255), highlightEnd=new Color32(255,213,80,255), accent=new Color32(65,105,225,255) },
                new ThemeData { id="ocean", displayName="Ocean", unlockAtLevel=10,
                    background=new Color32(13,42,53,255), path=new Color32(116,181,192,255), highlightStart=new Color32(36,211,196,255), highlightEnd=new Color32(110,247,222,255), accent=new Color32(54,151,220,255) },
                new ThemeData { id="violet", displayName="Violet", unlockAtLevel=25,
                    background=new Color32(36,26,55,255), path=new Color32(166,146,194,255), highlightStart=new Color32(232,100,255,255), highlightEnd=new Color32(255,174,255,255), accent=new Color32(137,92,255,255) },
                new ThemeData { id="forest", displayName="Forest", unlockAtLevel=50,
                    background=new Color32(22,42,32,255), path=new Color32(133,166,143,255), highlightStart=new Color32(119,224,118,255), highlightEnd=new Color32(200,255,145,255), accent=new Color32(69,172,105,255) },
                new ThemeData { id="ember", displayName="Ember", unlockAtLevel=100,
                    background=new Color32(50,27,24,255), path=new Color32(188,153,143,255), highlightStart=new Color32(255,88,47,255), highlightEnd=new Color32(255,196,72,255), accent=new Color32(232,88,65,255) }
            };
            return lib;
        }
    }