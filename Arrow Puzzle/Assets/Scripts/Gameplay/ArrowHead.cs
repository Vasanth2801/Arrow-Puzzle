using UnityEngine;

public class ArrowHead : MonoBehaviour
{
    public int pathId;
    private BoardManager board;
     
    public void Setup(BoardManager owner, int id, Vector2 direction)
    {
        board = owner;
        pathId = id;

        Vector2 d = direction.normalized;
        float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnMouseDown()
    {
        if (board != null)
            board.TryMovePath(pathId);
    }
}
