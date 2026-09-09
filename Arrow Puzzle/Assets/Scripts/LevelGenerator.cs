using System.Collections.Generic;
using UnityEngine;

public class PathData
{
    public List<Vector2Int> cells = new List<Vector2Int>();
    public bool cleared = false;

    public Vector2Int Head => cells[cells.Count - 1];
}

public static class LevelGenerator
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public static List<PathData> Generate(
        int width,
        int height,
        int maxPathLength,
        out int[,] cellPathId)
    {
        // Safety check.
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        maxPathLength = Mathf.Max(2, maxPathLength);

        cellPathId = CreateEmptyGrid(width, height);

        int area = width * height;

        // Keep path count reasonable for the board.
        int targetPathCount = Mathf.Clamp(Mathf.RoundToInt(area / 4f),4,12);

        targetPathCount = Mathf.Min(targetPathCount,Mathf.Max(1, area / 3));

        // Try several times to get a good puzzle.
        const int attempts = 100;

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            List<PathData> paths = TryGenerate(
                width,
                height,
                maxPathLength,
                targetPathCount,
                out cellPathId
            );

            if (paths == null)
            {
                continue;
            }

            if (!HasFreeMove(paths,cellPathId,width,height))
            {
                continue;
            }

            if (!HasBlockingRelationship(paths,cellPathId,width,height))
            {
                continue;
            }

            if (!HasDirectionVariety(paths))
            {
                continue;
            }

            if (!IsSolvable(paths,cellPathId,width,height))
            {
                continue;
            }

            Debug.Log($"[GENERATOR] Valid puzzle generated: " + $"{paths.Count} paths | Board {width}x{height}");

            return paths;
        }

        // If random generation fails, use a guaranteed safe fallback.
        Debug.LogWarning("[GENERATOR] Random generation failed. " + "Using safe fallback.");

        return CreateFallback(width,height,maxPathLength,out cellPathId);
    }

    // ================================================================
    // MAIN GENERATOR
    // ================================================================

    private static List<PathData> TryGenerate(int width,int height,int maxPathLength,int targetPathCount,out int[,] cellPathId)
    {
        cellPathId = CreateEmptyGrid(width, height);

        bool[,] occupied = new bool[width, height];

        List<PathData> paths = new List<PathData>();

        int pathAttempts = targetPathCount * 30;

        for (int attempt = 0;
             attempt < pathAttempts &&
             paths.Count < targetPathCount;
             attempt++)
        {
            // --------------------------------------------------------
            // Pick a valid head.
            // --------------------------------------------------------

            Vector2Int head = GetRandomFreeCell(
                occupied,
                width,
                height
            );

            if (!InBounds(head, width, height))
                continue;

            if (occupied[head.x, head.y])
                continue;

            // --------------------------------------------------------
            // Pick an arrow direction.
            // --------------------------------------------------------

            List<Vector2Int> validDirections =
                GetValidHeadDirections(
                    head,
                    width,
                    height
                );

            if (validDirections.Count == 0)
                continue;

            Shuffle(validDirections);

            Vector2Int direction =
                validDirections[0];

            // --------------------------------------------------------
            // Build the path backwards.
            // --------------------------------------------------------

            List<Vector2Int> cells =
                BuildPathBackwards(
                    head,
                    direction,
                    occupied,
                    width,
                    height,
                    maxPathLength
                );

            if (cells == null)
                continue;

            if (cells.Count < 2)
                continue;

            // --------------------------------------------------------
            // Validate EVERY cell before touching arrays.
            // --------------------------------------------------------

            bool valid = true;

            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];

                if (!InBounds(cell, width, height))
                {
                    valid = false;
                    break;
                }

                if (occupied[cell.x, cell.y])
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            // --------------------------------------------------------
            // Reverse:
            //
            // tail -> ... -> head
            // --------------------------------------------------------

            cells.Reverse();

            PathData path = new PathData();

            for (int i = 0; i < cells.Count; i++)
            {
                path.cells.Add(cells[i]);
            }

            // --------------------------------------------------------
            // Register cells.
            // --------------------------------------------------------

            int pathId = paths.Count;

            for (int i = 0; i < path.cells.Count; i++)
            {
                Vector2Int cell = path.cells[i];

                // Final safety check.
                if (!InBounds(cell, width, height))
                {
                    valid = false;
                    break;
                }

                occupied[cell.x, cell.y] = true;
                cellPathId[cell.x, cell.y] = pathId;
            }

            if (!valid)
                continue;

            paths.Add(path);
        }

        if (paths.Count < targetPathCount)
            return null;

        return paths;
    }

    // ================================================================
    // RANDOM FREE CELL
    // ================================================================

    private static Vector2Int GetRandomFreeCell(
        bool[,] occupied,
        int width,
        int height)
    {
        // Try random positions first.
        for (int i = 0; i < 50; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (!occupied[x, y])
                return new Vector2Int(x, y);
        }

        // Guaranteed scan.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!occupied[x, y])
                    return new Vector2Int(x, y);
            }
        }

        // No free cell.
        return new Vector2Int(-1, -1);
    }

    // ================================================================
    // HEAD DIRECTIONS
    // ================================================================

    private static List<Vector2Int> GetValidHeadDirections(
        Vector2Int head,
        int width,
        int height)
    {
        List<Vector2Int> result =
            new List<Vector2Int>();

        for (int i = 0; i < Directions.Length; i++)
        {
            Vector2Int direction =
                Directions[i];

            // Cell behind the head must exist.
            Vector2Int previous =
                head - direction;

            if (InBounds(
                previous,
                width,
                height))
            {
                result.Add(direction);
            }
        }

        return result;
    }

    // ================================================================
    // BUILD PATH BACKWARDS
    // ================================================================

    private static List<Vector2Int> BuildPathBackwards(
        Vector2Int head,
        Vector2Int headDirection,
        bool[,] occupied,
        int width,
        int height,
        int maxPathLength)
    {
        // Absolutely validate head first.
        if (!InBounds(
            head,
            width,
            height))
        {
            return null;
        }

        List<Vector2Int> path =
            new List<Vector2Int>();

        path.Add(head);

        // ------------------------------------------------------------
        // First cell behind the head.
        // ------------------------------------------------------------

        Vector2Int current =
            head - headDirection;

        if (!InBounds(
            current,
            width,
            height))
        {
            return null;
        }

        if (occupied[current.x, current.y])
            return null;

        path.Add(current);

        // ------------------------------------------------------------
        // Continue backwards.
        // ------------------------------------------------------------

        while (path.Count < maxPathLength)
        {
            List<Vector2Int> candidates =
                new List<Vector2Int>();

            for (int i = 0;
                 i < Directions.Length;
                 i++)
            {
                Vector2Int next =
                    current + Directions[i];

                if (!InBounds(
                    next,
                    width,
                    height))
                {
                    continue;
                }

                if (occupied[next.x, next.y])
                    continue;

                if (path.Contains(next))
                    continue;

                candidates.Add(next);
            }

            if (candidates.Count == 0)
                break;

            // Prefer continuing in the same direction.
            Vector2Int previousDirection =
                current -
                path[path.Count - 2];

            Vector2Int chosen;

            if (candidates.Contains(previousDirection) &&
                Random.value < 0.65f)
            {
                chosen = previousDirection;
            }
            else
            {
                chosen =
                    candidates[
                        Random.Range(
                            0,
                            candidates.Count
                        )
                    ];
            }

            // Final safety check.
            Vector2Int newPosition =
                current + chosen;

            if (!InBounds(
                newPosition,
                width,
                height))
            {
                break;
            }

            if (occupied[
                newPosition.x,
                newPosition.y])
            {
                break;
            }

            if (path.Contains(newPosition))
                break;

            current = newPosition;

            path.Add(current);
        }

        return path;
    }

    // ================================================================
    // FREE MOVE
    // ================================================================

    private static bool HasFreeMove(
        List<PathData> paths,
        int[,] cellPathId,
        int width,
        int height)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            if (CanExit(
                i,
                paths,
                cellPathId,
                width,
                height,
                null))
            {
                return true;
            }
        }

        return false;
    }

    // ================================================================
    // BLOCKING
    // ================================================================

    private static bool HasBlockingRelationship(
        List<PathData> paths,
        int[,] cellPathId,
        int width,
        int height)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            if (!CanExit(
                i,
                paths,
                cellPathId,
                width,
                height,
                null))
            {
                return true;
            }
        }

        return false;
    }

    // ================================================================
    // DIRECTION VARIETY
    // ================================================================

    private static bool HasDirectionVariety(
        List<PathData> paths)
    {
        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;

        for (int i = 0; i < paths.Count; i++)
        {
            PathData path = paths[i];

            if (path.cells.Count < 2)
                continue;

            Vector2Int direction =
                path.Head -
                path.cells[path.cells.Count - 2];

            if (direction == Vector2Int.up)
                up = true;

            if (direction == Vector2Int.down)
                down = true;

            if (direction == Vector2Int.left)
                left = true;

            if (direction == Vector2Int.right)
                right = true;
        }

        int count = 0;

        if (up) count++;
        if (down) count++;
        if (left) count++;
        if (right) count++;

        return count >= 2;
    }

    // ================================================================
    // CAN EXIT
    // ================================================================

    private static bool CanExit(
        int pathId,
        List<PathData> paths,
        int[,] cellPathId,
        int width,
        int height,
        bool[] cleared)
    {
        PathData path =
            paths[pathId];

        if (path.cells.Count < 2)
            return true;

        Vector2Int head =
            path.Head;

        Vector2Int previous =
            path.cells[path.cells.Count - 2];

        Vector2Int direction =
            head - previous;

        Vector2Int check =
            head + direction;

        while (InBounds(
            check,
            width,
            height))
        {
            int otherPathId =
                cellPathId[
                    check.x,
                    check.y
                ];

            // Empty.
            if (otherPathId == -1)
            {
                check += direction;
                continue;
            }

            // Same path.
            if (otherPathId == pathId)
            {
                check += direction;
                continue;
            }

            // Cleared.
            if (cleared != null &&
                cleared[otherPathId])
            {
                check += direction;
                continue;
            }

            // Active path blocking us.
            return false;
        }

        // Reached outside the board.
        return true;
    }

    // ================================================================
    // SOLVABILITY
    // ================================================================

    private static bool IsSolvable(
        List<PathData> paths,
        int[,] cellPathId,
        int width,
        int height)
    {
        bool[] cleared =
            new bool[paths.Count];

        int remaining =
            paths.Count;

        while (remaining > 0)
        {
            bool foundMove = false;

            for (int i = 0;
                 i < paths.Count;
                 i++)
            {
                if (cleared[i])
                    continue;

                if (CanExit(
                    i,
                    paths,
                    cellPathId,
                    width,
                    height,
                    cleared))
                {
                    cleared[i] = true;
                    remaining--;
                    foundMove = true;
                    break;
                }
            }

            if (!foundMove)
                return false;
        }

        return true;
    }

    // ================================================================
    // SAFE FALLBACK
    // ================================================================

    private static List<PathData> CreateFallback(
        int width,
        int height,
        int maxPathLength,
        out int[,] cellPathId)
    {
        cellPathId =
            CreateEmptyGrid(
                width,
                height
            );

        bool[,] occupied =
            new bool[width, height];

        List<PathData> paths =
            new List<PathData>();

        int target =
            Mathf.Clamp(
                Mathf.RoundToInt(
                    width * height / 5f
                ),
                4,
                10
            );

        // ------------------------------------------------------------
        // Safe deterministic-style random fallback.
        // ------------------------------------------------------------

        for (int attempt = 0;
             attempt < 1000 &&
             paths.Count < target;
             attempt++)
        {
            Vector2Int head =
                GetRandomFreeCell(
                    occupied,
                    width,
                    height
                );

            if (!InBounds(
                head,
                width,
                height))
            {
                break;
            }

            List<Vector2Int> directions =
                GetValidHeadDirections(
                    head,
                    width,
                    height
                );

            if (directions.Count == 0)
                continue;

            Shuffle(directions);

            Vector2Int direction =
                directions[0];

            List<Vector2Int> cells =
                BuildPathBackwards(
                    head,
                    direction,
                    occupied,
                    width,
                    height,
                    Mathf.Min(
                        maxPathLength,
                        3
                    )
                );

            if (cells == null ||
                cells.Count < 2)
            {
                continue;
            }

            bool valid = true;

            for (int i = 0;
                 i < cells.Count;
                 i++)
            {
                Vector2Int cell =
                    cells[i];

                if (!InBounds(
                    cell,
                    width,
                    height))
                {
                    valid = false;
                    break;
                }

                if (occupied[
                    cell.x,
                    cell.y])
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            cells.Reverse();

            PathData path =
                new PathData();

            for (int i = 0;
                 i < cells.Count;
                 i++)
            {
                Vector2Int cell =
                    cells[i];

                path.cells.Add(cell);

                occupied[
                    cell.x,
                    cell.y
                ] = true;

                cellPathId[
                    cell.x,
                    cell.y
                ] = paths.Count;
            }

            paths.Add(path);
        }

        return paths;
    }

    // ================================================================
    // EMPTY GRID
    // ================================================================

    private static int[,] CreateEmptyGrid(
        int width,
        int height)
    {
        int[,] grid =
            new int[width, height];

        for (int x = 0;
             x < width;
             x++)
        {
            for (int y = 0;
                 y < height;
                 y++)
            {
                grid[x, y] = -1;
            }
        }

        return grid;
    }

    // ================================================================
    // BOUNDS
    // ================================================================

    private static bool InBounds(
        Vector2Int position,
        int width,
        int height)
    {
        return
            position.x >= 0 &&
            position.x < width &&
            position.y >= 0 &&
            position.y < height;
    }

    // ================================================================
    // SHUFFLE
    // ================================================================

    private static void Shuffle(
        List<Vector2Int> list)
    {
        for (int i = list.Count - 1;
             i > 0;
             i--)
        {
            int j =
                Random.Range(
                    0,
                    i + 1
                );

            Vector2Int temp =
                list[i];

            list[i] =
                list[j];

            list[j] =
                temp;
        }
    }
}