using UnityEngine;

public static class ArrowUtility
{
     public static Vector2Int GetNextPosition(Vector2Int position, ArrowDirection direction)
    {
        switch(direction)
        {
            case ArrowDirection.Up:
                return new Vector2Int(position.x +1, position.y);
            case ArrowDirection.Down:
                return new Vector2Int(position.x - 1, position.y);
            case ArrowDirection.Left:
                return new Vector2Int(position.x, position.y - 1);
            case ArrowDirection.Right:
                return new Vector2Int(position.x, position.y + 1);
        }

        return position;
    }
}