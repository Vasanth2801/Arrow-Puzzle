using UnityEngine;

public class GridCell : MonoBehaviour
{
    [HideInInspector] public int pathId;
    [HideInInspector] public Vector2Int coord;
    [HideInInspector] public bool isHead;

    // Unity automatically calls this when this object's collider is clicked
    // (works with mouse in the Editor, and with touch on mobile builds too).
    private void OnMouseDown()
    {
        if (BoardManager.Instance != null)
            BoardManager.Instance.OnCellTapped(this);
    }
}