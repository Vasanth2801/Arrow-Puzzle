using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Generation Seetings")]
    [SerializeField] private int numberOfLevels = 10;
    [SerializeField] private int rows = 9;
    [SerializeField] private int columns = 4;

    [Header("References")]
    [SerializeField] private LevelManager levelManager;

    public List<Leveldata> GenerateLevels()
    {
        List<Leveldata> generatedLevels = new List<Leveldata>();

        for(int i = 0; i < numberOfLevels; i++)
        {
            Leveldata newLevel = CreateLevel(i);
            generatedLevels.Add(newLevel);
        }

        Debug.Log("Generated " + generatedLevels.Count + " Levels");

        return generatedLevels;
    }

    private Leveldata CreateLevel(int levelNumber)
    {
        Leveldata level = ScriptableObject.CreateInstance<Leveldata>();

        level.rows = rows;
        level.columns = columns;

        int arrowCount = 2 + levelNumber;

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