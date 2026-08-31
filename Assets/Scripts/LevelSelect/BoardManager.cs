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


    // =========================================================
    // AWAKE
    // =========================================================

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

            if (grid[x, y] != null)
            {
                Debug.LogWarning(
                    "BoardManager: Duplicate GridCell at (" +
                    x + ", " + y + ")"
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


        // Turn off previous highlight.
        if (currentlySelectedCell != null)
        {
            currentlySelectedCell.SetHighlight(false);
        }


        // Select new cell.
        currentlySelectedCell = newSelectedCell;

        currentlySelectedCell.SetHighlight(true);


        Debug.Log(
            "Selected Cell: (" +
            x +
            ", " +
            y +
            ")"
        );
    }


    // =========================================================
    // CHECK CURRENT POSITION
    // =========================================================

    public bool CanPlaceCurrentBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null)
        {
            return false;
        }

        if (currentlySelectedCell.IsOccupied)
        {
            return false;
        }

        return CanPlaceBuildingAt(
            buildingIndex,
            currentlySelectedCell.X,
            currentlySelectedCell.Y
        );
    }


    // =========================================================
    // CHECK ANY VALID POSITION
    // =========================================================

    public bool HasValidPlacement(int buildingIndex)
    {
        if (grid == null)
            return false;

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (grid[x, y] == null)
                    continue;

                if (CanPlaceBuildingAt(
                    buildingIndex,
                    x,
                    y))
                {
                    return true;
                }
            }
        }

        return false;
    }


    // =========================================================
    // PLACEMENT RULES
    // =========================================================

    private bool CanPlaceBuildingAt(
        int buildingIndex,
        int x,
        int y)
    {
        GridCell cell = GetCell(x, y);

        if (cell == null)
            return false;

        if (cell.IsOccupied)
            return false;


        // -----------------------------------------------------
        // BLUE
        // -----------------------------------------------------

        if (buildingIndex == 0)
        {
            return true;
        }


        // -----------------------------------------------------
        // RED
        // Must have BLUE neighbour.
        // -----------------------------------------------------

        if (buildingIndex == 1)
        {
            return HasNeighbour(
                x,
                y,
                GridCell.BuildingType.Blue
            );
        }


        // -----------------------------------------------------
        // GREEN
        // Must have BLUE AND RED neighbours.
        // -----------------------------------------------------

        if (buildingIndex == 2)
        {
            return
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Blue
                )
                &&
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Red
                );
        }


        // -----------------------------------------------------
        // YELLOW
        // Must have BLUE, RED AND GREEN neighbours.
        // -----------------------------------------------------

        if (buildingIndex == 3)
        {
            return
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Blue
                )
                &&
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Red
                )
                &&
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Green
                );
        }


        return false;
    }


    // =========================================================
    // NEIGHBOUR CHECK
    // =========================================================

    private bool HasNeighbour(
        int x,
        int y,
        GridCell.BuildingType requiredType)
    {
        GridCell neighbour;


        // UP
        neighbour = GetCell(x, y - 1);

        if (neighbour != null &&
            neighbour.CurrentBuilding == requiredType)
        {
            return true;
        }


        // DOWN
        neighbour = GetCell(x, y + 1);

        if (neighbour != null &&
            neighbour.CurrentBuilding == requiredType)
        {
            return true;
        }


        // LEFT
        neighbour = GetCell(x - 1, y);

        if (neighbour != null &&
            neighbour.CurrentBuilding == requiredType)
        {
            return true;
        }


        // RIGHT
        neighbour = GetCell(x + 1, y);

        if (neighbour != null &&
            neighbour.CurrentBuilding == requiredType)
        {
            return true;
        }


        return false;
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


        if (!CanPlaceCurrentBuilding(buildingIndex))
        {
            Debug.Log(
                "Cannot place building " +
                buildingIndex +
                " at (" +
                currentlySelectedCell.X +
                ", " +
                currentlySelectedCell.Y +
                ")."
            );

            return false;
        }


        Sprite spriteToPlace =
            GetBuildingSprite(buildingIndex);

        if (spriteToPlace == null)
        {
            Debug.LogWarning(
                "BoardManager: No sprite assigned for building index " +
                buildingIndex
            );

            return false;
        }


        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);


        currentlySelectedCell.SetBuilding(
            spriteToPlace,
            buildingType
        );


        // Remove selection highlight.
        currentlySelectedCell.SetHighlight(false);


        Debug.Log(
            "Placed building " +
            buildingType +
            " at (" +
            currentlySelectedCell.X +
            ", " +
            currentlySelectedCell.Y +
            ")"
        );


        return true;
    }


    // =========================================================
    // SPRITE SELECTION
    // =========================================================

    private Sprite GetBuildingSprite(
        int buildingIndex)
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
    // BUILDING TYPE
    // =========================================================

    private GridCell.BuildingType GetBuildingType(
        int buildingIndex)
    {
        switch (buildingIndex)
        {
            case 0:
                return GridCell.BuildingType.Blue;

            case 1:
                return GridCell.BuildingType.Red;

            case 2:
                return GridCell.BuildingType.Green;

            case 3:
                return GridCell.BuildingType.Yellow;

            default:
                return GridCell.BuildingType.None;
        }
    }


    // =========================================================
    // BUILDING NAME
    // =========================================================

    public string GetBuildingName(int buildingIndex)
    {
        switch (buildingIndex)
        {
            case 0:
                return "Blue Building";

            case 1:
                return "Red Building";

            case 2:
                return "Green Building";

            case 3:
                return "Yellow Building";

            default:
                return "Unknown Building";
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