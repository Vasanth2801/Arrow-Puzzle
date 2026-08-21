using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("References for the Level,arrow and the grid")]
    [SerializeField] private Cell cell;
    [SerializeField] private ArrowController arrow;

    [Header("References")]
    private Leveldata currentLevel;
    private LevelManager levelManager;
    [SerializeField] private LevelCompleteUI levelCompleteUI;
    
    [Header("Spacing between the cells and the size of the cells")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float spacing = 0.1f;

    private Cell[,] grid;

    private void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();

        currentLevel = levelManager.GetCurrentLevel();

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
    }

    private void PlaceArrows()
    {
        foreach(ArrowData arrowData in currentLevel.arrows)
        {
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

    public bool CanMove(ArrowController selectedArrow)
    {
        Vector2Int current = selectedArrow.gridPosition;

        while(true)
        {
            Vector2Int next = GetNextPosition(current, selectedArrow.direction);

            if(!IsInsideGrid(next))
            {
                return true;
            } 

            foreach (ArrowController arrow in FindObjectsByType<ArrowController>(FindObjectsSortMode.None))
            {
                if(arrow == selectedArrow)
                {
                    continue;
                }

                if(arrow.gridPosition == next)
                {
                    return false;
                }
            }

            current = next;
        }
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

    public Vector3 GetExitPosition(ArrowController arrow)
    {
        Vector2Int current = arrow.gridPosition;

        while (true)
        {
            Vector2Int next = GetNextPosition(current, arrow.direction);

            if (!IsInsideGrid(next))
            {
                break;
            }

            current = next;
        }

        Vector3 exitPosition = GetWorldPosition(current);

        switch (arrow.direction)
        {
            case ArrowDirection.Up:
                exitPosition += Vector3.up * (cellSize + spacing) * 5;
                break;
            case ArrowDirection.Down:
                exitPosition += Vector3.down * (cellSize + spacing) * 5;
                break;
            case ArrowDirection.Left:
                exitPosition += Vector3.left * (cellSize + spacing) * 5;
                break;
            case ArrowDirection.Right:
                exitPosition += Vector3.right * (cellSize + spacing) * 5;
                break;
        }

        return exitPosition;
    }

    public bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < currentLevel.rows && position.y >= 0 && position.y < currentLevel.columns;
    }

    public ArrowController GetArrowAtPosition(Vector2Int position)
    {
        ArrowController[] arrows = FindObjectsByType<ArrowController>(FindObjectsSortMode.None);

        foreach(ArrowController ar in arrows)
        {
            if(ar.gridPosition == position)
            {
                return ar;
            }
        }

        return null;
    }

    public void UpdateArrowPosition(ArrowController arrow, Vector2Int newPosition)
    {
        arrow.gridPosition = newPosition;
    }
    
    public int GetRemainingArrow()
    {
        return FindObjectsByType<ArrowController>(FindObjectsSortMode.None).Length - 1;
    }

    public void ArrowExited()
    {
        ArrowController[] remainingArrows = FindObjectsByType<ArrowController>(FindObjectsSortMode.None);

        Debug.Log($"Remaining Arrows : {remainingArrows.Length - 1}");

        if(remainingArrows.Length-1 == 0)
        {
            Debug.Log("Level Complete");

            if(levelCompleteUI != null)
            {
                levelCompleteUI.Show();
            }
        }
    }

    public bool IsLevelComplete()
    {
        ArrowController[] arrows = FindObjectsByType<ArrowController>(FindObjectsSortMode.None);

        return arrows.Length == 0;
    }


    public void LoadLevel(Leveldata leveldata)
    {
        ClearLevel();

        currentLevel = leveldata;

        GenerateGrid();
        PlaceArrows();
    }

    void ClearLevel()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}