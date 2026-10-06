using System;
using System.Collections.Generic;
using UnityEngine;

    public sealed class LevelGenerator
    {
        private readonly GameConfigSO config;
        private readonly LevelSolver solver = new LevelSolver();

        public LevelGenerator(GameConfigSO config) { this.config = config; }

        public LevelData Generate(int levelNumber)
        {
            var difficulty = GetDifficulty(levelNumber);
            for (int attempt = 0; attempt < config.maxGenerationAttempts; attempt++)
            {
                var rng = new System.Random(SeedFor(levelNumber, attempt));
                var data = TryGenerate(levelNumber, difficulty, rng);
                if (data != null && solver.Solve(data).solvable)
                {
                    data.minimumCorrectTaps = data.paths.Count;
                    return data;
                }
            }

            return GenerateGuaranteed(levelNumber, difficulty);
        }

        public LevelDifficulty GetDifficulty(int level)
        {
            int t = Mathf.Max(0, level - 1);
            int grid = Mathf.Clamp(config.minGridSize + t / 167, config.minGridSize, config.maxGridSize);
            int paths = Mathf.Clamp(7 + t / 55, 7, 24);
            int minLen = Mathf.Clamp(config.minPathLength + t / 180, config.minPathLength, 5);
            int maxLen = Mathf.Clamp(minLen + 2 + t / 350, minLen, config.maxPathLength);
            int bends = Mathf.Clamp(1 + t / 140, 1, 5);
            return new LevelDifficulty(grid, paths, minLen, maxLen, bends);
        }

        private LevelData TryGenerate(int level, LevelDifficulty d, System.Random rng)
        {
            var occupied = new HashSet<Vector2Int>();
            var data = new LevelData
            {
                levelNumber = level,
                gridSize = d.gridSize,
                hard = level % 10 == 0
            };

            for (int id = 0; id < d.pathCount; id++)
            {
                GridPathData path = null;
                for (int attempt = 0; attempt < config.maxPathPlacementAttempts; attempt++)
                {
                    path = TryCreatePath(id, d, rng, occupied);
                    if (path != null) break;
                }

                if (path == null) return null;
                data.paths.Add(path);
                for (int i = 0; i < path.cells.Count; i++) occupied.Add(path.cells[i]);
            }

            data.minimumCorrectTaps = data.paths.Count;
            return data;
        }

        private GridPathData TryCreatePath(int id, LevelDifficulty d, System.Random rng, HashSet<Vector2Int> occupied)
        {
            var directions = CardinalDirections;
            Vector2Int head = new Vector2Int(rng.Next(d.gridSize), rng.Next(d.gridSize));
            Vector2Int headDir = directions[rng.Next(directions.Length)];
            var pathCells = new List<Vector2Int>();
            var local = new HashSet<Vector2Int>();

            if (occupied.Contains(head)) return null;
            Vector2Int previous = head - headDir;
            if (!Inside(previous, d.gridSize) || occupied.Contains(previous)) return null;

            // The head must have a clear exit against every path already placed.
            var ray = head + headDir;
            while (Inside(ray, d.gridSize))
            {
                if (occupied.Contains(ray)) return null;
                ray += headDir;
            }

            pathCells.Add(head);
            local.Add(head);
            pathCells.Add(previous);
            local.Add(previous);

            int targetLength = rng.Next(d.minLength, d.maxLength + 1);
            Vector2Int current = previous;
            Vector2Int previousStep = -headDir;
            int bends = 0;

            while (pathCells.Count < targetLength)
            {
                var candidates = new List<Vector2Int>(4);
                for (int i = 0; i < directions.Length; i++)
                {
                    var dir = directions[i];
                    if (dir == -previousStep) continue;
                    if (dir != previousStep && bends >= d.maxBends) continue;
                    var next = current + dir;
                    if (!Inside(next, d.gridSize) || occupied.Contains(next) || local.Contains(next)) continue;
                    candidates.Add(dir);
                }

                if (candidates.Count == 0) break;
                var chosen = candidates[rng.Next(candidates.Count)];
                if (chosen != previousStep) bends++;
                current += chosen;
                pathCells.Add(current);
                local.Add(current);
                previousStep = chosen;
            }

            if (pathCells.Count < d.minLength) return null;

            pathCells.Reverse(); // tail -> head
            return new GridPathData { id = id, cells = pathCells, headDirection = headDir };
        }

        private LevelData GenerateGuaranteed(int level, LevelDifficulty d)
        {
            // Deterministic fallback: fill the board with disjoint horizontal snakes.
            // Every head points toward an open right edge, so the fallback is always solvable.
            var data = new LevelData { levelNumber = level, gridSize = d.gridSize, hard = level % 10 == 0 };
            var occupied = new HashSet<Vector2Int>();
            int id = 0;

            for (int y = 0; y < d.gridSize && id < d.pathCount; y += 2)
            {
                for (int x = 0; x < d.gridSize && id < d.pathCount; x += 3)
                {
                    int length = Mathf.Min(Mathf.Max(2, d.minLength), d.gridSize - x);
                    if (length < 2) continue;
                    var cells = new List<Vector2Int>(length);
                    for (int k = 0; k < length; k++)
                    {
                        var c = new Vector2Int(x + k, y);
                        if (occupied.Contains(c)) { cells.Clear(); break; }
                        cells.Add(c);
                    }
                    if (cells.Count < 2) continue;
                    foreach (var c in cells) occupied.Add(c);
                    data.paths.Add(new GridPathData { id = id++, cells = cells, headDirection = Vector2Int.right });
                }
            }

            // If the row packing did not fit enough paths, add vertical two-cell snakes.
            for (int x = 0; x < d.gridSize && id < d.pathCount; x++)
            {
                for (int y = 0; y < d.gridSize - 1 && id < d.pathCount; y++)
                {
                    var a = new Vector2Int(x, y);
                    var b = new Vector2Int(x, y + 1);
                    if (occupied.Contains(a) || occupied.Contains(b)) continue;
                    occupied.Add(a); occupied.Add(b);
                    data.paths.Add(new GridPathData { id = id++, cells = new List<Vector2Int> { a, b }, headDirection = Vector2Int.up });
                }
            }

            data.minimumCorrectTaps = data.paths.Count;
            return data;
        }

        private int SeedFor(int level, int attempt)
        {
            unchecked
            {
                int x = config.baseSeed;
                x = x * 397 ^ level;
                x = x * 397 ^ attempt * 7919;
                return x;
            }
        }

        private static bool Inside(Vector2Int c, int size)
            => c.x >= 0 && c.x < size && c.y >= 0 && c.y < size;

        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };
    }
