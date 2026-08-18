using System.Collections;
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
    [SerializeField] private bool isExiting = false;
    private Vector3 targetPosition;

    [Header("Sprite settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color blockedColor = Color.red;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private float blockedDistance = 0.15f;
    [SerializeField] private float blockedDuration = 0.2f;

    private Vector3 originalPosition;

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
        isExiting = true;
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

            if (isExiting) 
            { 
                ExitBoard();
            }
        }
    }

    public void SelectArrow()
    {
        if(isMoving)
        {
            return;
        }

        Vector2Int currentPosition = gridPosition;

        while(true)
        {
            Vector2Int nextPosition = gridManager.GetNextPosition(currentPosition, direction);

            if(!gridManager.IsInsideGrid(nextPosition))
            {
                Move(gridManager.GetExitPosition(this));
                return;
            }

            ArrowController blockingArrow = gridManager.GetArrowAtPosition(nextPosition);

            if(blockingArrow != null)
            {
                Vector3 stopPosition = gridManager.GetWorldPosition(currentPosition);

                StartCoroutine(MoveToBlockedPosition(stopPosition));

                gridManager.UpdateArrowPosition(this, currentPosition);

                return;
            }

            currentPosition = nextPosition;
        }
    }

    private IEnumerator MoveToBlockedPosition(Vector3 target)
    {
        isMoving = true;

        while(Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        isMoving = false;

        StartCoroutine(BlockedFeedback());
    }

    private IEnumerator BlockedFeedback()
    {
        originalPosition = transform.position;
        spriteRenderer.color = blockedColor;

        Vector3 pushPosition = originalPosition;

        switch(direction)
        {
            case ArrowDirection.Up:
                pushPosition += Vector3.up * blockedDistance;
                break;
            case ArrowDirection.Down:
                pushPosition += Vector3.down * blockedDistance;
                break;
            case ArrowDirection.Left:
                pushPosition += Vector3.left * blockedDistance;
                break;
            case ArrowDirection.Right:
                pushPosition += Vector3.right * blockedDistance;
                break;
        }

        yield return MoveToPosition(pushPosition);
        yield return new WaitForSeconds(0.5f);
        yield return MoveToPosition(originalPosition);
        spriteRenderer.color = normalColor;
    }

    private IEnumerator MoveToPosition(Vector3 target)  
    {
        while(Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
    }

    private void ExitBoard()
    {
        gridManager.ArrowExited();

        Destroy(gameObject);
    }
}