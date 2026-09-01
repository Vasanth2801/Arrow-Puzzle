using UnityEngine;

public class GridCell : MonoBehaviour
{
    [HideInInspector] public int pathId;
    [HideInInspector] public Vector2Int coord;
    [HideInInspector] public bool isHead;

    private void OnMouseDown()
    {
        if (BoardManager.Instance != null)
            BoardManager.Instance.OnCellTapped(this);
    }
}