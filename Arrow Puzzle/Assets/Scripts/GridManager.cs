using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Cell cell;
    [SerializeField] private ArrowController arrow;
    [SerializeField] private Leveldata currentLevel;

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
    }

    private void PlaceArrows()
    {

    }
}