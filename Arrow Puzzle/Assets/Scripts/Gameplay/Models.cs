using System;
using System.Collections.Generic;
using UnityEngine;


    [Serializable]
    public sealed class GridPathData
    {
        public int id;
        public List<Vector2Int> cells = new List<Vector2Int>();
        public Vector2Int headDirection;

        public Vector2Int Head => cells[cells.Count - 1];
    }

    [Serializable]
    public sealed class LevelData
    {
        public int levelNumber;
        public int gridSize;
        public bool hard;
        public int minimumCorrectTaps;
        public List<GridPathData> paths = new List<GridPathData>();
    }

    public readonly struct LevelDifficulty
    {
        public readonly int gridSize;
        public readonly int pathCount;
        public readonly int minLength;
        public readonly int maxLength;
        public readonly int maxBends;

        public LevelDifficulty(int gridSize, int pathCount, int minLength, int maxLength, int maxBends)
        {
            this.gridSize = gridSize;
            this.pathCount = pathCount;
            this.minLength = minLength;
            this.maxLength = maxLength;
            this.maxBends = maxBends;
        }
    }