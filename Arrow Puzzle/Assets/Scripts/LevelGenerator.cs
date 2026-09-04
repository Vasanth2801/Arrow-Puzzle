using System.Collections.Generic;
using UnityEngine;


public class PathData
{
    public List<Vector2Int> cells = new List<Vector2Int>();
    public bool cleared = false;

    public Vector2Int Head => cells[cells.Count - 1];
}

/// <summary>
/// Procedurally fills a width x height grid with non-overlapping, self-avoiding
/// paths (straight segments + 90-degree turns only) so that EVERY cell belongs
/// to exactly one path.
/// </summary>
public static class LevelGenerator
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    /// <summary>
    /// Generates a full-grid partition into paths.
    /// </summary>
    /// <param name="width">Grid width in cells.</param>
    /// <param name="height">Grid height in cells.</param>
    /// <param name="maxPathLength">
    /// Hard cap on how many cells a single path may occupy. Keeping this small
    /// (5-7) is what keeps the board readable — without a cap, random walks
    /// occasionally sprawl across a third of the board and become impossible
    /// to visually trace.
    /// </param>
    /// <param name="cellPathId">Output: for each [x,y], which path index owns that cell.</param>
    public static List<PathData> Generate(int width, int height, int maxPathLength, out int[,] cellPathId)
    {
        cellPathId = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cellPathId[x, y] = -1;

        bool[,] visited = new bool[width, height];
        List<PathData> paths = new List<PathData>();

        List<Vector2Int> allCells = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                allCells.Add(new Vector2Int(x, y));

        Shuffle(allCells);

        foreach (var startCell in allCells)
        {
            if (visited[startCell.x, startCell.y]) continue;

            PathData path = new PathData();
            Vector2Int current = startCell;
            visited[current.x, current.y] = true;
            path.cells.Add(current);
            Vector2Int? lastDir = null;

            while (path.cells.Count < maxPathLength)
            {
                List<Vector2Int> candidateDirs = new List<Vector2Int>();
                foreach (var dir in Directions)
                {
                    Vector2Int next = current + dir;
                    if (InBounds(next, width, height) && !visited[next.x, next.y])
                        candidateDirs.Add(dir);
                }

                if (candidateDirs.Count == 0) break; // dead end -> this cell is the head

                Vector2Int chosenDir;
                // Bias toward continuing straight so paths look like winding pipes
                // instead of jittering back and forth every single cell.
                if (lastDir.HasValue && candidateDirs.Contains(lastDir.Value) && Random.value < 0.65f)
                    chosenDir = lastDir.Value;
                else
                    chosenDir = candidateDirs[Random.Range(0, candidateDirs.Count)];

                current += chosenDir;
                visited[current.x, current.y] = true;
                path.cells.Add(current);
                lastDir = chosenDir;
            }

            int pathId = paths.Count;
            foreach (var c in path.cells)
                cellPathId[c.x, c.y] = pathId;

            paths.Add(path);
        }

        return paths;
    }

    private static bool InBounds(Vector2Int p, int w, int h) =>
        p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;

    private static void Shuffle(List<Vector2Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

    }
}