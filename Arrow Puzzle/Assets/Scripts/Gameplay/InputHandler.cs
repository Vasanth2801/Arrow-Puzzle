using UnityEngine;
using UnityEngine.EventSystems;

    public sealed class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private BoardView boardView;
        [SerializeField] private LayerMask pathLayer = ~0;

        private void Awake()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began && !IsPointerOverUI(touch.fingerId)) HandleScreenPoint(touch.position);
                }
                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI(-1)) HandleScreenPoint(Input.mousePosition);
        }

        private void HandleScreenPoint(Vector2 screenPoint)
        {
            Vector3 world = gameplayCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, -gameplayCamera.transform.position.z));
            Collider2D hit = Physics2D.OverlapPoint(world, pathLayer);
            if (hit == null) return;
            var path = hit.GetComponentInParent<PathView>();
            if (path != null) boardView.TryExtract(path);
        }

        private static bool IsPointerOverUI(int pointerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
        }
    }