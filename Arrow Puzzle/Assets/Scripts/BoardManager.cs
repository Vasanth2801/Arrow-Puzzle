using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Grid Settings")]
    public int width = 6;
    public int height = 8;
    public float cellSize = 1f;
    public float cellGap = 0.08f; // small gap so you can see individual cells

    [Header("Prefab")]
    public GameObject cellPrefab;

    private List<PathData> paths;
    private int[,] cellPathId;
    private GameObject[,] cellObjects;
    private int clearedCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildBoard();
        FitCameraToGrid();
    }

    private void BuildBoard()
    {
        paths = LevelGenerator.Generate(width, height, out cellPathId);
        cellObjects = new GameObject[width, height];
        for (int i = 0; i < paths.Count; i++)
        {
            Color pathColor = Random.ColorHSV(0f, 1f, 0.55f, 0.85f, 0.75f, 0.95f);
            Vector2Int head = paths[i].Head;
            foreach (var c in paths[i].cells)
            {
                GameObject go = Instantiate(cellPrefab, transform);
                go.name = $"Cell_{c.x}_{c.y}_Path{i}";
                go.transform.localPosition = new Vector3(c.x * cellSize, c.y * cellSize, 0f);
                go.transform.localScale = Vector3.one * (cellSize - cellGap);
                bool isHead = c == head;
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                sr.color = isHead ? Color.Lerp(pathColor, Color.white, 0.55f) : pathColor;
                GridCell cell = go.GetComponent<GridCell>();
                cell.pathId = i;
                cell.coord = c;
                cell.isHead = isHead;
                cellObjects[c.x, c.y] = go;
            }
        }
    }

    private void FitCameraToGrid()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.orthographic = true;

        float gridWidth = width * cellSize;
        float gridHeight = height * cellSize;

        cam.transform.position = new Vector3((gridWidth - cellSize)/2f, (gridWidth - cellSize) / 2f, -10f);

        float verticalSize = gridHeight / 2f + 0.5f;
        float horizontalSize = (gridWidth / 2f + 0.5f) / cam.aspect;
        cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    public void OnCellTapped(GridCell cell)
    {
        PathData path = paths[cell.pathId];

        if(path.cleared)
        {
            return;
        }

        if(cell.isHead)
        {
            ClearPath(cell.pathId);
        }
        else
        {
            StartCoroutine(FlashWrong(cell));
        }
    }

    private void ClearPath(int pathId)
    {
        PathData path = paths[pathId];
        path.cleared = true;

        foreach (var c in path.cells)
        {
            GameObject go = cellObjects[c.x, c.y];
            if (go != null)
            {
                Destroy(go);
                cellObjects[c.x, c.y] = null;
            }
        }

        clearedCount++;
        Debug.Log($"Path {pathId} cleared! ({clearedCount}/{paths.Count})");

        if(clearedCount >= paths.Count)
        {
            Debug.Log("LEVEL COMPLETE!");
        }
    }

    private IEnumerator FlashWrong(GridCell cell)
    {
        SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            yield break;
        }

        Color originalColor = sr.color;
        sr.color = Color.red;
        Debug.Log("Wrong cell tapped!");
        yield return new WaitForSeconds(0.2f);
        
        if(sr != null)
        {
            sr.color = originalColor;
        }
    }
}
