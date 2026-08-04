using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Cell cell;
    [SerializeField] private ArrowController arrow;
    [SerializeField] private Leveldata currentLevel;

    [SerializeField] private float cellSize = 1f;

    private Cell[,] grid;

    private void Start()
    {
        GenerateGrid();
        PlaceArrows();
    }

    private void GenerateGrid()
    {

    }

    private void PlaceArrows()
    {

    }
}