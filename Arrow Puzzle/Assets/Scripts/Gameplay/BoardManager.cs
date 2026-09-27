using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Board")]
    [SerializeField] private int baseWidth = 8;
    [SerializeField] private int baseHeight = 12;
    [SerializeField] private int maxWidth = 12;
    [SerializeField] private int maxHeight = 18;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float visualSpacing = 0.1f;
    [SerializeField] private float pathWidth = 0.16f;
    [SerializeField] private float arrowSize = 0.34f;

    [Header("Gameplay")]
    [SerializeField] private int maxHearts = 3;
    [SerializeField] private float clearDuration = 0.8f;

    [Header("Colors")]
    [SerializeField] private Color pathColor = new Color(0.55f, 0.63f, 0.76f);
    [SerializeField] private Color arrowColor = new Color(0.55f, 0.63f, 0.76f);
    [SerializeField] private Color activeColor = new Color(1f, 0.67f, 0.12f);

    public int CurrentLevel { get; private set; } = 1;
    public int Hearts { get; private set; }

    public System.Action<int> OnHeartsChanged;
    public System.Action<int> OnLevelChanged;
    public System.Action OnLevelComplete;
    public System.Action OnGameOver;

    private List<PathData> paths = new List<PathData>();
    private List<GameObject> pathObjects = new List<GameObject>();
    private List<GameObject> arrowObjects = new List<GameObject>();

    private int[,] cellPathId;
    private int clearedCount;
    private bool inputLocked;
    private Material lineMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CurrentLevel = GameProgress.Instance != null
            ? GameProgress.Instance.GetLevel()
            : 1;

        StartLevel(CurrentLevel);
    }

    private void Update()
    {
        HandlePathBodyClick();
    }

    private void HandlePathBodyClick()
    {
        if (inputLocked)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Camera cam = Camera.main;

        if (cam == null)
            return;

        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 mousePosition =
            new Vector2(mouseWorld.x, mouseWorld.y);

        Collider2D hit =
            Physics2D.OverlapPoint(mousePosition);

        if (hit == null)
            return;

        GameObject clickedObject = hit.gameObject;

        string objectName = clickedObject.name;

        if (!objectName.StartsWith("PathClickArea_"))
            return;

        string idText =
            objectName.Substring("PathClickArea_".Length);

        if (int.TryParse(idText, out int pathId))
        {
            TryMovePath(pathId);
        }
    }

    public void StartLevel(int level)
    {
        StopAllCoroutines();
        ClearBoard();

        CurrentLevel = Mathf.Max(1, level);
        Hearts = maxHearts;
        clearedCount = 0;
        inputLocked = false;

        int width =
            Mathf.Min(
                baseWidth + CurrentLevel / 250,
                maxWidth
            );

        int height =
            Mathf.Min(
                baseHeight + CurrentLevel / 180,
                maxHeight
            );

        int seed =
            CurrentLevel * 100003 + 7919;

        paths = LevelGenerator.Generate(
            width,
            height,
            CurrentLevel,
            out cellPathId
        );

        BuildVisuals(width, height);

        OnHeartsChanged?.Invoke(Hearts);
        OnLevelChanged?.Invoke(CurrentLevel);
    }

    private void BuildVisuals(int width, int height)
    {
        EnsureLineMaterial();

        Vector2 origin = new Vector2(
            -(width - 1) * cellSize * visualSpacing * 0.5f,
            -(height - 1) * cellSize * visualSpacing * 0.5f
        );

        for (int i = 0; i < paths.Count; i++)
        {
            PathData path = paths[i];

            GameObject container =
                new GameObject("Path_" + i);

            container.transform.SetParent(transform);

            pathObjects.Add(container);

            LineRenderer line =
                container.AddComponent<LineRenderer>();

            line.useWorldSpace = false;
            line.positionCount = path.cells.Count;

            line.startWidth = pathWidth;
            line.endWidth = pathWidth;

            line.numCapVertices = 6;
            line.numCornerVertices = 6;

            line.material = lineMaterial;

            line.startColor = pathColor;
            line.endColor = pathColor;

            line.sortingOrder = 2;

            for (int c = 0; c < path.cells.Count; c++)
            {
                Vector2 p =
                    GridToLocal(
                        path.cells[c],
                        origin
                    );

                line.SetPosition(
                    c,
                    new Vector3(
                        p.x,
                        p.y,
                        0f
                    )
                );
            }

            CreateArrow(
                path,
                origin,
                i
            );

            CreatePathClickAreas(
                path,
                origin,
                i,
                container
            );
        }

        FitCamera(width, height);
    }

    private void CreateArrow(
        PathData path,
        Vector2 origin,
        int id)
    {
        GameObject arrow =
            new GameObject(
                "ArrowHead_" + id
            );

        arrow.transform.SetParent(transform);

        Vector2 p =
            GridToLocal(
                path.Head,
                origin
            );

        arrow.transform.localPosition =
            new Vector3(
                p.x,
                p.y,
                -0.05f
            );

        arrow.transform.localScale =
            Vector3.one * arrowSize;

        MeshFilter mf =
            arrow.AddComponent<MeshFilter>();

        MeshRenderer mr =
            arrow.AddComponent<MeshRenderer>();

        mr.material =
            CreateArrowMaterial(
                arrowColor
            );

        mr.sortingOrder = 5;

        Mesh mesh = new Mesh();

        mesh.vertices = new[]
        {
            new Vector3(0f, 0.72f, 0f),
            new Vector3(-0.48f, -0.38f, 0f),
            new Vector3(0f, -0.12f, 0f),
            new Vector3(0.48f, -0.38f, 0f)
        };

        mesh.triangles = new[]
        {
            0, 1, 2,
            0, 2, 3
        };

        mesh.RecalculateNormals();

        mf.mesh = mesh;

        PolygonCollider2D collider =
            arrow.AddComponent<PolygonCollider2D>();

        collider.points = new[]
        {
            new Vector2(0f, 0.72f),
            new Vector2(-0.48f, -0.38f),
            new Vector2(0f, -0.12f),
            new Vector2(0.48f, -0.38f)
        };

        ArrowHead head =
            arrow.AddComponent<ArrowHead>();

        head.Setup(
            this,
            id,
            path.HeadDirection
        );

        arrowObjects.Add(arrow);
    }

    private void CreatePathClickAreas(
        PathData path,
        Vector2 origin,
        int pathId,
        GameObject container)
    {
        if (path == null ||
            path.cells == null ||
            path.cells.Count == 0)
        {
            return;
        }

        float clickThickness = 0.55f;

        // One-cell path
        if (path.cells.Count == 1)
        {
            Vector2 position =
                GridToLocal(
                    path.cells[0],
                    origin
                );

            GameObject clickArea =
                new GameObject(
                    "PathClickArea_" + pathId
                );

            clickArea.transform.SetParent(
                container.transform
            );

            clickArea.transform.localPosition =
                new Vector3(
                    position.x,
                    position.y,
                    -0.1f
                );

            BoxCollider2D collider =
                clickArea.AddComponent<BoxCollider2D>();

            collider.size =
                new Vector2(
                    clickThickness,
                    clickThickness
                );

            return;
        }

        // Multiple-cell path
        for (int i = 0;
             i < path.cells.Count - 1;
             i++)
        {
            Vector2 current =
                GridToLocal(
                    path.cells[i],
                    origin
                );

            Vector2 next =
                GridToLocal(
                    path.cells[i + 1],
                    origin
                );

            Vector2 difference =
                next - current;

            float length =
                difference.magnitude;

            Vector2 middle =
                (current + next) * 0.5f;

            GameObject clickArea =
                new GameObject(
                    "PathClickArea_" + pathId
                );

            clickArea.transform.SetParent(
                container.transform
            );

            clickArea.transform.localPosition =
                new Vector3(
                    middle.x,
                    middle.y,
                    -0.1f
                );

            BoxCollider2D collider =
                clickArea.AddComponent<BoxCollider2D>();

            if (Mathf.Abs(difference.x) >
                Mathf.Abs(difference.y))
            {
                // Horizontal segment
                collider.size =
                    new Vector2(
                        length + clickThickness,
                        clickThickness
                    );
            }
            else
            {
                // Vertical segment
                collider.size =
                    new Vector2(
                        clickThickness,
                        length + clickThickness
                    );
            }
        }
    }

    public void TryMovePath(int pathId)
    {
        if (inputLocked)
            return;

        if (pathId < 0 ||
            pathId >= paths.Count)
            return;

        if (paths[pathId].cleared)
            return;

        int blocker =
            GetBlockingPathId(pathId);

        if (blocker != -1)
        {
            LoseHeart();

            HighlightBlocked(pathId);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrong();

            return;
        }

        StartCoroutine(
            ClearPathRoutine(pathId)
        );
    }

    private int GetBlockingPathId(int pathId)
    {
        PathData path =
            paths[pathId];

        Vector2Int check =
            path.Head;

        Vector2Int dir =
            path.HeadDirection;

        while (true)
        {
            check += dir;

            if (check.x < 0 ||
                check.x >= cellPathId.GetLength(0) ||
                check.y < 0 ||
                check.y >= cellPathId.GetLength(1))
            {
                return -1;
            }

            int otherId =
                cellPathId[
                    check.x,
                    check.y
                ];

            if (otherId == -1 ||
                otherId == pathId)
            {
                continue;
            }

            if (otherId >= 0 &&
                otherId < paths.Count &&
                !paths[otherId].cleared)
            {
                // Only the other path's HEAD blocks.
                if (paths[otherId].Head == check)
                    return otherId;
            }
        }
    }

    private IEnumerator ClearPathRoutine(
        int pathId)
    {
        inputLocked = true;

        PathData path =
            paths[pathId];

        path.cleared = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClear();

        GameObject pathObject =
            pathObjects[pathId];

        GameObject arrowObject =
            arrowObjects[pathId];

        Vector2 dir =
            path.HeadDirection;

        Vector3 offset =
            new Vector3(
                dir.x,
                dir.y,
                0f
            ) *
            (
                Mathf.Max(
                    cellPathId.GetLength(0),
                    cellPathId.GetLength(1)
                ) + 2f
            );

        float t = 0f;

        Vector3 start =
            pathObject.transform.localPosition;

        while (t < clearDuration)
        {
            t += Time.deltaTime;

            float p =
                Mathf.Clamp01(
                    t / clearDuration
                );

            float eased =
                1f -
                Mathf.Pow(
                    1f - p,
                    3f
                );

            pathObject.transform.localPosition =
                start + offset * eased;

            if (arrowObject != null)
            {
                arrowObject.transform.localPosition =
                    arrowObject.transform.localPosition +
                    offset *
                    (
                        Time.deltaTime /
                        clearDuration
                    );
            }

            yield return null;
        }

        if (pathObject != null)
            Destroy(pathObject);

        if (arrowObject != null)
            Destroy(arrowObject);

        pathObjects[pathId] = null;
        arrowObjects[pathId] = null;

        clearedCount++;

        if (clearedCount >= paths.Count)
        {
            inputLocked = true;

            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.SaveLevel(
                    CurrentLevel + 1
                );
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayLevelComplete();

            OnLevelComplete?.Invoke();
        }
        else
        {
            inputLocked = false;
        }
    }

    private void LoseHeart()
    {
        Hearts =
            Mathf.Max(
                0,
                Hearts - 1
            );

        OnHeartsChanged?.Invoke(
            Hearts
        );

        if (Hearts <= 0)
        {
            inputLocked = true;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayGameOver();

            OnGameOver?.Invoke();
        }
    }

    public void RestartLevel()
    {
        StartLevel(
            CurrentLevel
        );
    }

    public void NextLevel()
    {
        int next =
            CurrentLevel + 1;

        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.SaveLevel(
                next
            );
        }

        StartLevel(next);
    }

    public void ShowHint()
    {
        if (inputLocked)
            return;

        for (int i = 0;
             i < paths.Count;
             i++)
        {
            if (!paths[i].cleared &&
                GetBlockingPathId(i) == -1)
            {
                HighlightPath(i);
                return;
            }
        }
    }

    private void HighlightBlocked(int id)
    {
        HighlightPath(id);
    }

    private void HighlightPath(int id)
    {
        if (id < 0 ||
            id >= pathObjects.Count)
        {
            return;
        }

        LineRenderer line =
            pathObjects[id] != null
            ? pathObjects[id]
                .GetComponent<LineRenderer>()
            : null;

        if (line == null)
            return;

        line.startColor =
            activeColor;

        line.endColor =
            activeColor;

        StartCoroutine(
            ReturnPathColor(line)
        );
    }

    private IEnumerator ReturnPathColor(
        LineRenderer line)
    {
        yield return new WaitForSeconds(
            0.16f
        );

        if (line != null)
        {
            line.startColor =
                pathColor;

            line.endColor =
                pathColor;
        }
    }

    private Vector2 GridToLocal(
        Vector2Int c,
        Vector2 origin)
    {
        return origin +
            new Vector2(
                c.x *
                cellSize *
                visualSpacing,

                c.y *
                cellSize *
                visualSpacing
            );
    }

    private void FitCamera(
        int width,
        int height)
    {
        Camera cam =
            Camera.main;

        if (cam == null)
            return;

        cam.orthographic = true;

        float w =
            width *
            cellSize *
            visualSpacing;

        float h =
            height *
            cellSize *
            visualSpacing;

        cam.transform.position =
            new Vector3(
                0f,
                0f,
                -10f
            );

        float vertical =
            h * 0.5f + 1.2f;

        float horizontal =
            (
                w * 0.5f + 0.6f
            ) /
            Mathf.Max(
                0.01f,
                cam.aspect
            );

        cam.orthographicSize =
            Mathf.Max(
                vertical,
                horizontal
            );
    }

    private void EnsureLineMaterial()
    {
        if (lineMaterial != null)
            return;

        Shader shader =
            Shader.Find(
                "Sprites/Default"
            );

        lineMaterial =
            new Material(shader);

        lineMaterial.name =
            "ArrowPuzzle_PathMaterial";
    }

    private Material CreateArrowMaterial(
        Color color)
    {
        Shader shader =
            Shader.Find(
                "Sprites/Default"
            );

        Material m =
            new Material(shader);

        m.color = color;

        return m;
    }

    private void ClearBoard()
    {
        for (int i = 0;
             i < pathObjects.Count;
             i++)
        {
            if (pathObjects[i] != null)
                Destroy(pathObjects[i]);
        }

        for (int i = 0;
             i < arrowObjects.Count;
             i++)
        {
            if (arrowObjects[i] != null)
                Destroy(arrowObjects[i]);
        }

        pathObjects.Clear();
        arrowObjects.Clear();
        paths.Clear();
    }
}