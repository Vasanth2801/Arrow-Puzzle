using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Grid Settings (grows slowly with level)")]
    public int baseWidth = 5;
    public int baseHeight = 6;
    public int maxWidth = 8;
    public int maxHeight = 10;
    public float cellSize = 1f;
    public float cellGap = 0.08f;

    [Header("Readability")]
    [Tooltip("Max cells a single path can occupy. Lower = shorter, easier to read paths.")]
    public int maxPathLength = 6;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject arrowPrefab;

    [Header("Gameplay")]
    public int maxHearts = 3;

    public int CurrentLevel { get; private set; } = 1;
    public int MaxHearts => maxHearts;

    public event Action<int> OnHeartsChanged;
    public event Action<int> OnLevelChanged;
    public event Action OnLevelComplete;
    public event Action OnGameOver;

    private int width, height;
    private int[,] cellPathId;
    private List<PathData> paths;
    private List<Vector2> pathDirections;
    private List<GameObject> pathContainers;
    private List<GameObject> headGameObjects;
    private int clearedCount;
    private int heartsRemaining;
    private bool inputLocked;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameProgress.Instance != null)
        {
            CurrentLevel = GameProgress.Instance.GetSavedLevel();
        }

        StartLevel(CurrentLevel);
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
        
        RestoreClearedPaths();

        FitCameraToGrid();

        OnHeartsChanged?.Invoke(heartsRemaining);
        OnLevelChanged?.Invoke(CurrentLevel);
    }

    private void ClearBoardObjects()
    {
        if (pathContainers == null)
        {
            return;
        }

        foreach (var go in pathContainers)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
    }

    private void BuildBoard()
    {
        UnityEngine.Random.State previousRandomState = UnityEngine.Random.state;

        UnityEngine.Random.InitState(CurrentLevel * 100003);

        paths = LevelGenerator.Generate(
            width,
            height,
            maxPathLength,
            out cellPathId
        );

        UnityEngine.Random.state = previousRandomState;

        pathDirections = new List<Vector2>();
        pathContainers = new List<GameObject>();
        headGameObjects = new List<GameObject>();

        for (int i = 0; i < paths.Count; i++)
        {
            GameObject container = new GameObject($"Path_{i}");

            container.transform.SetParent(transform,false);

            pathContainers.Add(container);

            PathData path = paths[i];

            Vector2 dir = ComputeHeadDirection(path);

            pathDirections.Add(dir);

            Vector2Int head = path.Head;

            GameObject headGO = Instantiate(cellPrefab, container.transform,false);

            headGO.name = $"Head_{head.x}_{head.y}_Path{i}";

            headGO.transform.localPosition = new Vector3(head.x * cellSize, head.y * cellSize, 0f);

            headGO.transform.localScale = Vector3.one * cellSize;

            Transform visual = headGO.transform.Find("Visual");

            if (visual != null)
            {
                SpriteRenderer cellSprite = visual.GetComponent<SpriteRenderer>();

                if (cellSprite != null)
                {
                    cellSprite.enabled = false;
                }
            }

            GridCell cell = headGO.GetComponent<GridCell>();

            if (cell != null)
            {
                cell.pathId = i;
                cell.coord = head;
                cell.isHead = true;
            }

            if (arrowPrefab != null && visual != null)
            {
                GameObject arrow = Instantiate(arrowPrefab, visual, false);

                arrow.transform.localPosition = Vector3.zero;

                arrow.transform.localScale = Vector3.one * 0.75f;

                float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg - 90f;

                arrow.transform.localRotation = Quaternion.Euler( 0f, 0f, angle);

                SpriteRenderer arrowSr = arrow.GetComponent<SpriteRenderer>();

                if (arrowSr != null)
                {
                    arrowSr.sortingOrder = 2;
                }
            }

            headGameObjects.Add(headGO);
        }

        Debug.Log( $"[BUILD] {paths.Count} paths created, " + $"{headGameObjects.FindAll(h => h != null).Count} have a valid head object");
    }

    private void RestoreClearedPaths()
    {
        if(GameProgress.Instance == null)
        {
            return;
        }

        string saved = GameProgress.Instance.GetClearedPaths(CurrentLevel);

        if(string.IsNullOrEmpty(saved))
        {
            return;
        }

        string[] parts = saved.Split(',');

        foreach(string part in parts)
        {
            if(!int.TryParse(part, out int pathid))
            {
                continue;
            }

            if(pathid < 0 || pathid >= paths.Count)
            {
                continue;
            }

            if (paths[pathid].cleared)
            {
                continue;
            }

            paths[pathid].cleared = true;

            if(pathid < headGameObjects.Count && headGameObjects[pathid] != null)
            {
                Destroy(headGameObjects[pathid]);
                headGameObjects[pathid] = null;
            }

            if(pathid < pathContainers.Count && pathContainers[pathid] != null)
            {
                Destroy(pathContainers[pathid]);
                pathContainers[pathid] = null;
            }

            clearedCount++;
        }

        Debug.Log($"[Load] Restored {clearedCount} clearedPaths" + $"for level {CurrentLevel}");
    }

    private Vector2 ComputeHeadDirection(PathData path)
    {
        if (path.cells.Count >= 2)
        {
            Vector2Int prev =path.cells[path.cells.Count - 2];

            Vector2Int head = path.Head;

            return new Vector2(head.x - prev.x,head.y - prev.y);
        }

        Vector2Int c = path.Head;

        float distLeft = c.x;
        float distRight = width - 1 - c.x;

        float distDown = c.y;
        float distUp = height - 1 - c.y;

        float min = Mathf.Min(Mathf.Min(distLeft,distRight), Mathf.Min(distDown,distUp));

        if (min == distLeft)
        {
            return Vector2.left;
        }
        else if (min == distRight)
        {
            return Vector2.right;
        }
        else if (min == distDown)
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

        float gridWidth =  width * cellSize;

        float gridHeight =  height * cellSize;

        cam.transform.position =  new Vector3((gridWidth - cellSize) / 2f,(gridHeight - cellSize) / 2f,-10f);

        float verticalSize = gridHeight / 2f + 0.5f;

        float horizontalSize = (gridWidth / 2f + 0.5f) / cam.aspect;

        cam.orthographicSize = Mathf.Max(verticalSize,horizontalSize);
    }

    public void OnCellTapped(GridCell cell)
    {
        Debug.Log($"[TAP] coord={cell.coord} " + $"pathId={cell.pathId} " + $"isHead={cell.isHead} " +$"inputLocked={inputLocked}");

        if (inputLocked)
        {
            return;
        }

        PathData path = paths[cell.pathId];

        if (path.cleared)
        {
            return;
        }

        if (!cell.isHead)
        {
            StartCoroutine(FlashWrong(cell));

            return;
        }

        int blockedPathId = GetBlockingPathId(cell.pathId);

        if (blockedPathId != -1)
        {
            Debug.Log($"Blocked Path {cell.pathId} " + $"Head {cell.coord} id blocked by Path {blockedPathId}" + $"Head {paths[blockedPathId].Head}");

            StartCoroutine(FlashWrong(cell));

            StartCoroutine(ShowBlockedFeedback(cell.pathId,blockedPathId));

            return;
        }

        StartCoroutine(SlideOutAndClear(cell.pathId));
    }

    private int GetBlockingPathId(int pathId)
    {
        if (pathId < 0 || pathId >= paths.Count)
        {
            return -1;
        }

        PathData path = paths[pathId];

        if (path == null || path.cells == null || path.cells.Count == 0)
        {
            return -1;
        }

        Vector2Int head = path.Head;

        Vector2 direction = ComputeHeadDirection(path);

        Vector2Int gridDirection = new Vector2Int(Mathf.RoundToInt(direction.x),Mathf.RoundToInt(direction.y));

        Vector2Int check = head + gridDirection;

        while (check.x >= 0 && check.x < width && check.y >= 0 && check.y < height)
        {
            int otherPathId = cellPathId[check.x,check.y];

            if (otherPathId == -1 || otherPathId == pathId)
            {
                check += gridDirection;
                continue;
            }

            PathData otherPath = paths[otherPathId];

            if (otherPath == null || otherPath.cleared)
            {
                check += gridDirection;
                continue;
            }

            if (otherPath.Head == check)
            {
                Debug.Log("Block Found");
                return otherPathId;
            }

            check += gridDirection;
        }

        return -1;
    }

    private IEnumerator ShowBlockedFeedback(int headPathId,int blockingPathId)
    {
        GameObject headGO = headGameObjects[headPathId];

        GameObject blockerGO = headGameObjects[blockingPathId];

        if (headGO != null)
        {
            StartCoroutine(FlashBlockedObject(headGO,Color.red));
        }

        if (blockerGO != null)
        {
            StartCoroutine(FlashBlockedObject(blockerGO,Color.yellow));
        }

        if (headGO != null)
        {
            yield return StartCoroutine(ShakeBlockedObject(headGO));
        }

        yield return null;
    }

    private IEnumerator FlashBlockedObject(GameObject go,Color flashColor)
    {
        if (go == null)
        {
            yield break;
        }

        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
        {
            yield break;
        }

        Color original = sr.color;

        sr.color = flashColor;

        yield return new WaitForSeconds(0.12f);

        if (sr != null)
        {
            sr.color = original;
        }
    }

    private IEnumerator ShakeBlockedObject(GameObject go)
    {
        if (go == null)
        {
            yield break;
        }

        Transform t = go.transform;

        Vector3 originalPosition = t.localPosition;

        float duration = 0.15f;
        float strength = 0.08f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x =
                UnityEngine.Random.Range(-strength,strength);

            float y =
                UnityEngine.Random.Range(-strength,strength);

            t.localPosition = originalPosition + new Vector3(x,y,0f);

            yield return null;
        }

        if (t != null)
        {
            t.localPosition = originalPosition;
        }
    }

    private bool CanPathExit(int pathId)
    {
        if (pathId < 0 || pathId >= paths.Count)
        {
            return false;
        }

        PathData path = paths[pathId];

        if (path == null || path.cells == null || path.cells.Count == 0)
        {
            return false;
        }

        Vector2Int head = path.Head;

        Vector2 direction = ComputeHeadDirection(path);

        Vector2Int gridDirection = new Vector2Int(Mathf.RoundToInt(direction.x),Mathf.RoundToInt(direction.y));

        Vector2Int check = head + gridDirection;

        while (check.x >= 0 && check.x < width && check.y >= 0 && check.y < height)
        {
            int otherPathId = cellPathId[check.x,check.y];

            if (otherPathId == -1)
            {
                check += gridDirection;
                continue;
            }

            if (otherPathId == pathId)
            {
                check += gridDirection;
                continue;
            }

            if (paths[otherPathId].cleared)
            {
                check += gridDirection;
                continue;
            }

            return false;
        }

        return true;
    }

    private IEnumerator SlideOutAndClear(int pathId)
    {
        inputLocked = true;

        PathData path = paths[pathId];

        path.cleared = true;

        Vector2 dir = pathDirections[pathId].normalized;

        Transform container = pathContainers[pathId].transform;

        float distance = width + height;

        Vector3 offset = new Vector3(dir.x,dir.y,0f) * distance;

        float duration = 1f;
        float t = 0f;

        Vector3 start = container.localPosition;

        while (t < duration)
        {
            t += Time.deltaTime;

            float p = Mathf.Clamp01(t / duration);

            container.localPosition = start + offset * p;

            yield return null;
        }

        Destroy(pathContainers[pathId]);

        pathContainers[pathId] = null;

        clearedCount++;

        if(GameProgress.Instance != null)
        {
            GameProgress.Instance.SaveClearedPath(CurrentLevel, pathId);
        }

        if (clearedCount >= paths.Count)
        {
            inputLocked = true;

            OnLevelComplete?.Invoke();
        }
        else
        {
            inputLocked = false;
        }
    }

    private IEnumerator FlashWrong(GridCell cell)
    {
        SpriteRenderer sr = cell.GetComponentInChildren<SpriteRenderer>();

        Color original = sr.color;

        sr.color = Color.red;

        heartsRemaining--;

        OnHeartsChanged?.Invoke(heartsRemaining);

        yield return new WaitForSeconds(0.15f);

        if (sr != null)
        {
            sr.color = original;
        }

        if (heartsRemaining <= 0)
        {
            inputLocked = true;

            OnGameOver?.Invoke();
        }
    }

    public void ShowHint()
    {
        if (inputLocked)
        {
            return;
        }

        for (int i = 0;i < paths.Count;i++)
        {
            if (paths[i].cleared)
            {
                continue;
            }

            if (CanPathExit(i))
            {
                GameObject go = headGameObjects[i];

                if (go != null)
                {
                    StartCoroutine(PulseHint(go));
                }

                break;
            }
        }
    }

    private IEnumerator PulseHint(GameObject go)
    {
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
        {
            yield break;
        }

        Color original = sr.color;

        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.white;

            yield return new WaitForSeconds(0.15f);

            if (sr == null)
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

        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.SaveLevel(CurrentLevel);
        }

        StartLevel(CurrentLevel);
    }

    public void RestartLevel()
    {
        StartLevel(CurrentLevel);
    }
}