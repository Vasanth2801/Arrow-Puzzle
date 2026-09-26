using UnityEngine;

public class GridCell : MonoBehaviour
{
    [HideInInspector] public int pathId;
    [HideInInspector] public Vector2Int coord;
    [HideInInspector] public bool isHead;

    // Direct reference to the ARROW's SpriteRenderer (the only sprite that's
    // actually visible — the square "Visual" background is disabled).
    // BoardManager sets this right after it creates the arrow. Flash/hint
    // code uses this instead of GetComponentInChildren, which used to grab
    // the invisible background square by mistake and silently do nothing.
    [HideInInspector] public SpriteRenderer visualSprite;

    private void OnMouseDown()
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.OnCellTapped(this);
        }
    }
}
