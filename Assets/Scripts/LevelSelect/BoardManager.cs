using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Building Sprites")]
    [SerializeField] private Sprite blueBuildingSprite;
    [SerializeField] private Sprite redBuildingSprite;
    [SerializeField] private Sprite greenBuildingSprite;
    [SerializeField] private Sprite yellowBuildingSprite;

    [Header("Placement Rules")]
    [SerializeField] private bool usePlacementRules = true;

    // All actual GridCells found in the scene.
    private Dictionary<Vector2Int, GridCell> grid =
        new Dictionary<Vector2Int, GridCell>();

    private GridCell currentlySelectedCell;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        BuildGridReference();
    }


    private void BuildGridReference()
    {
        grid.Clear();

        GridCell[] cells =
            FindObjectsByType<GridCell>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        if (cells == null || cells.Length == 0)
        {
            Debug.LogWarning(
                "BoardManager: No GridCells were found in the scene."
            );

            return;
        }

        foreach (GridCell cell in cells)
        {
            if (cell == null)
                continue;

            Vector2Int position =
                new Vector2Int(cell.X, cell.Y);

            if (grid.ContainsKey(position))
            {
                Debug.LogWarning(
                    "BoardManager: Duplicate GridCell coordinates found at (" +
                    cell.X +
                    ", " +
                    cell.Y +
                    "). Object: " +
                    cell.name
                );

                continue;
            }

            grid.Add(position, cell);
        }

        Debug.Log(
            "BoardManager: Automatically found " +
            grid.Count +
            " GridCells."
        );
    }


    // =========================================================
    // CELL SELECTION
    // =========================================================

    public void SetSelectedCell(int x, int y)
    {
        GridCell cell = GetCell(x, y);

        if (cell == null)
        {
            Debug.LogWarning(
                "BoardManager: No GridCell exists at (" +
                x +
                ", " +
                y +
                ")."
            );

            return;
        }

        SetSelectedCell(cell);
    }


    public void SetSelectedCell(GridCell cell)
    {
        if (cell == null)
        {
            Debug.LogWarning(
                "BoardManager: Tried to select a null GridCell."
            );

            return;
        }

        if (currentlySelectedCell != null &&
            currentlySelectedCell != cell)
        {
            currentlySelectedCell.SetHighlight(false);
        }

        currentlySelectedCell = cell;

        currentlySelectedCell.SetHighlight(true);

        Debug.Log(
            "Selected Cell: (" +
            cell.X +
            ", " +
            cell.Y +
            ")"
        );
    }


    public GridCell GetSelectedCell()
    {
        return currentlySelectedCell;
    }


    // =========================================================
    // GET CELL
    // =========================================================

    public GridCell GetCell(int x, int y)
    {
        Vector2Int position =
            new Vector2Int(x, y);

        if (grid.TryGetValue(position, out GridCell cell))
        {
            return cell;
        }

        return null;
    }


    public bool HasCell(int x, int y)
    {
        return GetCell(x, y) != null;
    }


    // =========================================================
    // NEIGHBOUR NAVIGATION
    // =========================================================

    public GridCell GetNeighbour(
        GridCell currentCell,
        int directionX,
        int directionY)
    {
        if (currentCell == null)
        {
            return null;
        }

        int targetX =
            currentCell.X + directionX;

        int targetY =
            currentCell.Y + directionY;

        return GetCell(targetX, targetY);
    }


    // =========================================================
    // FIND FIRST AVAILABLE CELL
    // =========================================================

    public GridCell GetFirstAvailableCell()
    {
        GridCell[] cells =
            FindObjectsByType<GridCell>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        GridCell firstAvailable = null;

        foreach (GridCell cell in cells)
        {
            if (cell == null)
                continue;

            if (cell.IsOccupied)
                continue;

            if (firstAvailable == null)
            {
                firstAvailable = cell;
                continue;
            }

            // Prefer lower Y first.
            if (cell.Y < firstAvailable.Y)
            {
                firstAvailable = cell;
            }
            else if (
                cell.Y == firstAvailable.Y &&
                cell.X < firstAvailable.X)
            {
                firstAvailable = cell;
            }
        }

        return firstAvailable;
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

        return PlaceBuilding(
            currentlySelectedCell,
            buildingIndex
        );
    }


    public bool PlaceBuilding(
        GridCell cell,
        int buildingIndex)
    {
        if (cell == null)
        {
            Debug.LogWarning(
                "BoardManager: Cannot place building on null cell."
            );

            return false;
        }

        if (cell.IsOccupied)
        {
            Debug.Log(
                "Cannot place building. Cell (" +
                cell.X +
                ", " +
                cell.Y +
                ") is already occupied."
            );

            return false;
        }

        if (!CanPlaceBuilding(cell, buildingIndex))
        {
            Debug.Log(
                "Building " +
                GetBuildingName(buildingIndex) +
                " cannot be placed on Cell (" +
                cell.X +
                ", " +
                cell.Y +
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

        cell.SetBuilding(
            spriteToPlace,
            buildingType
        );

        cell.SetHighlight(false);

        Debug.Log(
            "Placed " +
            GetBuildingName(buildingIndex) +
            " at (" +
            cell.X +
            ", " +
            cell.Y +
            ")"
        );

        return true;
    }


    // =========================================================
    // PLACEMENT VALIDATION
    // =========================================================

    public bool CanPlaceBuilding(int buildingIndex)
    {
        if (!usePlacementRules)
        {
            return GetFirstAvailableCell() != null;
        }

        foreach (GridCell cell in grid.Values)
        {
            if (cell == null)
                continue;

            if (CanPlaceBuilding(cell, buildingIndex))
            {
                return true;
            }
        }

        return false;
    }


    public bool CanPlaceBuilding(
        GridCell cell,
        int buildingIndex)
    {
        if (cell == null)
            return false;

        if (cell.IsOccupied)
            return false;

        if (!usePlacementRules)
            return true;

        switch (buildingIndex)
        {
            // -------------------------------------------------
            // BLUE
            // -------------------------------------------------

            case 0:
                // Blue can be placed anywhere.
                return true;


            // -------------------------------------------------
            // RED
            // -------------------------------------------------

            case 1:
                // Red requires a Blue neighbour.
                return HasNeighbourBuilding(
                    cell,
                    GridCell.BuildingType.Blue
                );


            // -------------------------------------------------
            // GREEN
            // -------------------------------------------------

            case 2:
                // Green requires BOTH Blue and Red neighbours.
                return
                    HasNeighbourBuilding(
                        cell,
                        GridCell.BuildingType.Blue
                    )
                    &&
                    HasNeighbourBuilding(
                        cell,
                        GridCell.BuildingType.Red
                    );


            // -------------------------------------------------
            // YELLOW
            // -------------------------------------------------

            case 3:
                // Yellow requires Blue, Red and Green neighbours.
                return
                    HasNeighbourBuilding(
                        cell,
                        GridCell.BuildingType.Blue
                    )
                    &&
                    HasNeighbourBuilding(
                        cell,
                        GridCell.BuildingType.Red
                    )
                    &&
                    HasNeighbourBuilding(
                        cell,
                        GridCell.BuildingType.Green
                    );


            default:
                Debug.LogWarning(
                    "BoardManager: Invalid building index " +
                    buildingIndex
                );

                return false;
        }
    }


    // =========================================================
    // NEIGHBOUR BUILDING CHECK
    // =========================================================

    private bool HasNeighbourBuilding(
        GridCell cell,
        GridCell.BuildingType buildingType)
    {
        if (cell == null)
            return false;

        // UP
        GridCell up =
            GetCell(
                cell.X,
                cell.Y - 1
            );

        if (up != null &&
            up.CurrentBuilding == buildingType)
        {
            return true;
        }


        // DOWN
        GridCell down =
            GetCell(
                cell.X,
                cell.Y + 1
            );

        if (down != null &&
            down.CurrentBuilding == buildingType)
        {
            return true;
        }


        // LEFT
        GridCell left =
            GetCell(
                cell.X - 1,
                cell.Y
            );

        if (left != null &&
            left.CurrentBuilding == buildingType)
        {
            return true;
        }


        // RIGHT
        GridCell right =
            GetCell(
                cell.X + 1,
                cell.Y
            );

        if (right != null &&
            right.CurrentBuilding == buildingType)
        {
            return true;
        }


        return false;
    }


    // =========================================================
    // BUILDING INFORMATION
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
                return null;
        }
    }


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
}