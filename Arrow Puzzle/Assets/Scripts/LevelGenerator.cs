using System.Collections.Generic;
using UnityEngine;


public class PathData
{
    public List<Vector2Int> cells = new List<Vector2Int>();
    public bool cleared = false;

    public Vector2Int Head => cells[cells.Count - 1];
}

public class LevelGenerator
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public static List<PathData> Generate(int width, int height, out int[,] cellPathId)
    {
        cellPathId = new int[width, height];
        for(int x =0; x < width; x++)
        {
            for(int y =0; y < height; y++)
            {
                cellPathId[x, y] = -1;
            }
        }

        bool[,] visited = new bool[width, height];
        List<PathData> paths = new List<PathData>();

        List<Vector2Int> allCells = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for(int y =0; y < height;  y++)
            {
                allCells.Add(new Vector2Int(x, y));
            }
        }

        Shuffle(allCells);

        foreach(var StartCell in allCells)
        {
            if (visited[StartCell.x, StartCell.y])
            {
                continue;
            }

            PathData path = new PathData();
            Vector2Int current = StartCell;
            visited[current.x, current.y] = true;
            path.cells.Add(current);
            Vector2Int? lastDir = null;

            while(true)
            {
                List<Vector2Int> canditateDirs = new List<Vector2Int>();
                foreach(var dir in Directions)
                {
                    Vector2Int next = current + dir;
                    if(InBounds(next, width, height) && !visited[next.x, next.y])
                    {
                        canditateDirs.Add(dir);
                    }
                }

                if(canditateDirs.Count == 0)
                {
                    break;
                }

                Vector2Int chosenDir;

                if(lastDir.HasValue && canditateDirs.Contains(lastDir.Value) && Random.value < 0.65f)
                {
                    chosenDir = lastDir.Value;
                }
                else
                {
                    chosenDir = canditateDirs[Random.Range(0, canditateDirs.Count)];
                }

                current += chosenDir;
                visited[current.x, current.y] = true;
                path.cells.Add(current);
                lastDir = chosenDir;
            }

            int pathId = paths.Count;
            foreach (var c in path.cells)
            {
                cellPathId[c.x, c.y] = pathId;
            }

            paths.Add(path);
        }
        return paths;
    }

    private static bool InBounds(Vector2Int p, int w, int h) => p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;

    private static void Shuffle(List<Vector2Int> list)
    {
        for(int i = list.Count -1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}