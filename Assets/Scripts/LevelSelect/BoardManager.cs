using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardWidth = 5;
    [SerializeField] private int boardHeight = 5;

    [Header("Prebuilt Grid")]
    [SerializeField] private GridCell[] gridCells;

    private GridCell[,] grid;

    private GridCell currentlySelectedCell;


    private void Awake()
    {
        BuildGridReference();
    }


    private void BuildGridReference()
    {
        grid = new GridCell[boardWidth, boardHeight];

        if (gridCells == null || gridCells.Length == 0)
        {
            Debug.LogWarning("BoardManager: No GridCells have been assigned.");
            return;
        }

        foreach (GridCell cell in gridCells)
        {
            if (cell == null)
                continue;

            int x = cell.X;
            int y = cell.Y;

            if (x < 0 || x >= boardWidth ||
                y < 0 || y >= boardHeight)
            {
                Debug.LogWarning(
                    "GridCell " + cell.name +
                    " has invalid coordinates: " +
                    x + ", " + y
                );

                continue;
            }

            grid[x, y] = cell;
        }

        Debug.Log(
            "BoardManager: " +
            gridCells.Length +
            " GridCells assigned."
        );
    }


    public void SetSelectedCell(int x, int y)
    {
        if (grid == null)
        {
            Debug.LogWarning("BoardManager: Grid has not been initialized.");
            return;
        }

        if (x < 0 || x >= boardWidth ||
            y < 0 || y >= boardHeight)
        {
            Debug.LogWarning(
                "Invalid board position: " +
                x + ", " + y
            );

            return;
        }

        GridCell newSelectedCell = grid[x, y];

        if (newSelectedCell == null)
        {
            Debug.LogWarning(
                "No GridCell assigned at position: " +
                x + ", " + y
            );

            return;
        }

        if (currentlySelectedCell != null)
        {
            currentlySelectedCell.SetHighlight(false);
        }

        currentlySelectedCell = newSelectedCell;

        currentlySelectedCell.SetHighlight(true);
    }


    public GridCell GetCell(int x, int y)
    {
        if (grid == null)
            return null;

        if (x < 0 || x >= boardWidth ||
            y < 0 || y >= boardHeight)
        {
            return null;
        }

        return grid[x, y];
    }


    public GridCell GetSelectedCell()
    {
        return currentlySelectedCell;
    }
}