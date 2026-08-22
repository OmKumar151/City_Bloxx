using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // =========================================================
    // BOARD SETTINGS
    // =========================================================

    [Header("Board Settings")]
    [SerializeField] private int boardWidth = 5;
    [SerializeField] private int boardHeight = 5;


    // =========================================================
    // GRID
    // =========================================================

    [Header("Prebuilt Grid")]
    [SerializeField] private GridCell[] gridCells;


    // =========================================================
    // BUILDING SPRITES
    // =========================================================

    [Header("Building Sprites")]
    [SerializeField] private Sprite blueBuildingSprite;
    [SerializeField] private Sprite redBuildingSprite;
    [SerializeField] private Sprite greenBuildingSprite;
    [SerializeField] private Sprite yellowBuildingSprite;


    // =========================================================
    // INTERNAL GRID
    // =========================================================

    private GridCell[,] grid;

    private GridCell currentlySelectedCell;


    // =========================================================
    // INITIALIZATION
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


            // Make sure the cell coordinates are valid.
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


            // Check for duplicate coordinates.
            if (grid[x, y] != null)
            {
                Debug.LogWarning(
                    "BoardManager: Multiple GridCells exist at (" +
                    x + ", " +
                    y +
                    ")."
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
                x + ", " +
                y
            );

            return;
        }


        GridCell newSelectedCell = grid[x, y];


        if (newSelectedCell == null)
        {
            Debug.LogWarning(
                "No GridCell assigned at position: " +
                x + ", " +
                y
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


        // Turn on new highlight.
        currentlySelectedCell.SetHighlight(true);
    }


    // =========================================================
    // BUILDING PLACEMENT
    // =========================================================

    public bool PlaceBuilding(int buildingIndex)
    {
        // -----------------------------------------------------
        // CHECK 1: Do we have a selected cell?
        // -----------------------------------------------------

        if (currentlySelectedCell == null)
        {
            Debug.LogWarning(
                "BoardManager: No cell is currently selected."
            );

            return false;
        }


        // -----------------------------------------------------
        // CHECK 2: Is the cell already occupied?
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // CHECK 3: Determine building type
        // -----------------------------------------------------

        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);


        if (buildingType == GridCell.BuildingType.None)
        {
            Debug.LogWarning(
                "BoardManager: Invalid building index " +
                buildingIndex
            );

            return false;
        }


        // -----------------------------------------------------
        // CHECK 4: Placement rules
        // -----------------------------------------------------

        if (!CanPlaceBuilding(
                currentlySelectedCell.X,
                currentlySelectedCell.Y,
                buildingType))
        {
            Debug.Log(
                "Cannot place " +
                buildingType +
                " at (" +
                currentlySelectedCell.X +
                ", " +
                currentlySelectedCell.Y +
                "). " +
                "Placement rules not satisfied."
            );

            return false;
        }


        // -----------------------------------------------------
        // CHECK 5: Get building sprite
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // PLACE BUILDING
        // -----------------------------------------------------

        currentlySelectedCell.SetBuilding(
            spriteToPlace,
            buildingType
        );


        // Remove the selection highlight after placement.
        currentlySelectedCell.SetHighlight(false);


        Debug.Log(
            "Placed " +
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
    // PLACEMENT RULES
    // =========================================================

    private bool CanPlaceBuilding(
        int x,
        int y,
        GridCell.BuildingType buildingType)
    {
        // =====================================================
        // BLUE
        // =====================================================
        //
        // Blue can be placed anywhere.
        //

        if (buildingType == GridCell.BuildingType.Blue)
        {
            return true;
        }


        // =====================================================
        // RED
        // =====================================================
        //
        // Red requires at least one Blue neighbour.
        //

        if (buildingType == GridCell.BuildingType.Red)
        {
            return HasNeighbour(
                x,
                y,
                GridCell.BuildingType.Blue
            );
        }


        // =====================================================
        // GREEN
        // =====================================================
        //
        // Green requires:
        // Blue + Red
        //

        if (buildingType == GridCell.BuildingType.Green)
        {
            bool hasBlue =
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Blue
                );


            bool hasRed =
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Red
                );


            return hasBlue && hasRed;
        }


        // =====================================================
        // YELLOW
        // =====================================================
        //
        // Yellow requires:
        // Blue + Red + Green
        //

        if (buildingType == GridCell.BuildingType.Yellow)
        {
            bool hasBlue =
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Blue
                );


            bool hasRed =
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Red
                );


            bool hasGreen =
                HasNeighbour(
                    x,
                    y,
                    GridCell.BuildingType.Green
                );


            return hasBlue &&
                   hasRed &&
                   hasGreen;
        }


        // Unknown building type.
        return false;
    }


    // =========================================================
    // NEIGHBOUR CHECKING
    // =========================================================

    private bool HasNeighbour(
        int x,
        int y,
        GridCell.BuildingType requiredType)
    {
        // -----------------------------------------------------
        // ABOVE
        // -----------------------------------------------------

        if (IsBuildingAt(
                x,
                y + 1,
                requiredType))
        {
            return true;
        }


        // -----------------------------------------------------
        // BELOW
        // -----------------------------------------------------

        if (IsBuildingAt(
                x,
                y - 1,
                requiredType))
        {
            return true;
        }


        // -----------------------------------------------------
        // LEFT
        // -----------------------------------------------------

        if (IsBuildingAt(
                x - 1,
                y,
                requiredType))
        {
            return true;
        }


        // -----------------------------------------------------
        // RIGHT
        // -----------------------------------------------------

        if (IsBuildingAt(
                x + 1,
                y,
                requiredType))
        {
            return true;
        }


        return false;
    }


    // =========================================================
    // CHECK SPECIFIC CELL
    // =========================================================

    private bool IsBuildingAt(
        int x,
        int y,
        GridCell.BuildingType requiredType)
    {
        // Outside the board means there is no building.
        if (x < 0 || x >= boardWidth ||
            y < 0 || y >= boardHeight)
        {
            return false;
        }


        if (grid == null)
        {
            return false;
        }


        GridCell cell = grid[x, y];


        // There may be no cell at these coordinates.
        if (cell == null)
        {
            return false;
        }


        return cell.CurrentBuilding == requiredType;
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
    // BUILDING SPRITE
    // =========================================================

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