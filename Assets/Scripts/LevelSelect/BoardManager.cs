using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardWidth = 5;
    [SerializeField] private int boardHeight = 5;

    [Header("Prebuilt Grid")]
    [SerializeField] private GridCell[] gridCells;

    [Header("Building Sprites")]
    [SerializeField] private Sprite blueBuildingSprite;
    [SerializeField] private Sprite redBuildingSprite;
    [SerializeField] private Sprite greenBuildingSprite;
    [SerializeField] private Sprite yellowBuildingSprite;

    private GridCell[,] grid;

    private GridCell currentlySelectedCell;


    private void Awake()
    {
        BuildGridReference();
    }


    // =========================================================
    // GRID SETUP
    // =========================================================

    private void BuildGridReference()
    {
        grid = new GridCell[boardWidth, boardHeight];

        if (gridCells == null || gridCells.Length == 0)
        {
            Debug.LogWarning(
                "BoardManager: No GridCells have been assigned."
            );

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


    // =========================================================
    // CELL SELECTION
    // =========================================================

    public void SetSelectedCell(int x, int y)
    {
        if (grid == null)
        {
            Debug.LogWarning(
                "BoardManager: Grid has not been initialized."
            );

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


    // =========================================================
    // BUILDING PLACEMENT
    // =========================================================

    public bool PlaceBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null)
        {
            Debug.LogWarning(
                "BoardManager: No cell is currently selected."
            );

            return false;
        }

        if (currentlySelectedCell.IsOccupied)
        {
            Debug.Log(
                "Cannot place building. Cell (" +
                currentlySelectedCell.X +
                ", " +
                currentlySelectedCell.Y +
                ") is already occupied."
            );

            return false;
        }

        Sprite spriteToPlace = GetBuildingSprite(buildingIndex);

        if (spriteToPlace == null)
        {
            Debug.LogWarning(
                "BoardManager: No sprite assigned for building index " +
                buildingIndex
            );

            return false;
        }

        currentlySelectedCell.SetBuilding(spriteToPlace);

        Debug.Log(
            "Placed building " +
            buildingIndex +
            " at (" +
            currentlySelectedCell.X +
            ", " +
            currentlySelectedCell.Y +
            ")"
        );

        return true;
    }


    private Sprite GetBuildingSprite(int buildingIndex)
    {
        switch (buildingIndex)
        {
            case 0:
                return blueBuildingSprite;

            case 1:
                return redBuildingSprite;

            case 2:
                return greenBuildingSprite;

            case 3:
                return yellowBuildingSprite;

            default:
                Debug.LogWarning(
                    "BoardManager: Invalid building index " +
                    buildingIndex
                );

                return null;
        }
    }


    // =========================================================
    // GETTERS
    // =========================================================

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