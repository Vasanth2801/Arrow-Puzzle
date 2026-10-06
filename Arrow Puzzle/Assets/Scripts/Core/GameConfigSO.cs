using UnityEngine;


    [CreateAssetMenu(menuName = "Pathbound/Game Config", fileName = "GameConfig")]
    public sealed class GameConfigSO : ScriptableObject
    {
        [Header("Generation")]
        public int baseSeed = 913742;
        [Min(5)] public int minGridSize = 9;
        [Min(5)] public int maxGridSize = 14;
        [Min(2)] public int minPathLength = 2;
        [Min(2)] public int maxPathLength = 7;
        [Min(1)] public int maxGenerationAttempts = 80;
        [Min(100)] public int maxPathPlacementAttempts = 2500;

        [Header("Presentation")]
        [Range(0.70f, 1f)] public float boardWidthSafeArea = 0.92f;
        [Range(0.40f, 0.80f)] public float boardHeightSafeArea = 0.60f;
        [Range(0.02f, 0.12f)] public float lineWidthAsCell = 0.055f;
        [Range(0.05f, 0.30f)] public float arrowLengthAsCell = 0.23f;
        [Range(0.03f, 0.20f)] public float arrowWidthAsCell = 0.14f;

        [Header("Animation")]
        public float extractDuration = 0.42f;
        public float wrongShakeDuration = 0.18f;
        public float wrongShakeDistanceAsCell = 0.08f;
        public float hintDuration = 1.8f;

        [Header("Game Rules")]
        [Min(1)] public int startingLives = 3;
        [Min(0)] public int freeHintsPerDay = 3;
        [Min(1)] public int rewardedHintAmount = 3;

        public static GameConfigSO CreateRuntimeDefault()
        {
            var config = CreateInstance<GameConfigSO>();
            return config;
        }
    }