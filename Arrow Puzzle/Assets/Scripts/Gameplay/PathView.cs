using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [RequireComponent(typeof(LineRenderer))]
    public sealed class PathView : MonoBehaviour
    {
        private LineRenderer line;
        private readonly List<BoxCollider2D> segmentColliders = new List<BoxCollider2D>();
        private readonly List<CircleCollider2D> cellColliders = new List<CircleCollider2D>();
        private GameObject arrowObject;
        private MeshFilter arrowMeshFilter;
        private MeshRenderer arrowRenderer;
        private Material runtimeMaterial;
        private GridPathData data;
        private GameConfigSO config;
        private BoardView board;
        private float cellSize;
        private float lineWidth;
        private Color normalColor;
        private Color highlightA;
        private Color highlightB;
        private Vector3 initialPosition;
        private bool inputEnabled;

        public int Id => data != null ? data.id : -1;
        public bool IsExtracted { get; private set; }
        public Vector2Int HeadDirection => data.headDirection;
        public GridPathData Data => data;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.numCapVertices = 6;
            line.numCornerVertices = 6;
            line.alignment = LineAlignment.TransformZ;
            runtimeMaterial = new Material(Shader.Find("Sprites/Default"));
            line.material = runtimeMaterial;

            arrowObject = new GameObject("ArrowHead");
            arrowObject.transform.SetParent(transform, false);
            arrowMeshFilter = arrowObject.AddComponent<MeshFilter>();
            arrowRenderer = arrowObject.AddComponent<MeshRenderer>();
            arrowRenderer.material = runtimeMaterial;
        }

        public void Initialize(GridPathData path, BoardView owner, GameConfigSO gameConfig, float worldCellSize,
            Color normal, Color highlightStart, Color highlightEnd)
        {
            data = path;
            board = owner;
            config = gameConfig;
            cellSize = worldCellSize;
            lineWidth = cellSize * config.lineWidthAsCell;
            normalColor = normal;
            highlightA = highlightStart;
            highlightB = highlightEnd;
            IsExtracted = false;
            inputEnabled = true;
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
            BuildVisuals();
        }

        public void SetTheme(Color normal, Color highlightStart, Color highlightEnd)
        {
            normalColor = normal;
            highlightA = highlightStart;
            highlightB = highlightEnd;
            ApplyNormalVisual();
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            SetCollidersEnabled(enabled && !IsExtracted);
        }

        public void Highlight(bool on)
        {
            if (IsExtracted) return;
            if (on) ApplyHighlightVisual();
            else ApplyNormalVisual();
        }

        public void PlayExtraction(Action completed)
        {
            inputEnabled = false;
            SetCollidersEnabled(false);
            Highlight(true);
            StartCoroutine(ExtractRoutine(completed));
        }

        public void PlayWrongBump(Action completed = null)
        {
            StartCoroutine(WrongRoutine(completed));
        }

        public void DisableAndPool()
        {
            StopAllCoroutines();
            SetCollidersEnabled(false);
            IsExtracted = true;
            gameObject.SetActive(false);
        }

        private void BuildVisuals()
        {
            StopAllCoroutines();
            transform.localPosition = Vector3.zero;
            initialPosition = transform.localPosition;
            line.positionCount = data.cells.Count;
            for (int i = 0; i < data.cells.Count; i++)
                line.SetPosition(i, board.GridToLocalWorld(data.cells[i]));
            line.widthMultiplier = lineWidth;
            ApplyNormalVisual();
            BuildArrow();
            BuildColliders();
        }

        private void BuildArrow()
        {
            Vector3 head = board.GridToLocalWorld(data.Head);
            Vector2 d = data.headDirection;
            Vector2 side = new Vector2(-d.y, d.x);
            float length = cellSize * config.arrowLengthAsCell;
            float width = cellSize * config.arrowWidthAsCell;
            Vector3 tip = head + (Vector3)(d * length);
            Vector3 left = head + (Vector3)(side * width);
            Vector3 right = head - (Vector3)(side * width);

            var mesh = arrowMeshFilter.sharedMesh;
            if (mesh == null) mesh = new Mesh { name = "ArrowHeadMesh" };
            mesh.Clear();
            mesh.vertices = new[] { tip, left, right };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateBounds();
            arrowMeshFilter.sharedMesh = mesh;
            arrowObject.transform.localPosition = Vector3.zero;
            arrowRenderer.enabled = true;
            arrowRenderer.sortingOrder = 2;
        }

        private void BuildColliders()
        {
            EnsureCount(segmentColliders, Mathf.Max(0, data.cells.Count - 1), CreateSegmentCollider);
            EnsureCount(cellColliders, data.cells.Count, CreateCellCollider);

            for (int i = 0; i < segmentColliders.Count; i++) segmentColliders[i].enabled = i < data.cells.Count - 1;
            for (int i = 0; i < cellColliders.Count; i++) cellColliders[i].enabled = i < data.cells.Count;

            for (int i = 0; i < data.cells.Count - 1; i++)
            {
                var a = board.GridToLocalWorld(data.cells[i]);
                var b = board.GridToLocalWorld(data.cells[i + 1]);
                var col = segmentColliders[i];
                col.transform.localPosition = (a + b) * 0.5f;
                col.transform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, b - a));
                col.size = new Vector2(Vector3.Distance(a, b) + lineWidth, lineWidth * 1.7f);
            }

            for (int i = 0; i < data.cells.Count; i++)
            {
                var col = cellColliders[i];
                col.transform.localPosition = board.GridToLocalWorld(data.cells[i]);
                col.radius = lineWidth * 1.05f;
            }
        }

        private void ApplyNormalVisual()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(normalColor, 0f), new GradientColorKey(normalColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            line.colorGradient = gradient;
            arrowRenderer.material.color = normalColor;
        }

        private void ApplyHighlightVisual()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(highlightA, 0f), new GradientColorKey(highlightB, 0.5f), new GradientColorKey(highlightA, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            line.colorGradient = gradient;
            arrowRenderer.material.color = highlightB;
        }

        private IEnumerator ExtractRoutine(Action completed)
        {
            Vector3 start = transform.localPosition;
            Vector3 direction = new Vector3(data.headDirection.x, data.headDirection.y, 0f).normalized;
            int steps = ExitSteps();
            Vector3 target = start + direction * cellSize * steps;
            float elapsed = 0f;
            while (elapsed < config.extractDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / config.extractDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                transform.localPosition = Vector3.LerpUnclamped(start, target, t);
                yield return null;
            }
            transform.localPosition = target;
            IsExtracted = true;
            completed?.Invoke();
        }

        private IEnumerator WrongRoutine(Action completed)
        {
            Vector3 start = transform.localPosition;
            Vector3 dir = new Vector3(data.headDirection.x, data.headDirection.y, 0f).normalized;
            Vector3 bump = start - dir * cellSize * config.wrongShakeDistanceAsCell;
            float half = config.wrongShakeDuration * 0.5f;
            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localPosition = Vector3.Lerp(start, bump, Mathf.Clamp01(elapsed / half));
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localPosition = Vector3.Lerp(bump, start, Mathf.Clamp01(elapsed / half));
                yield return null;
            }
            transform.localPosition = start;
            completed?.Invoke();
        }

        private int ExitSteps()
        {
            int x = data.Head.x;
            int y = data.Head.y;
            int size = board.GridSize;
            if (data.headDirection.x > 0) return size + 2 - x;
            if (data.headDirection.x < 0) return size + 2 - (size - 1 - x);
            if (data.headDirection.y > 0) return size + 2 - y;
            return size + 2 - (size - 1 - y);
        }

        private void SetCollidersEnabled(bool enabled)
        {
            for (int i = 0; i < segmentColliders.Count; i++) segmentColliders[i].enabled = enabled && i < data.cells.Count - 1;
            for (int i = 0; i < cellColliders.Count; i++) cellColliders[i].enabled = enabled && i < data.cells.Count;
        }

        private BoxCollider2D CreateSegmentCollider()
        {
            var go = new GameObject("SegmentCollider");
            go.transform.SetParent(transform, false);
            go.layer = gameObject.layer;
            return go.AddComponent<BoxCollider2D>();
        }

        private CircleCollider2D CreateCellCollider()
        {
            var go = new GameObject("CellCollider");
            go.transform.SetParent(transform, false);
            go.layer = gameObject.layer;
            return go.AddComponent<CircleCollider2D>();
        }

        private static void EnsureCount<T>(List<T> list, int count, Func<T> factory) where T : Component
        {
            while (list.Count < count) list.Add(factory());
        }
    }