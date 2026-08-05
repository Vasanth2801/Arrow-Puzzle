using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public Vector2Int gridPosition;

    public ArrowDirection direction;

    private GridManager gridManager;

    private void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
    }

    public void SelectArrow()
    {
        if(gridManager.CanMove(this))
        {
            Debug.Log("Arrow selected and can move.");
        }
    }
}
