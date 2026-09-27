using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathData
{
    public int id;
    public List<Vector2Int> cells = new List<Vector2Int>();
    public bool cleared;

    public Vector2Int Head
    {
        get { return cells[cells.Count - 1]; }
    }

    public Vector2Int PreviousToHead
    {
        get { return cells.Count >= 2 ? cells[cells.Count - 2] : Head; }
    }

    public Vector2Int HeadDirection
    {
        get
        {
            Vector2Int d = Head - PreviousToHead;
            if (d == Vector2Int.zero) return Vector2Int.up;
            return new Vector2Int(Mathf.Clamp(d.x, -1, 1), Mathf.Clamp(d.y, -1, 1));
        }
    }
}
