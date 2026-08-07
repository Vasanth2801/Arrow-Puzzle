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
    private Vector3 targetPosition;

    [Header("Sprite settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color blockedColor = Color.red;
    [SerializeField] private Color normalColor = Color.green;

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
        Debug.Log($"Arrow at {gridPosition} selected. Direction: {direction}");

        bool canMove = gridManager.CanMove(this);

        Debug.Log("Can move: " + canMove);

        if (canMove)
        {
            Debug.Log("Moving arrow to exit position.");
            Move(gridManager.GetExitPosition(this));
        }
        else
        {
            Debug.Log("Blocked");
            StartCoroutine(BlockedFeedback());
        }
    }

    private IEnumerator BlockedFeedback()
    {
        spriteRenderer.color = blockedColor;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.color = normalColor;
    }
}