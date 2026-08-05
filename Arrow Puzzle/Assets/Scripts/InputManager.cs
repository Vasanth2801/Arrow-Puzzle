using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;

    private void Update()
    {
        HandleMouseMovement();
        HandleTouchInput();
    }

    void HandleMouseMovement()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse Clicked");
            Vector2 mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if(hit.collider != null)
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name}");
            }
        }
    }

    void HandleTouchInput()
    {
        if ((Input.touchCount < 0))
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                Debug.Log("Touch Began");
            }
        }
    }
}