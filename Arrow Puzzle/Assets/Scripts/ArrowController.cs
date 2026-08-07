using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [Header("Arrow position and direction")]
    public Vector2Int gridPosition;
    public ArrowDirection direction;

    [Header("References")]
    private GridManager gridManager;

    [Header("Arrow movement settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool isMoving = false;
    private Vector3 targetPosition;

    private void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
    }

    public void Move(Vector3 target)
    {
        if(isMoving)
        {
            return;
        }

        targetPosition = target;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;

            Destroy(gameObject);
        }
    }

    public void SelectArrow()
    {
        if(gridManager.CanMove(this))
        {
            if(isMoving)
            {
                return;
            }

            if(gridManager.CanMove(this))
            {
                Move(gridManager.GetExitPosition(this));
            }
            else
            {
                Debug.Log("Blocked! Cannot move in the current direction.");
            }
        }
    }
}