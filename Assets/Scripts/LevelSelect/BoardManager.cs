using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Transform gridRoot;

    [Header("Building Sprites")]
    [SerializeField] private Sprite blueBuildingSprite;
    [SerializeField] private Sprite redBuildingSprite;
    [SerializeField] private Sprite greenBuildingSprite;
    [SerializeField] private Sprite yellowBuildingSprite;

    private Dictionary<Vector2Int, GridCell> gridCells =
        new Dictionary<Vector2Int, GridCell>();

    private GridCell currentlySelectedCell;

    private void Awake()
    {
        FindGridRoot();
        BuildGridReference();
    }

    private void FindGridRoot()
    {
        if (gridRoot != null)
        {
            return;
        }

        GameObject gridObject =
            GameObject.Find("Grid");

        if (gridObject != null)
        {
            gridRoot = gridObject.transform;

            Debug.Log(
                "BoardManager: Automatically found Grid object."
            );
        }
        else
        {
            Debug.LogError(
                "BoardManager: Could not find a Grid object. " +
                "Make sure your board parent is named 'Grid' " +
                "or assign Grid Root manually in the Inspector."
            );
        }
    }

    private void BuildGridReference()
    {
        gridCells.Clear();

        GridCell[] discoveredCells;

        if (gridRoot != null)
        {
            discoveredCells =
                gridRoot.GetComponentsInChildren<GridCell>(true);
        }
        else
        {
            discoveredCells =
                GetComponentsInChildren<GridCell>(true);
        }

        foreach (GridCell cell in discoveredCells)
        {
            if (cell == null)
            {
                continue;
            }

            Vector2Int position =
                new Vector2Int(cell.X, cell.Y);

            if (gridCells.ContainsKey(position))
            {
                Debug.LogWarning(
                    "BoardManager: Multiple GridCells use coordinates " +
                    position +
                    ". Cell " +
                    cell.name +
                    " will be ignored."
                );

                continue;
            }

            gridCells.Add(position, cell);
        }

        Debug.Log(
            "BoardManager: Found " +
            gridCells.Count +
            " GridCells."
        );
    }

    // =========================================================
    // CELL ACCESS
    // =========================================================

    public GridCell GetCell(int x, int y)
    {
        Vector2Int position =
            new Vector2Int(x, y);

        if (!gridCells.TryGetValue(
            position,
            out GridCell cell))
        {
            return null;
        }

        if (cell == null)
        {
            return null;
        }

        if (!cell.gameObject.activeInHierarchy)
        {
            return null;
        }

        return cell;
    }

    public GridCell GetSelectedCell()
    {
        return currentlySelectedCell;
    }

    public bool HasCell(int x, int y)
    {
        return GetCell(x, y) != null;
    }

    public IEnumerable<GridCell> GetAllActiveCells()
    {
        foreach (GridCell cell in gridCells.Values)
        {
            if (cell != null &&
                cell.gameObject.activeInHierarchy)
            {
                yield return cell;
            }
        }
    }

    // =========================================================
    // NAVIGATION
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

    public void SetSelectedCell(
        int x,
        int y)
    {
        GridCell cell =
            GetCell(x, y);

        if (cell == null)
        {
            Debug.Log(
                "BoardManager: No active GridCell at (" +
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
            return;
        }

        if (!cell.gameObject.activeInHierarchy)
        {
            return;
        }

        if (currentlySelectedCell != null)
        {
            currentlySelectedCell.SetHighlight(false);
        }

        currentlySelectedCell = cell;

        currentlySelectedCell.SetHighlight(true);

        // =====================================================
        // UPDATE CURRENT BUILDING SCORE
        // =====================================================

        UpdateSelectedBuildingScore();
    }

    private void UpdateSelectedBuildingScore()
    {
        if (BuildingScoreManager.Instance == null)
        {
            Debug.LogWarning(
                "BoardManager: BuildingScoreManager " +
                "instance was not found."
            );

            return;
        }

        if (currentlySelectedCell == null)
        {
            BuildingScoreManager.Instance.ClearBuildingScore();
            return;
        }

        if (!currentlySelectedCell.IsOccupied)
        {
            BuildingScoreManager.Instance.ClearBuildingScore();
            return;
        }

        int score =
            GridCell.GetBuildingScore(
                currentlySelectedCell.CurrentBuilding
            );

        BuildingScoreManager.Instance.SetBuildingScore(
            score
        );

        Debug.Log(
            "Selected Building Score: " +
            score +
            " | Building: " +
            currentlySelectedCell.CurrentBuilding +
            " | Cell: (" +
            currentlySelectedCell.X +
            ", " +
            currentlySelectedCell.Y +
            ")"
        );
    }

    // =========================================================
    // FIRST BOARD CELLS
    // =========================================================

    public GridCell GetFirstAvailableCell()
    {
        GridCell cellAtOrigin =
            GetCell(0, 0);

        if (cellAtOrigin != null &&
            !cellAtOrigin.IsOccupied)
        {
            return cellAtOrigin;
        }

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell != null &&
                cell.gameObject.activeInHierarchy &&
                !cell.IsOccupied)
            {
                return cell;
            }
        }

        return null;
    }

    public GridCell GetFirstBoardCell()
    {
        GridCell cellAtOrigin =
            GetCell(0, 0);

        if (cellAtOrigin != null)
        {
            return cellAtOrigin;
        }

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell != null &&
                cell.gameObject.activeInHierarchy)
            {
                return cell;
            }
        }

        return null;
    }

    // =========================================================
    // BUILDING PLACEMENT
    // =========================================================

    public bool CanPlaceBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null)
        {
            return false;
        }

        if (!currentlySelectedCell.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (currentlySelectedCell.IsOccupied)
        {
            return false;
        }

        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        return CanPlaceBuilding(
            currentlySelectedCell,
            buildingType
        );
    }

    private bool CanPlaceBuilding(
        GridCell cell,
        GridCell.BuildingType buildingType)
    {
        if (cell == null)
        {
            return false;
        }

        if (!cell.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (cell.IsOccupied)
        {
            return false;
        }

        return CanBuildingExistAtCell(
            cell,
            buildingType
        );
    }

    private bool CanBuildingExistAtCell(
        GridCell cell,
        GridCell.BuildingType buildingType)
    {
        if (cell == null)
        {
            return false;
        }

        switch (buildingType)
        {
            case GridCell.BuildingType.Blue:

                return true;

            case GridCell.BuildingType.Red:

                return HasNeighbourBuilding(
                    cell,
                    GridCell.BuildingType.Blue
                );

            case GridCell.BuildingType.Green:

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

            case GridCell.BuildingType.Yellow:

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

                return false;
        }
    }

    public bool HasValidPlacement(int buildingIndex)
    {
        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell == null)
            {
                continue;
            }

            if (!cell.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (CanPlaceBuilding(
                cell,
                buildingType))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasValidPlacementOrReplacement(
        int buildingIndex)
    {
        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        if (buildingType == GridCell.BuildingType.None)
        {
            return false;
        }

        // First check normal placement.
        if (HasValidPlacement(buildingIndex))
        {
            return true;
        }

        // Then check every occupied cell for replacement.
        foreach (GridCell cell in gridCells.Values)
        {
            if (cell == null)
            {
                continue;
            }

            if (!cell.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!cell.IsOccupied)
            {
                continue;
            }

            if (IsBoardValidAfterReplacement(
                cell,
                buildingType))
            {
                return true;
            }
        }

        return false;
    }

    public bool PlaceBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null)
        {
            Debug.LogWarning(
                "BoardManager: No cell is currently selected."
            );

            return false;
        }

        if (!CanPlaceBuilding(buildingIndex))
        {
            Debug.Log(
                "BoardManager: Building cannot be placed here."
            );

            return false;
        }

        Sprite spriteToPlace =
            GetBuildingSprite(buildingIndex);

        if (spriteToPlace == null)
        {
            Debug.LogWarning(
                "BoardManager: Building sprite is missing."
            );

            return false;
        }

        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        int population =
            GridCell.GetBuildingScore(buildingType);

        currentlySelectedCell.SetBuilding(
            spriteToPlace,
            buildingType,
            population
        );

        currentlySelectedCell.SetHighlight(false);

        // The building has just been placed, so update
        // the score associated with this cell.
        UpdateSelectedBuildingScore();

        Debug.Log(
            "BoardManager: " +
            buildingType +
            " placed at (" +
            currentlySelectedCell.X +
            ", " +
            currentlySelectedCell.Y +
            ")" +
            " | Population: " +
            population
        );

        return true;
    }

    // =========================================================
    // REPLACEMENT
    // =========================================================

    public bool CanReplaceBuilding(
        int buildingIndex)
    {
        if (currentlySelectedCell == null)
        {
            return false;
        }

        if (!currentlySelectedCell.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (!currentlySelectedCell.IsOccupied)
        {
            return false;
        }

        GridCell.BuildingType replacementType =
            GetBuildingType(buildingIndex);

        if (replacementType ==
            GridCell.BuildingType.None)
        {
            return false;
        }

        return IsBoardValidAfterReplacement(
            currentlySelectedCell,
            replacementType
        );
    }

    public bool CanPlaceOrReplaceBuilding(
        int buildingIndex)
    {
        if (currentlySelectedCell == null)
        {
            return false;
        }

        if (!currentlySelectedCell.IsOccupied)
        {
            return CanPlaceBuilding(buildingIndex);
        }

        return CanReplaceBuilding(buildingIndex);
    }

    private bool IsBoardValidAfterReplacement(
        GridCell replacementCell,
        GridCell.BuildingType replacementType)
    {
        if (replacementCell == null)
        {
            return false;
        }

        if (!replacementCell.IsOccupied)
        {
            return false;
        }

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell == null)
            {
                continue;
            }

            if (!cell.gameObject.activeInHierarchy)
            {
                continue;
            }

            GridCell.BuildingType resultingBuilding;

            if (cell == replacementCell)
            {
                resultingBuilding =
                    replacementType;
            }
            else
            {
                resultingBuilding =
                    cell.CurrentBuilding;
            }

            if (resultingBuilding ==
                GridCell.BuildingType.None)
            {
                continue;
            }

            if (!CanBuildingExistAfterReplacement(
                cell,
                resultingBuilding,
                replacementCell,
                replacementType))
            {
                return false;
            }
        }

        return true;
    }

    private bool CanBuildingExistAfterReplacement(
        GridCell cell,
        GridCell.BuildingType buildingType,
        GridCell replacementCell,
        GridCell.BuildingType replacementType)
    {
        if (cell == null)
        {
            return false;
        }

        switch (buildingType)
        {
            case GridCell.BuildingType.Blue:

                return true;

            case GridCell.BuildingType.Red:

                return HasNeighbourBuildingAfterReplacement(
                    cell,
                    GridCell.BuildingType.Blue,
                    replacementCell,
                    replacementType
                );

            case GridCell.BuildingType.Green:

                return
                    HasNeighbourBuildingAfterReplacement(
                        cell,
                        GridCell.BuildingType.Blue,
                        replacementCell,
                        replacementType
                    )
                    &&
                    HasNeighbourBuildingAfterReplacement(
                        cell,
                        GridCell.BuildingType.Red,
                        replacementCell,
                        replacementType
                    );

            case GridCell.BuildingType.Yellow:

                return
                    HasNeighbourBuildingAfterReplacement(
                        cell,
                        GridCell.BuildingType.Blue,
                        replacementCell,
                        replacementType
                    )
                    &&
                    HasNeighbourBuildingAfterReplacement(
                        cell,
                        GridCell.BuildingType.Red,
                        replacementCell,
                        replacementType
                    )
                    &&
                    HasNeighbourBuildingAfterReplacement(
                        cell,
                        GridCell.BuildingType.Green,
                        replacementCell,
                        replacementType
                    );

            default:

                return false;
        }
    }

    private bool HasNeighbourBuildingAfterReplacement(
        GridCell cell,
        GridCell.BuildingType requiredBuilding,
        GridCell replacementCell,
        GridCell.BuildingType replacementType)
    {
        if (cell == null)
        {
            return false;
        }

        GridCell neighbour;

        // Right
        neighbour =
            GetCell(
                cell.X + 1,
                cell.Y
            );

        if (IsNeighbourBuildingTypeAfterReplacement(
            neighbour,
            requiredBuilding,
            replacementCell,
            replacementType))
        {
            return true;
        }

        // Left
        neighbour =
            GetCell(
                cell.X - 1,
                cell.Y
            );

        if (IsNeighbourBuildingTypeAfterReplacement(
            neighbour,
            requiredBuilding,
            replacementCell,
            replacementType))
        {
            return true;
        }

        // Up
        neighbour =
            GetCell(
                cell.X,
                cell.Y + 1
            );

        if (IsNeighbourBuildingTypeAfterReplacement(
            neighbour,
            requiredBuilding,
            replacementCell,
            replacementType))
        {
            return true;
        }

        // Down
        neighbour =
            GetCell(
                cell.X,
                cell.Y - 1
            );

        if (IsNeighbourBuildingTypeAfterReplacement(
            neighbour,
            requiredBuilding,
            replacementCell,
            replacementType))
        {
            return true;
        }

        return false;
    }

    private bool IsNeighbourBuildingTypeAfterReplacement(
        GridCell neighbour,
        GridCell.BuildingType requiredBuilding,
        GridCell replacementCell,
        GridCell.BuildingType replacementType)
    {
        if (neighbour == null)
        {
            return false;
        }

        if (!neighbour.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (neighbour == replacementCell)
        {
            return replacementType ==
                   requiredBuilding;
        }

        return neighbour.CurrentBuilding ==
               requiredBuilding;
    }

    public bool ReplaceBuilding(int buildingIndex)
    {
        if (!CanReplaceBuilding(buildingIndex))
        {
            Debug.Log(
                "BoardManager: Building cannot replace " +
                "the building at the selected cell."
            );

            return false;
        }

        Sprite spriteToPlace =
            GetBuildingSprite(buildingIndex);

        if (spriteToPlace == null)
        {
            Debug.LogWarning(
                "BoardManager: Building sprite is missing."
            );

            return false;
        }

        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        GridCell.BuildingType oldBuildingType =
            currentlySelectedCell.CurrentBuilding;

        int oldPopulation =
            currentlySelectedCell.BuildingPopulation;

        int population =
            GridCell.GetBuildingScore(buildingType);

        currentlySelectedCell.SetBuilding(
            spriteToPlace,
            buildingType,
            population
        );

        currentlySelectedCell.SetHighlight(false);

        // Update the score after replacing the building.
        UpdateSelectedBuildingScore();

        Debug.Log(
            "BoardManager: Replaced " +
            oldBuildingType +
            " with " +
            buildingType +
            " at (" +
            currentlySelectedCell.X +
            ", " +
            currentlySelectedCell.Y +
            ")" +
            " | Old Population: " +
            oldPopulation +
            " | New Population: " +
            population
        );

        return true;
    }

    // =========================================================
    // SELECTED CELL INFORMATION
    // =========================================================

    public int GetSelectedCellPopulation()
    {
        if (currentlySelectedCell == null)
        {
            return 0;
        }

        return currentlySelectedCell.BuildingPopulation;
    }

    public GridCell.BuildingType GetSelectedCellBuildingType()
    {
        if (currentlySelectedCell == null)
        {
            return GridCell.BuildingType.None;
        }

        return currentlySelectedCell.CurrentBuilding;
    }

    public bool IsSelectedCellOccupied()
    {
        return
            currentlySelectedCell != null &&
            currentlySelectedCell.IsOccupied;
    }

    // =========================================================
    // BUILDING COUNTS
    // =========================================================

    public int GetBuildingCount(
        GridCell.BuildingType buildingType)
    {
        int count = 0;

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell == null)
            {
                continue;
            }

            if (!cell.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (cell.CurrentBuilding ==
                buildingType)
            {
                count++;
            }
        }

        return count;
    }

    public int GetBlueCount()
    {
        return GetBuildingCount(
            GridCell.BuildingType.Blue
        );
    }

    public int GetRedCount()
    {
        return GetBuildingCount(
            GridCell.BuildingType.Red
        );
    }

    public int GetGreenCount()
    {
        return GetBuildingCount(
            GridCell.BuildingType.Green
        );
    }

    public int GetYellowCount()
    {
        return GetBuildingCount(
            GridCell.BuildingType.Yellow
        );
    }

    public int GetTotalBuildingCount()
    {
        return
            GetBlueCount() +
            GetRedCount() +
            GetGreenCount() +
            GetYellowCount();
    }

    // =========================================================
    // POPULATION
    // =========================================================

    public int GetPopulation(
        GridCell.BuildingType buildingType)
    {
        int population = 0;

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell == null)
            {
                continue;
            }

            if (!cell.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (cell.CurrentBuilding ==
                buildingType)
            {
                population +=
                    cell.BuildingPopulation;
            }
        }

        return population;
    }

    public int GetBluePopulation()
    {
        return GetPopulation(
            GridCell.BuildingType.Blue
        );
    }

    public int GetRedPopulation()
    {
        return GetPopulation(
            GridCell.BuildingType.Red
        );
    }

    public int GetGreenPopulation()
    {
        return GetPopulation(
            GridCell.BuildingType.Green
        );
    }

    public int GetYellowPopulation()
    {
        return GetPopulation(
            GridCell.BuildingType.Yellow
        );
    }

    public int GetTotalPopulation()
    {
        return
            GetBluePopulation() +
            GetRedPopulation() +
            GetGreenPopulation() +
            GetYellowPopulation();
    }

    // =========================================================
    // RESET / CLEAR
    // =========================================================

    public void ClearAllBuildings()
    {
        foreach (GridCell cell in gridCells.Values)
        {
            if (cell == null)
            {
                continue;
            }

            cell.ClearBuilding();
            cell.SetHighlight(false);
        }

        currentlySelectedCell = null;

        if (BuildingScoreManager.Instance != null)
        {
            BuildingScoreManager.Instance.ClearBuildingScore();
        }

        Debug.Log(
            "BoardManager: All buildings cleared."
        );
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private bool HasNeighbourBuilding(
        GridCell cell,
        GridCell.BuildingType buildingType)
    {
        if (cell == null)
        {
            return false;
        }

        GridCell neighbour;

        // Right
        neighbour =
            GetCell(
                cell.X + 1,
                cell.Y
            );

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        // Left
        neighbour =
            GetCell(
                cell.X - 1,
                cell.Y
            );

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        // Up
        neighbour =
            GetCell(
                cell.X,
                cell.Y + 1
            );

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        // Down
        neighbour =
            GetCell(
                cell.X,
                cell.Y - 1
            );

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        return false;
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
                return null;
        }
    }

    [ContextMenu("Print Building Counts")]
    private void PrintBuildingCounts()
    {
        Debug.Log(
            "BUILDINGS | " +
            "Blue: " + GetBlueCount() +
            " | Red: " + GetRedCount() +
            " | Green: " + GetGreenCount() +
            " | Yellow: " + GetYellowCount() +
            " | Total: " + GetTotalBuildingCount()
        );

        Debug.Log(
            "POPULATION | " +
            "Blue: " + GetBluePopulation() +
            " | Red: " + GetRedPopulation() +
            " | Green: " + GetGreenPopulation() +
            " | Yellow: " + GetYellowPopulation() +
            " | Total: " + GetTotalPopulation()
        );
    }
}
