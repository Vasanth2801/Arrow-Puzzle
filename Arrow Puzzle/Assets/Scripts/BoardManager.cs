using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Grid Settings")]
    public int baseWidth = 5;
    public int baseHeight = 6;
    public int maxWidth = 8;
    public int maxHeight = 10;
    public float cellSize = 1f;
    public float cellGap = 0.08f; // small gap so you can see individual cells

    [Header("Prefab")]
    public GameObject cellPrefab;
    public GameObject arrowPrefab; // small triangle shown on the head cell only

    [Header("Gameplay")]
    public int maxHearts = 3;

    public int CurrentLevel { get; private set; } = 1;
    public int MaxHearts => maxHearts;

    //("Events")
    public event Action<int> onHeartsChanged;
    public event Action<int> onLevelChanged;
    public event Action onLevelComplete;
    public event Action onGameOver;

    private int width, height;
    private List<PathData> paths;
    private List<Vector2> pathDirections; // one exit direction per path, same index as 'paths'
    private GameObject[,] cellObjects;
    private int clearedCount;
    private int heartsRemaining;
    private bool inputLocked;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartLevel(CurrentLevel);
        BuildBoard();
        FitCameraToGrid();
    }

    private void StartLevel(int level)
    {
        StopAllCoroutines();
        ClearBoardObjects();

        width = Mathf.Min(baseWidth + level / 3, maxWidth);
        height = Mathf.Min(baseHeight + level / 4, maxHeight);

        heartsRemaining = maxHearts;
        clearedCount = 0;
        inputLocked = false;

        BuildBoard();
        FitCameraToGrid();

        onHeartsChanged?.Invoke(heartsRemaining);
        onLevelChanged?.Invoke(CurrentLevel);
    }

    private void ClearBoardObjects()
    {
        if(cellObjects == null)
        {
            return;
        }

        foreach(var go in cellObjects)
        {
            if(go != null)
            {
                Destroy(go);
            }
        }
    }

    private void BuildBoard()
    {
        paths = LevelGenerator.Generate(baseWidth, baseHeight, out _);
        pathDirections = new List<Vector2>();
        cellObjects = new GameObject[baseWidth, baseHeight];

        int total = paths.Count;
        List<float> hues = new List<float>();

        for(int i = 0; i < total; i++)
        {
            hues.Add((float)i / total);
        }

       ShuffleFloats(hues);

        for (int i = 0; i < paths.Count; i++)
        {
            Color pathColor = Color.HSVToRGB(hues[i], 0.65f, 0.9f);
            Vector2Int head = paths[i].Head;
            Vector2 dir = ComputeHeadDirection(paths[i]);
            pathDirections.Add(dir);

            foreach (var c in paths[i].cells)
            {
                GameObject go = Instantiate(cellPrefab, transform);
                go.name = $"Cell_{c.x}_{c.y}_Path{i}";
                go.transform.localPosition = new Vector3(c.x * cellSize, c.y * cellSize, 0f);
                go.transform.localScale = Vector3.one * (cellSize - cellGap);

                bool isHead = c == head;

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                sr.color = pathColor;

                GridCell cell = go.GetComponent<GridCell>();
                cell.pathId = i;
                cell.coord = c;
                cell.isHead = isHead;

                if(isHead && arrowPrefab != null)
                {
                    GameObject arrow = Instantiate(arrowPrefab, go.transform);
                    arrow.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                    arrow.transform.localScale = Vector3.one * 0.6f;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    arrow.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                }

                cellObjects[c.x, c.y] = go;
            }
        }
    }

    private Vector2 ComputeHeadDirection(PathData path)
    {
        if (path.cells.Count >= 2)
        {
            Vector2Int prev = path.cells[path.cells.Count - 2];
            Vector2Int head = path.Head;
            return new Vector2(head.x - prev.x, head.y - prev.y);
        }

        Vector2Int c = path.Head;
        float distLeft = c.x, distRight = width -1 - c.x, distDown = c.y, distUp = height - 1 - c.y;
        float min = Mathf.Min(Mathf.Min(distLeft, distRight), Mathf.Min(distDown, distUp));

        if(min == distLeft)
        {
            return Vector2.left;
        }

        if(min == distRight)
        {
            return Vector2.right;
        }

        if(min == distDown)
        {
           return Vector2.down;    
        }

        return Vector2.up;
    }

    private void FitCameraToGrid()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.orthographic = true;

        float gridWidth = baseWidth * cellSize;
        float gridHeight = baseHeight * cellSize;

        cam.transform.position = new Vector3((gridWidth - cellSize)/2f, (gridWidth - cellSize) / 2f, -10f);

        float verticalSize = gridHeight / 2f + 0.5f;
        float horizontalSize = (gridWidth / 2f + 0.5f) / cam.aspect;
        cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    public void OnCellTapped(GridCell cell)
    {
       if(inputLocked)
        {
            return;
        }

       PathData path = paths[cell.pathId];
        if(path.cleared)
        {
            return;
        }

        if(cell.isHead)
        {
            StartCoroutine(SlideOutAndClear(cell.pathId));
        }
        else
        {
            StartCoroutine(FlashWrong(cell));
        }
    }

    private IEnumerator SlideOutAndClear(int pathId)
    {
        inputLocked = true;

        PathData path = paths[pathId];
        path.cleared = true;
        Vector2 dir = pathDirections[pathId].normalized;

        List<Transform> movers = new List<Transform>();
        List<Vector3> starts = new List<Vector3>();

        foreach(var c in path.cells)
        {
            GameObject go = cellObjects[c.x, c.y];
            if (go != null)
            {
                movers.Add(go.transform);
                starts.Add(go.transform.localPosition);
            }
        }

        float distance = width + height;
        Vector3 offset = new Vector3(dir.x, dir.y, 0f) * distance;
        float duration = 0.25f;
        float t = 0f;

        while(t < duration )
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            for(int i =0; i < starts.Count; i++)
            {
                movers[i].localPosition = starts[i] + offset * p;
            }

            yield return null;
        }

        foreach(var c in path.cells)
        {
            GameObject go = cellObjects[c.x, c.y];
            if (go != null)
            {
                Destroy(go);
                cellObjects[c.x, c.y] = null;
            }
        }

        clearedCount++;

        if(clearedCount >= paths.Count)
        {
            inputLocked = true;
            onLevelComplete?.Invoke();
        }
        else
        {
            inputLocked = false;
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
        Color original = sr.color;
        sr.color = Color.red;

        heartsRemaining--;
        onHeartsChanged?.Invoke(heartsRemaining);

        yield return new WaitForSeconds(0.15f);

        if(sr != null)
        {
            sr.color = original;
        }

        if(heartsRemaining <= 0)
        {
            inputLocked = true;
            onGameOver?.Invoke();
        }
    }

    public void ShowHint()
    {
        if(inputLocked)
        {
            return;
        }

        foreach(var path in paths)
        {
            if(path.cleared)
            {
                continue;
            }

            Vector2Int head = path.Head;
            GameObject go = cellObjects[head.x, head.y];

            if (go != null)
            {
                StartCoroutine(PulseHint(go));  
            }

            break;
        }
    }

    private IEnumerator PulseHint(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

        if(sr == null)
        {
            yield break;
        }

        Color original = sr.color;

        for(int i = 0;  i< 3; i++)
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.15f);

            if(sr == null)
            {
                yield break;
            }

            sr.color = original;

            yield return new WaitForSeconds(0.15f);
        }
    }

    public void NextLevel()
    {
        CurrentLevel++;
        StartLevel(CurrentLevel);
    }

    public void RestartLevel()
    {
        StartLevel(CurrentLevel);
    }

    private void ShuffleFloats(List<float> list)
    {
        for(int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}