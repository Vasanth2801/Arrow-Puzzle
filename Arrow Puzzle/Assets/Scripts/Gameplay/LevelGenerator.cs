using System.Collections.Generic;
using UnityEngine;

public static class LevelGenerator
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // ============================================================
    // MAIN GENERATE
    // ============================================================

    public static List<PathData> Generate(
        int width,
        int height,
        int maxPathLength,
        out int[,] cellPathId)
    {
        width =
            Mathf.Max(1, width);

        height =
            Mathf.Max(1, height);

        maxPathLength =
            Mathf.Max(6, maxPathLength);

        cellPathId =
            CreateEmptyGrid(
                width,
                height
            );

        // 8 paths on the current 8x12 board.
        int targetPathCount =
            Mathf.Clamp(
                Mathf.RoundToInt(
                    width * height / 12f
                ),
                8,
                10
            );

        targetPathCount =
            Mathf.Min(
                targetPathCount,
                Mathf.Max(
                    1,
                    width * height / 6
                )
            );

        // Bounded generation.
        // We NEVER generate hundreds/thousands of levels here.
        const int maxAttempts = 50;

        for (int attempt = 0;
             attempt < maxAttempts;
             attempt++)
        {
            List<PathData> paths =
                TryGenerate(
                    width,
                    height,
                    maxPathLength,
                    targetPathCount,
                    out cellPathId
                );

            if (paths == null)
                continue;

            if (paths.Count !=
                targetPathCount)
            {
                continue;
            }

            // Every path must be a proper
            // multi-cell orthogonal path.
            if (!AllPathsValid(
                    paths,
                    width,
                    height))
            {
                continue;
            }

            // IMPORTANT:
            // No head may point into another
            // path's BODY.
            //
            // A head may point toward another HEAD.
            if (HasHeadIntoBody(
                    paths,
                    cellPathId,
                    width,
                    height))
            {
                continue;
            }

            // Reject directly opposing heads.
            if (HasOpposingHeads(paths))
            {
                continue;
            }

            // Need at least one move available.
            if (!HasFreeMove(
                    paths,
                    cellPathId,
                    width,
                    height
                ))
            {
                continue;
            }

            // We still want actual blocking.
            if (!HasBlockingRelationship(
                    paths,
                    cellPathId,
                    width,
                    height
                ))
            {
                continue;
            }

            // Need different directions.
            if (!HasDirectionVariety(
                    paths
                ))
            {
                continue;
            }

            // Final solvability check.
            if (!IsSolvable(
                    paths,
                    cellPathId,
                    width,
                    height
                ))
            {
                continue;
            }

            Debug.Log(
                "[GENERATOR] Valid puzzle generated: " +
                paths.Count +
                " paths | Board " +
                width +
                "x" +
                height +
                " | Attempt " +
                (attempt + 1)
            );

            return paths;
        }

        Debug.LogWarning(
            "[GENERATOR] Could not generate a valid " +
            "puzzle within the attempt limit. " +
            "Using safe fallback."
        );

        return CreateFallback(
            width,
            height,
            maxPathLength,
            out cellPathId
        );
    }

    // ============================================================
    // TRY GENERATE
    // ============================================================

    private static List<PathData> TryGenerate(
        int width,
        int height,
        int maxPathLength,
        int targetPathCount,
        out int[,] cellPathId)
    {
        cellPathId =
            CreateEmptyGrid(
                width,
                height
            );

        bool[,] occupied =
            new bool[
                width,
                height
            ];

        List<PathData> paths =
            new List<PathData>();

        // Enough attempts to find 8 good paths,
        // but still strictly bounded.
        int pathAttempts =
            Mathf.Clamp(
                width * height * 8,
                150,
                500
            );

        for (int attempt = 0;
             attempt < pathAttempts;
             attempt++)
        {
            if (paths.Count >=
                targetPathCount)
            {
                break;
            }

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

            List<Vector2Int>
                headDirections =
                GetValidHeadDirections(
                    head,
                    width,
                    height
                );

            if (headDirections.Count == 0)
                continue;

            Shuffle(
                headDirections
            );

            bool placed =
                false;

            for (int d = 0;
                 d < headDirections.Count;
                 d++)
            {
                Vector2Int headDirection =
                    headDirections[d];

                List<Vector2Int> cells =
                    BuildPathBackwards(
                        head,
                        headDirection,
                        occupied,
                        width,
                        height,
                        maxPathLength
                    );

                if (cells == null)
                    continue;

                // Every arrow needs a proper body.
                if (cells.Count < 5)
                    continue;

                if (!IsOrthogonalPath(
                        cells))
                {
                    continue;
                }

                // Reverse because the generator
                // builds backwards from the head.
                cells.Reverse();

                PathData path =
                    new PathData();

                for (int i = 0;
                     i < cells.Count;
                     i++)
                {
                    path.cells.Add(
                        cells[i]
                    );
                }

                if (!IsOrthogonalPath(
                        path.cells))
                {
                    continue;
                }

                bool valid =
                    true;

                for (int i = 0;
                     i < path.cells.Count;
                     i++)
                {
                    Vector2Int cell =
                        path.cells[i];

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

                int pathId =
                    paths.Count;

                for (int i = 0;
                     i < path.cells.Count;
                     i++)
                {
                    Vector2Int cell =
                        path.cells[i];

                    occupied[
                        cell.x,
                        cell.y
                    ] = true;

                    cellPathId[
                        cell.x,
                        cell.y
                    ] = pathId;
                }

                paths.Add(path);

                placed = true;

                break;
            }

            if (!placed)
                continue;
        }

        if (paths.Count !=
            targetPathCount)
        {
            return null;
        }

        // --------------------------------------------------------
        // FINAL BOARD VALIDATION
        // --------------------------------------------------------

        if (!AllPathsValid(
                paths,
                width,
                height))
        {
            return null;
        }

        if (HasHeadIntoBody(
                paths,
                cellPathId,
                width,
                height))
        {
            return null;
        }

        if (HasOpposingHeads(
                paths))
        {
            return null;
        }

        if (!IsSolvable(
                paths,
                cellPathId,
                width,
                height))
        {
            return null;
        }

        return paths;
    }

    // ============================================================
    // BUILD PATH
    //
    // Head is fixed.
    // Body grows backwards.
    //
    // Only:
    // UP
    // DOWN
    // LEFT
    // RIGHT
    //
    // Therefore every turn is automatically 90 degrees.
    // ============================================================

    private static List<Vector2Int>
        BuildPathBackwards(
            Vector2Int head,
            Vector2Int headDirection,
            bool[,] occupied,
            int width,
            int height,
            int maxPathLength)
    {
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

        Vector2Int current =
            head - headDirection;

        if (!InBounds(
                current,
                width,
                height))
        {
            return null;
        }

        if (occupied[
                current.x,
                current.y])
        {
            return null;
        }

        path.Add(current);

        Vector2Int previousDirection =
            current - head;

        while (path.Count <
               maxPathLength)
        {
            List<Vector2Int>
                candidates =
                new List<Vector2Int>();

            for (int i = 0;
                 i < Directions.Length;
                 i++)
            {
                Vector2Int next =
                    current +
                    Directions[i];

                if (!InBounds(
                        next,
                        width,
                        height))
                {
                    continue;
                }

                if (occupied[
                        next.x,
                        next.y])
                {
                    continue;
                }

                if (path.Contains(
                        next))
                {
                    continue;
                }

                candidates.Add(
                    Directions[i]
                );
            }

            if (candidates.Count == 0)
                break;

            Vector2Int chosen;

            // Prefer continuing straight.
            if (candidates.Contains(
                    previousDirection) &&
                Random.value < 0.65f)
            {
                chosen =
                    previousDirection;
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

            if (path.Contains(
                    newPosition))
            {
                break;
            }

            current =
                newPosition;

            path.Add(
                current
            );

            previousDirection =
                chosen;
        }

        if (path.Count < 5)
            return null;

        return path;
    }

    // ============================================================
    // HEAD INTO BODY
    //
    // THIS IS THE IMPORTANT FIX.
    //
    // From every head, follow its exit direction.
    //
    // If the first occupied cell belongs to:
    //
    //     another PATH BODY
    //
    // reject the entire board.
    //
    // If it belongs to:
    //
    //     another PATH HEAD
    //
    // that is allowed.
    //
    // This lets us create real blocking chains:
    //
    //     → → →
    //
    // while preventing:
    //
    //     → ───────── BODY
    // ============================================================

    private static bool HasHeadIntoBody(
        List<PathData> paths,
        int[,] cellPathId,
        int width,
        int height)
    {
        for (int i = 0;
             i < paths.Count;
             i++)
        {
            PathData path =
                paths[i];

            Vector2Int head =
                path.Head;

            Vector2Int direction =
                GetHeadDirection(
                    path
                );

            Vector2Int check =
                head + direction;

            while (InBounds(
                    check,
                    width,
                    height))
            {
                int otherId =
                    cellPathId[
                        check.x,
                        check.y
                    ];

                if (otherId == -1 ||
                    otherId == i)
                {
                    check += direction;
                    continue;
                }

                PathData other =
                    paths[otherId];

                // First occupied cell is
                // another HEAD = valid blocker.
                if (other.Head == check)
                {
                    break;
                }

                // First occupied cell is
                // another BODY = BAD.
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // OPPOSING HEADS
    //
    // Reject:
    //
    //      →     ←
    //
    // and:
    //
    //      ↑
    //      ↓
    //
    // Same-direction chains are allowed.
    // ============================================================

    private static bool HasOpposingHeads(
        List<PathData> paths)
    {
        for (int i = 0;
             i < paths.Count;
             i++)
        {
            PathData a =
                paths[i];

            Vector2Int aHead =
                a.Head;

            Vector2Int aDirection =
                GetHeadDirection(a);

            for (int j = i + 1;
                 j < paths.Count;
                 j++)
            {
                PathData b =
                    paths[j];

                Vector2Int bHead =
                    b.Head;

                Vector2Int bDirection =
                    GetHeadDirection(b);

                // Same row.
                if (aHead.y ==
                    bHead.y)
                {
                    if (aDirection ==
                            Vector2Int.right &&
                        bDirection ==
                            Vector2Int.left &&
                        aHead.x <
                        bHead.x)
                    {
                        return true;
                    }

                    if (aDirection ==
                            Vector2Int.left &&
                        bDirection ==
                            Vector2Int.right &&
                        aHead.x >
                        bHead.x)
                    {
                        return true;
                    }
                }

                // Same column.
                if (aHead.x ==
                    bHead.x)
                {
                    if (aDirection ==
                            Vector2Int.up &&
                        bDirection ==
                            Vector2Int.down &&
                        aHead.y <
                        bHead.y)
                    {
                        return true;
                    }

                    if (aDirection ==
                            Vector2Int.down &&
                        bDirection ==
                            Vector2Int.up &&
                        aHead.y >
                        bHead.y)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // ============================================================
    // HEAD DIRECTION
    // ============================================================

    private static Vector2Int
        GetHeadDirection(
            PathData path)
    {
        if (path.cells.Count < 2)
            return Vector2Int.up;

        return
            path.Head -
            path.cells[
                path.cells.Count - 2
            ];
    }

    // ============================================================
    // PATH VALIDATION
    // ============================================================

    private static bool AllPathsValid(
        List<PathData> paths,
        int width,
        int height)
    {
        for (int i = 0;
             i < paths.Count;
             i++)
        {
            PathData path =
                paths[i];

            if (path == null)
                return false;

            if (path.cells == null)
                return false;

            // NO one-cell filler paths.
            if (path.cells.Count < 5)
                return false;

            if (!IsOrthogonalPath(
                    path.cells))
            {
                return false;
            }

            for (int c = 0;
                 c < path.cells.Count;
                 c++)
            {
                if (!InBounds(
                        path.cells[c],
                        width,
                        height))
                {
                    return false;
                }
            }
        }

        return true;
    }

    // ============================================================
    // ORTHOGONAL CHECK
    // ============================================================

    private static bool IsOrthogonalPath(
        List<Vector2Int> cells)
    {
        if (cells == null ||
            cells.Count < 2)
        {
            return false;
        }

        for (int i = 1;
             i < cells.Count;
             i++)
        {
            Vector2Int difference =
                cells[i] -
                cells[i - 1];

            if (!IsCardinal(
                    difference))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCardinal(
        Vector2Int difference)
    {
        return
            (difference.x == 1 &&
             difference.y == 0) ||

            (difference.x == -1 &&
             difference.y == 0) ||

            (difference.x == 0 &&
             difference.y == 1) ||

            (difference.x == 0 &&
             difference.y == -1);
    }

    // ============================================================
    // FREE MOVE
    // ============================================================

    private static bool HasFreeMove(
        List<PathData> paths,
        int[,] cellPathId,
        int width,
        int height)
    {
        for (int i = 0;
             i < paths.Count;
             i++)
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

    // ============================================================
    // BLOCKING RELATIONSHIP
    // ============================================================

    private static bool
        HasBlockingRelationship(
            List<PathData> paths,
            int[,] cellPathId,
            int width,
            int height)
    {
        for (int i = 0;
             i < paths.Count;
             i++)
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

    // ============================================================
    // DIRECTION VARIETY
    // ============================================================

    private static bool HasDirectionVariety(
        List<PathData> paths)
    {
        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;

        for (int i = 0;
             i < paths.Count;
             i++)
        {
            Vector2Int direction =
                GetHeadDirection(
                    paths[i]
                );

            if (direction ==
                Vector2Int.up)
                up = true;

            if (direction ==
                Vector2Int.down)
                down = true;

            if (direction ==
                Vector2Int.left)
                left = true;

            if (direction ==
                Vector2Int.right)
                right = true;
        }

        int count = 0;

        if (up) count++;
        if (down) count++;
        if (left) count++;
        if (right) count++;

        return count >= 2;
    }

    // ============================================================
    // CAN EXIT
    //
    // Runtime rule:
    // ONLY ANOTHER PATH'S HEAD BLOCKS.
    // ============================================================

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

        Vector2Int head =
            path.Head;

        Vector2Int direction =
            GetHeadDirection(
                path
            );

        Vector2Int check =
            head + direction;

        while (InBounds(
                check,
                width,
                height))
        {
            int otherId =
                cellPathId[
                    check.x,
                    check.y
                ];

            if (otherId == -1 ||
                otherId == pathId)
            {
                check += direction;
                continue;
            }

            if (cleared != null &&
                cleared[otherId])
            {
                check += direction;
                continue;
            }

            // ONLY OTHER HEAD BLOCKS.
            if (paths[otherId].Head ==
                check)
            {
                return false;
            }

            // Body does NOT block runtime.
            check += direction;
        }

        return true;
    }

    // ============================================================
    // SOLVABILITY
    // ============================================================

    private static bool IsSolvable(
        List<PathData> paths,
        int[,] cellPathId,
        int width,
        int height)
    {
        bool[] cleared =
            new bool[
                paths.Count
            ];

        int remaining =
            paths.Count;

        while (remaining > 0)
        {
            bool foundMove =
                false;

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

    // ============================================================
    // FALLBACK
    //
    // This fallback also obeys all important rules.
    // ============================================================

    private static List<PathData>
        CreateFallback(
            int width,
            int height,
            int maxPathLength,
            out int[,] cellPathId)
    {
        // Try the normal generator again with
        // a smaller path count instead of creating
        // one-cell filler arrows.
        cellPathId =
            CreateEmptyGrid(
                width,
                height
            );

        bool[,] occupied =
            new bool[
                width,
                height
            ];

        List<PathData> paths =
            new List<PathData>();

        int target =
            Mathf.Clamp(
                Mathf.RoundToInt(
                    width * height / 12f
                ),
                6,
                8
            );

        int attempts = 300;

        for (int attempt = 0;
             attempt < attempts &&
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

            List<Vector2Int>
                directions =
                GetValidHeadDirections(
                    head,
                    width,
                    height
                );

            Shuffle(directions);

            for (int d = 0;
                 d < directions.Count;
                 d++)
            {
                List<Vector2Int> cells =
                    BuildPathBackwards(
                        head,
                        directions[d],
                        occupied,
                        width,
                        height,
                        maxPathLength
                    );

                if (cells == null)
                    continue;

                if (cells.Count < 5)
                    continue;

                cells.Reverse();

                PathData path =
                    new PathData();

                for (int i = 0;
                     i < cells.Count;
                     i++)
                {
                    path.cells.Add(
                        cells[i]
                    );
                }

                int pathId =
                    paths.Count;

                for (int i = 0;
                     i < path.cells.Count;
                     i++)
                {
                    Vector2Int cell =
                        path.cells[i];

                    occupied[
                        cell.x,
                        cell.y
                    ] = true;

                    cellPathId[
                        cell.x,
                        cell.y
                    ] = pathId;
                }

                paths.Add(path);

                break;
            }
        }

        // If fallback itself is invalid,
        // do not pretend it is valid.
        if (paths.Count ==
                target &&
            AllPathsValid(
                paths,
                width,
                height) &&
            !HasHeadIntoBody(
                paths,
                cellPathId,
                width,
                height) &&
            !HasOpposingHeads(paths) &&
            IsSolvable(
                paths,
                cellPathId,
                width,
                height))
        {
            Debug.Log(
                "[GENERATOR] Safe fallback generated: " +
                paths.Count +
                " paths"
            );

            return paths;
        }

        Debug.LogError(
            "[GENERATOR] Could not create a valid fallback."
        );

        return paths;
    }

    // ============================================================
    // EMPTY GRID
    // ============================================================

    private static int[,] CreateEmptyGrid(
        int width,
        int height)
    {
        int[,] grid =
            new int[
                width,
                height
            ];

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

    // ============================================================
    // RANDOM FREE CELL
    // ============================================================

    private static Vector2Int
        GetRandomFreeCell(
            bool[,] occupied,
            int width,
            int height)
    {
        for (int i = 0;
             i < 50;
             i++)
        {
            int x =
                Random.Range(
                    0,
                    width
                );

            int y =
                Random.Range(
                    0,
                    height
                );

            if (!occupied[x, y])
            {
                return new Vector2Int(
                    x,
                    y
                );
            }
        }

        for (int x = 0;
             x < width;
             x++)
        {
            for (int y = 0;
                 y < height;
                 y++)
            {
                if (!occupied[x, y])
                {
                    return new Vector2Int(
                        x,
                        y
                    );
                }
            }
        }

        return new Vector2Int(
            -1,
            -1
        );
    }

    // ============================================================
    // VALID HEAD DIRECTIONS
    // ============================================================

    private static List<Vector2Int>
        GetValidHeadDirections(
            Vector2Int head,
            int width,
            int height)
    {
        List<Vector2Int> result =
            new List<Vector2Int>();

        for (int i = 0;
             i < Directions.Length;
             i++)
        {
            Vector2Int direction =
                Directions[i];

            Vector2Int previous =
                head - direction;

            if (InBounds(
                    previous,
                    width,
                    height))
            {
                result.Add(
                    direction
                );
            }
        }

        return result;
    }

    // ============================================================
    // BOUNDS
    // ============================================================

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

    // ============================================================
    // SHUFFLE
    // ============================================================

    private static void Shuffle(
        List<Vector2Int> list)
    {
        for (int i =
                list.Count - 1;
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