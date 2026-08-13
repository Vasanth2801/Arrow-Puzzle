using UnityEngine;
using System.Collections.Generic;

public class LevelValidator : MonoBehaviour
{
    public bool IsLevelSolvable(Leveldata level)
    {
        List<ArrowData> remainingArrows = new List<ArrowData>(level.arrows);

        while(remainingArrows.Count > 0)
        {
            bool arrowRemoved = false;

            for (int i = 0; i < remainingArrows.Count; i++)
            {
                if (CanArrowExit(remainingArrows[i], remainingArrows, level))
                {
                    remainingArrows.RemoveAt(i);
                    arrowRemoved = true;
                    break;
                }
            }

            if(!arrowRemoved)
            {
                return false;
            }
        }

        return true;
    }

    private bool CanArrowExit(ArrowData arrow, List<ArrowData> arrows, Leveldata level)
    {
        Vector2Int currentPosition = arrow.gridPosition;

        while(true)
        {
            Vector2Int nextPosition = GetNextPosition(currentPosition, arrow.direction);

            if(!IsInsideGrid(nextPosition, level))
            {
                return true;
            }

            if(IsCellOccupied(nextPosition, arrows))
            {
                return false;
            }

            currentPosition = nextPosition;
        }
    }

    private bool IsCellOccupied(Vector2Int position, List<ArrowData> arrows)
    {
        foreach(ArrowData arrow in arrows)
        {
            if(arrow.gridPosition == position)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2Int GetNextPosition(Vector2Int position, ArrowDirection direction)
    {
        switch(direction)
        {
            case ArrowDirection.Up:
                return new Vector2Int(position.x + 1, position.y);
            case ArrowDirection.Down:
                return new Vector2Int(position.x -1, position.y);
            case ArrowDirection.Left:
                return new Vector2Int(position.x, position.y - 1);
            case ArrowDirection.Right:
                return new Vector2Int(position.x, position.y + 1);
        }

        return position;
    }

    private bool IsInsideGrid(Vector2Int position, Leveldata level)
    {
        return position.x >= 0 && position.x < level.rows && position.y >= 0 && position.y < level.columns;
    }
}