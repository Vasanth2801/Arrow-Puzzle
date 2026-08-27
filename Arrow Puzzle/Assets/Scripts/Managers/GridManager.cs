using System.Collections.Generic;
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

    private Dictionary<Vector2Int, ArrowController> arrows = new Dictionary<Vector2Int, ArrowController>();

    public void LoadLevel(Leveldata level)
    {
        ClearLevel();

        currentLevel = level;

        GenerateGrid();

        PlaceArrows();
    }

    private void ClearGrid()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        arrows.Clear();

        grid = null;
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

    private void SetArrowRotation(ArrowController arrow, ArrowDirection direction)
    {
        switch(direction)
        {
            case ArrowDirection.Up:
                arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case ArrowDirection.Right:
                arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case ArrowDirection.Down:
                arrow.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case ArrowDirection.Left:
                arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
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

    public Vector2Int GetNextPosition(Vector2Int  position, ArrowDirection direction)
    {
        return ArrowUtility.GetNextPosition(position, direction);
    }

    public Vector3 GetExitPosition(ArrowController arrow)
    {
        Vector3 current = arrow.transform.position;

        float distance = 2f;

        switch(arrow.direction)
        {
            case ArrowDirection.Up:
                return current + Vector3.up * distance;
            case ArrowDirection.Down:
                return current + Vector3.down * distance;
            case ArrowDirection.Left:
                return current + Vector3.left * distance;
            case ArrowDirection.Right:
                return current + Vector3.right * distance;
        }

        return current;
    }

    public bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < currentLevel.rows && position.y >= 0 && position.y < currentLevel.columns;
    }

    public ArrowController GetArrowAtPosition(Vector2Int position)
    {
        if(arrows.ContainsKey(position))
        {
            return arrows[position];
        }

        return null;
    }

    public void UpdateArrowPosition(ArrowController arrow, Vector2Int newPosition)
    {
        if(arrows.ContainsKey(arrow.gridPosition))
        {
            arrows.Remove(arrow.gridPosition);
        }

        arrow.gridPosition = newPosition;

        arrows[newPosition] = arrow;
    }
    
    public int GetRemainingArrow()
    {
        return FindObjectsByType<ArrowController>(FindObjectsSortMode.None).Length - 1;
    }

    public void ArrowExited()
    {
        if(arrows.ContainsKey(arrow.gridPosition))
        {
            arrows.Remove(arrow.gridPosition);
        }

        if(arrows.Count == 0)
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

    void ClearLevel()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}