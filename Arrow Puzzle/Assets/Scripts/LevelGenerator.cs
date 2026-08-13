using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int numberOfLevels = 10;
    [SerializeField] private int rows = 9;
    [SerializeField] private int columns = 4;

    [Header("Difficulty")]
    [SerializeField] private int chainLength = 3;

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

        int attempts = 0;
        int maxAttempts = numberOfLevels * 100;

        while (generatedLevels.Count < numberOfLevels && attempts < maxAttempts) 
        {
            attempts++;

            Leveldata level = CreateLevel();

            if(levelValidator.IsLevelSolvable(level))
            {
                generatedLevels.Add(level);

                Debug.Log("Generated valid levels " + generatedLevels.Count);
            }
        }

        Debug.Log("Generated " + generatedLevels.Count + "valid Levels");

        return generatedLevels;
    }

    private Leveldata CreateLevel()
    {
        Leveldata level = ScriptableObject.CreateInstance<Leveldata>();

        level.rows = rows;
        level.columns = columns;

        int arrowCount = GetArrowCount();

        level.arrows = new ArrowData[arrowCount];

        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        ArrowDirection chainDirection = GetRandomDirection();

        Vector2Int startPosition = GetValidStartPosition(chainDirection);

        for(int i =0; i< arrowCount; i++)
        {
            Vector2Int position = new Vector2Int(startPosition.x, startPosition.y);

            position.x += GetRowOffset(chainDirection, i);

            if (!IsInsideGrid(position))
            {
                position = GetRandomFreePosition(usedPositions);
            }

            if(usedPositions.Contains(position))
            {
                position = GetRandomFreePosition(usedPositions);
            }

            usedPositions.Add(position);

            ArrowData arrow = new ArrowData();

            arrow.gridPosition = position;

            arrow.direction = chainDirection;

            level.arrows[i] = arrow;
        }

        return level;
    }

    private int GetArrowCount()
    {
        int minimum = Mathf.Max(2, chainLength);

        int maximum = Mathf.Min(minimum + 2, rows * columns);

        return Random.Range(minimum, maximum + 1);
    }

    private Vector2Int GetValidStartPosition(ArrowDirection direction)
    {
        switch(direction)
        {
            case ArrowDirection.Up:
                return new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));
            case ArrowDirection.Down:
                return new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));
            case ArrowDirection.Left:
                return new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));
            case ArrowDirection.Right:
                return new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));
        }

        return Vector2Int.zero;
    }

    private int GetRowOffset(ArrowDirection direction, int index)
    {
        if(direction == ArrowDirection.Up)
        {
            return index;
        }

        if(direction == ArrowDirection.Down)
        {
            return -index;   
        }

        return 0;
    }

    private int GetColumnOffSet(ArrowDirection direction, int index)
    {
        if(direction == ArrowDirection.Right)
        {
            return index;
        }

        if(direction == ArrowDirection.Left)
        {
            return -index;
        }

        return 0;
    }

    private Vector2Int GetRandomFreePosition(HashSet<Vector2Int> usedPositions)
    {
        Vector2Int position;

        int attempts = 0;

        do
        {
            position = new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));
            attempts++;

        } while (usedPositions.Contains(position) && attempts < 100);

        return position;
    }

    private bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < rows && position.y >= 0 && position.y < columns;
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
}