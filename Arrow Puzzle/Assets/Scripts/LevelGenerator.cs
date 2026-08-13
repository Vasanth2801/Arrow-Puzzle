using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int numberOfLevels = 10;
    [SerializeField] private int rows = 9;
    [SerializeField] private int columns = 4;

    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private LevelValidator levelValidator;

    private void Start()
    {
        levelValidator = FindAnyObjectByType<LevelValidator>();
    }

    public List<Leveldata> GenerateLevels()
    {
        List<Leveldata> generatedLevels = new List<Leveldata>();

        for(int i = 0; i < numberOfLevels; i++)
        {
            Leveldata newLevel = CreateLevel(i);

            if(levelValidator.IsLevelSolvable(newLevel))
            {
                generatedLevels.Add(newLevel);
            }
            else
            {
                i--;
            }
        }
        Debug.Log("Generated " + generatedLevels.Count + " Levels");

        return generatedLevels;
    }

    private Leveldata CreateLevel(int levelNumber)
    {
        Leveldata level = ScriptableObject.CreateInstance<Leveldata>();

        level.rows = rows;
        level.columns = columns;

        int arrowCount = GetArrowCount(levelNumber);

        if(arrowCount > rows * columns)
        {
            arrowCount = rows * columns;
        }

        level.arrows = new ArrowData[arrowCount];

        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        for(int i = 0;i < arrowCount;i++)
        {
            Vector2Int position;

            do
            {
               position = new Vector2Int(Random.Range(0,rows), Random.Range(0,columns));
            } while (usedPositions.Contains(position));

            usedPositions.Add(position);

            ArrowData arrow = new ArrowData();
            arrow.gridPosition = position;
            arrow.direction = GetRandomDirection();

            level.arrows[i] = arrow;  
        }

        return level;
    }

    private int GetArrowCount(int levelNumber)
    {
        if(levelNumber < 10)
        {
            return Random.Range(2, 4);
        }
        else if(levelNumber < 25)
        {
            return Random.Range(3, 6);
        }
        else if(levelNumber < 50)
        {
            return Random.Range(4, 8);
        }
        else if(levelNumber < 100)
        {
            return Random.Range(5, 10);
        }
        else if(levelNumber < 250)
        {
            return Random.Range(6, 12);
        }
        else if(levelNumber < 250)
        {
            return Random.Range(7, 14);
        }

        return Random.Range(8, 16);
    }

    private ArrowDirection GetRandomDirection()
    {
        int randomDirection = Random.Range(0, 4);

        switch(randomDirection)
        {
            case 0:
                return ArrowDirection.Up;
            case 1:
                return ArrowDirection.Down;
            case 2:
                return ArrowDirection.Right;
            case 3:
                return ArrowDirection.Left;
        }

        return ArrowDirection.Up;
    }

    private bool IsLevelValid(Leveldata level)
    {
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        foreach(ArrowData arrow in level.arrows)
        {
            if(positions.Contains(arrow.gridPosition))
            {
                return false;
            }

            positions.Add(arrow.gridPosition);
        }

        foreach(ArrowData arrow in level.arrows)
        {
            Vector2Int current = arrow.gridPosition;

            while(true)
            {
                Vector2Int next = GetNextPosition(current, arrow.direction);

                if(!IsInsideGrid(next, level))
                {
                    break;
                }

                current = next;
            }
        }

        return true;
    }

    private Vector2Int GetNextPosition(Vector2Int position, ArrowDirection direction)
    {
        switch(direction)
        {
            case ArrowDirection.Up:
                return new Vector2Int(position.x + 1, position.y);
            case ArrowDirection.Down:
                return new Vector2Int(position.x - 1, position.y);
            case ArrowDirection.Left:
                return new Vector2Int(position.x, position.y - 1);
            case ArrowDirection.Right:
                return new Vector2Int(position.x, position.y+1);
        }

        return position;
    }

    private bool IsInsideGrid(Vector2Int position, Leveldata level)
    {
        return position.x >= 0 && position.x < level.rows && position.y > 0 && position.y < level.columns;
    }
}