using System.Collections.Generic;
using UnityEngine;

    public sealed class LevelSolver
    {
        public bool CanExtract(LevelData level, GridPathData candidate, HashSet<int> removedIds = null)
        {
            var occupied = new HashSet<Vector2Int>();
            foreach (var path in level.paths)
            {
                if (path == candidate) continue;
                if (removedIds != null && removedIds.Contains(path.id)) continue;
                for (int i = 0; i < path.cells.Count; i++) occupied.Add(path.cells[i]);
            }

            var p = candidate.Head + candidate.headDirection;
            while (Inside(p, level.gridSize))
            {
                if (occupied.Contains(p)) return false;
                p += candidate.headDirection;
            }
            return true;
        }

        public SolveResult Solve(LevelData level)
        {
            var removed = new HashSet<int>();
            var order = new List<int>(level.paths.Count);

            while (order.Count < level.paths.Count)
            {
                GridPathData found = null;
                for (int i = 0; i < level.paths.Count; i++)
                {
                    var path = level.paths[i];
                    if (removed.Contains(path.id)) continue;
                    if (CanExtract(level, path, removed))
                    {
                        found = path;
                        break;
                    }
                }

                if (found == null)
                    return new SolveResult(false, order);

                removed.Add(found.id);
                order.Add(found.id);
            }

            return new SolveResult(true, order);
        }

        public List<int> GetCurrentlyExtractable(LevelData level, HashSet<int> removedIds = null)
        {
            var result = new List<int>();
            for (int i = 0; i < level.paths.Count; i++)
            {
                var path = level.paths[i];
                if (removedIds != null && removedIds.Contains(path.id)) continue;
                if (CanExtract(level, path, removedIds)) result.Add(path.id);
            }
            return result;
        }

        private static bool Inside(Vector2Int cell, int size)
            => cell.x >= 0 && cell.x < size && cell.y >= 0 && cell.y < size;
    }

    public readonly struct SolveResult
    {
        public readonly bool solvable;
        public readonly IReadOnlyList<int> extractionOrder;
        public SolveResult(bool solvable, IReadOnlyList<int> extractionOrder)
        {
            this.solvable = solvable;
            this.extractionOrder = extractionOrder;
        }
    }