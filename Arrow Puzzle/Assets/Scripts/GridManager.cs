using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("References for the Level,arrow and the grid")]
    [SerializeField] private Cell cell;
    [SerializeField] private ArrowController arrow;
    [SerializeField] private Leveldata currentLevel;

    [Header("Spacing between the cells and the size of the cells")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float spacing = 0.1f;

    private Cell[,] grid;

    private void Start()
    {
        GenerateGrid();
        PlaceArrows();
    }

    private void GenerateGrid()
    {
        grid = new Cell[currentLevel.rows, currentLevel.columns];

        float gridWidth = (currentLevel.columns -1) * (cellSize + spacing);
        float gridHeight = (currentLevel.rows -1) * (cellSize + spacing);

        Vector3 startPosition = new Vector3(-gridWidth / 2, -gridHeight / 2, 0);

        for (int i = 0; i < currentLevel.rows; i++)
        {
            for(int j = 0; j < currentLevel.columns; j++)
            {
                Vector3 spawnPosition = startPosition + new Vector3(j * (cellSize + spacing), i * (cellSize + spacing), 0);

                Cell newCell = Instantiate(cell, spawnPosition, Quaternion.identity, transform);
                newCell.gridPosition = new Vector2Int(i, j);
                grid[i, j] = newCell;
            }
        }
        Debug.Log($"Grid generated with {currentLevel.rows} rows and {currentLevel.columns} columns.");
    }

    private void PlaceArrows()
    {
        foreach(ArrowData arrowData in currentLevel.arrows)
        {
            Debug.Log($"Placing arrow at grid position {arrowData.gridPosition} with direction {arrowData.direction}.");
            Cell targetCell = grid[arrowData.gridPosition.x, arrowData.gridPosition.y];

            ArrowController newArrow = Instantiate(arrow, targetCell.transform.position, Quaternion.identity, transform);
            
            newArrow.gridPosition = arrowData.gridPosition;
            newArrow.direction = arrowData.direction;

            switch(arrowData.direction)
            {
                case ArrowDirection.Up:
                    newArrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case ArrowDirection.Right:    
                    newArrow.transform.rotation = Quaternion.Euler(0, 0, -90);
                    break;
                case ArrowDirection.Down:
                    newArrow.transform.rotation = Quaternion.Euler(0, 0, 180);
                    break;
                case ArrowDirection.Left:
                    newArrow.transform.rotation = Quaternion.Euler(0, 0, 90);
                    break;
            }
        }
    }

    public bool CanMove(ArrowController arrow)
    {
        return true;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return grid[gridPos.x, gridPos.y].transform.position;
    }

    public Vector2Int GetNextPosition(Vector2Int  currentPosition, ArrowDirection direction)
    {
        switch (direction)
        {
            case ArrowDirection.Up:
                return new Vector2Int(currentPosition.x + 1, currentPosition.y);
            case ArrowDirection.Down:
                return new Vector2Int(currentPosition.x - 1, currentPosition.y);
            case ArrowDirection.Left:
                return new Vector2Int(currentPosition.x, currentPosition.y - 1);
            case ArrowDirection.Right:
                return new Vector2Int(currentPosition.x, currentPosition.y + 1);
            default:
                return currentPosition;
        }
    }

    public bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < currentLevel.rows && position.y >= 0 && position.y < currentLevel.columns;
    }
}