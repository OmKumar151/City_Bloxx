
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Transform gridRoot;

    [Header("Placement Glow")]
    [SerializeField] private float placementGlowPadding = 6f;
    [SerializeField] private float placementGlowIntensity = 0.7f;
    [SerializeField] private float placementIndicatorSize = 88f;
    [SerializeField] private float placementPreviewAlpha = 0.55f;

    [SerializeField]
    private Color bluePlacementColor =
        new Color(0.15f, 0.55f, 1f, 0.85f);

    [SerializeField]
    private Color redPlacementColor =
        new Color(1f, 0.2f, 0.2f, 0.85f);

    [SerializeField]
    private Color greenPlacementColor =
        new Color(0.2f, 0.9f, 0.25f, 0.85f);

    [SerializeField]
    private Color yellowPlacementColor =
        new Color(1f, 0.85f, 0.1f, 0.9f);

    [Header("Building Sprites")]
    [SerializeField] private Sprite blueBuildingSprite;
    [SerializeField] private Sprite redBuildingSprite;
    [SerializeField] private Sprite greenBuildingSprite;
    [SerializeField] private Sprite yellowBuildingSprite;

    private Dictionary<Vector2Int, GridCell> gridCells =
        new Dictionary<Vector2Int, GridCell>();

    private GridCell currentlySelectedCell;
    private int currentPlacementBuildingIndex = -1;
    private bool placementModeActive = false;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        placementModeActive = false;
        currentPlacementBuildingIndex = -1;

        FindGridRoot();
        BuildGridReference();

        ClearPlacementHighlights();
    }


    // =========================================================
    // EXPLICIT PLACEMENT MODE
    // =========================================================

    public bool IsPlacementModeActive()
    {
        return placementModeActive;
    }


    /// <summary>
    /// Starts board placement mode and chooses the first valid
    /// cell for the newly selected building.
    /// </summary>
    public void BeginPlacementMode(int buildingIndex)
    {
        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        if (buildingType == GridCell.BuildingType.None)
        {
            Debug.LogWarning(
                "BoardManager: Cannot begin placement mode. " +
                "Invalid building index: " + buildingIndex
            );

            EndPlacementMode();
            return;
        }

        placementModeActive = true;
        currentPlacementBuildingIndex = buildingIndex;

        // Refresh the grid before looking for a starting cell.
        RefreshBoardAnalysis();

        // IMPORTANT:
        // A newly selected building starts at its first valid
        // placement or replacement cell, rather than reusing
        // the cell selected for the previous building.
        GridCell firstValidCell =
            FindFirstValidPlacementCell(buildingType);

        if (firstValidCell != null)
        {
            currentlySelectedCell = firstValidCell;
        }
        else if (currentlySelectedCell == null ||
                 !currentlySelectedCell.gameObject.activeInHierarchy)
        {
            currentlySelectedCell = GetFirstBoardCell();
        }

        // Draw the highlights and preview at the chosen cell.
        // ShowPlacementHighlights does not reset a valid,
        // already selected cursor, so movement remains intact.
        ShowPlacementHighlights(buildingIndex);

        Debug.Log(
            "BoardManager: Placement Mode STARTED for " +
            buildingType
        );
    }


    /// <summary>
    /// Completely exits board placement mode.
    /// Removes every glow, cursor and building preview.
    /// </summary>
    public void EndPlacementMode()
    {
        placementModeActive = false;
        currentPlacementBuildingIndex = -1;

        foreach (GridCell cell in GetAllActiveCells())
        {
            if (cell != null)
            {
                cell.SetHighlight(false);
                cell.HidePlacementIndicator();
            }
        }

        Debug.Log(
            "BoardManager: Placement Mode ENDED. " +
            "All placement visuals cleared."
        );
    }


    // =========================================================
    // GRID DISCOVERY
    // =========================================================

    private void FindGridRoot()
    {
        if (gridRoot != null)
        {
            return;
        }

        GameObject gridObject = GameObject.Find("Grid");

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
                    position + ". Cell " + cell.name +
                    " will be ignored."
                );

                continue;
            }

            gridCells.Add(position, cell);
        }

        Debug.Log(
            "BoardManager: Found " + gridCells.Count + " GridCells."
        );
    }


    // =========================================================
    // PUBLIC BOARD REFRESH
    // =========================================================

    public void RefreshBoardAnalysis()
    {
        BuildGridReference();

        Debug.Log(
            "BoardManager: Board analysis refreshed. " +
            "Current occupied cells: " + GetTotalBuildingCount()
        );
    }


    // =========================================================
    // CELL ACCESS
    // =========================================================

    public GridCell GetCell(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);

        if (!gridCells.TryGetValue(position, out GridCell cell))
        {
            return null;
        }

        if (cell == null || !cell.gameObject.activeInHierarchy)
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
            if (cell != null && cell.gameObject.activeInHierarchy)
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

        int targetX = currentCell.X + directionX;
        int targetY = currentCell.Y + directionY;

        return GetCell(targetX, targetY);
    }


    public void SetSelectedCell(int x, int y)
    {
        GridCell cell = GetCell(x, y);

        if (cell == null)
        {
            Debug.Log(
                "BoardManager: No active GridCell at (" +
                x + ", " + y + ")."
            );

            return;
        }

        SetSelectedCell(cell);
    }


    public void SetSelectedCell(GridCell cell)
    {
        if (cell == null || !cell.gameObject.activeInHierarchy)
        {
            return;
        }

        if (currentlySelectedCell != null &&
            currentlySelectedCell != cell)
        {
            currentlySelectedCell.SetHighlight(false);
            currentlySelectedCell.HidePlacementIndicator();
        }

        currentlySelectedCell = cell;

        UpdateSelectedBuildingScore();

        // Refresh visuals without changing the cursor position.
        if (placementModeActive &&
            currentPlacementBuildingIndex >= 0)
        {
            ShowPlacementHighlights(currentPlacementBuildingIndex);
        }
        else
        {
            currentlySelectedCell.SetHighlight(false);
            currentlySelectedCell.HidePlacementIndicator();
        }
    }


    private void UpdateSelectedBuildingScore()
    {
        if (BuildingScoreManager.Instance == null)
        {
            return;
        }

        if (currentlySelectedCell == null ||
            !currentlySelectedCell.IsOccupied)
        {
            BuildingScoreManager.Instance.ClearExistingBuildingScore();
            return;
        }

        int score =
            GridCell.GetBuildingScore(currentlySelectedCell.CurrentBuilding);

        BuildingScoreManager.Instance.SetExistingBuildingScore(score);

        Debug.Log(
            "Selected Existing Building Score: " + score +
            " | Building: " + currentlySelectedCell.CurrentBuilding +
            " | Cell: (" + currentlySelectedCell.X +
            ", " + currentlySelectedCell.Y + ")"
        );
    }


    // =========================================================
    // FIRST BOARD CELLS
    // =========================================================

    public GridCell GetFirstAvailableCell()
    {
        GridCell cellAtOrigin = GetCell(0, 0);

        if (cellAtOrigin != null && !cellAtOrigin.IsOccupied)
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
        GridCell cellAtOrigin = GetCell(0, 0);

        if (cellAtOrigin != null)
        {
            return cellAtOrigin;
        }

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell != null && cell.gameObject.activeInHierarchy)
            {
                return cell;
            }
        }

        return null;
    }


    // =========================================================
    // PLACEMENT HIGHLIGHTS
    // =========================================================

    public void ShowPlacementHighlights(int buildingIndex)
    {
        // Never show placement visuals outside placement mode.
        if (!placementModeActive)
        {
            ClearPlacementHighlights();

            Debug.Log(
                "BoardManager: Ignored placement highlight request " +
                "because Placement Mode is not active."
            );

            return;
        }

        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        if (buildingType == GridCell.BuildingType.None)
        {
            ClearPlacementHighlights();
            return;
        }

        currentPlacementBuildingIndex = buildingIndex;

        RefreshBoardAnalysis();

        // Only select a starting cell if the current selection
        // is missing or inactive. Normal cursor movement is
        // therefore not reset by highlight refreshes.
        if (currentlySelectedCell == null ||
            !currentlySelectedCell.gameObject.activeInHierarchy)
        {
            currentlySelectedCell =
                FindFirstValidPlacementCell(buildingType);

            if (currentlySelectedCell == null)
            {
                currentlySelectedCell = GetFirstBoardCell();
            }
        }

        Color glowColor = GetPlacementGlowColor(buildingType);
        Sprite buildingSprite = GetBuildingSprite(buildingIndex);

        foreach (GridCell cell in GetAllActiveCells())
        {
            if (cell == null)
            {
                continue;
            }

            bool validPlacement;

            if (cell.IsOccupied)
            {
                validPlacement =
                    CanReplaceBuildingAtCell(cell, buildingType);
            }
            else
            {
                validPlacement =
                    CanPlaceBuilding(cell, buildingType);
            }

            cell.SetPlacementHighlight(
                validPlacement,
                glowColor,
                placementGlowPadding,
                placementGlowIntensity
            );

            cell.HidePlacementIndicator();
        }

        // Display the preview at the currently selected cell.
        if (currentlySelectedCell != null &&
            currentlySelectedCell.gameObject.activeInHierarchy)
        {
            bool selectedCellCanPlace;

            if (currentlySelectedCell.IsOccupied)
            {
                selectedCellCanPlace =
                    CanReplaceBuildingAtCell(
                        currentlySelectedCell,
                        buildingType
                    );
            }
            else
            {
                selectedCellCanPlace =
                    CanPlaceBuilding(
                        currentlySelectedCell,
                        buildingType
                    );
            }

            Color indicatorColor = glowColor;

            if (!selectedCellCanPlace)
            {
                indicatorColor = new Color(1f, 0.25f, 0.25f, 0.95f);
            }

            currentlySelectedCell.SetPlacementIndicator(
                true,
                buildingSprite,
                indicatorColor,
                placementIndicatorSize,
                placementPreviewAlpha
            );
        }
    }


    private GridCell FindFirstValidPlacementCell(
        GridCell.BuildingType buildingType)
    {
        foreach (GridCell cell in GetAllActiveCells())
        {
            if (cell == null)
            {
                continue;
            }

            bool valid;

            if (cell.IsOccupied)
            {
                valid = CanReplaceBuildingAtCell(cell, buildingType);
            }
            else
            {
                valid = CanPlaceBuilding(cell, buildingType);
            }

            if (valid)
            {
                return cell;
            }
        }

        return null;
    }


    public void ClearPlacementHighlights()
    {
        currentPlacementBuildingIndex = -1;

        foreach (GridCell cell in GetAllActiveCells())
        {
            if (cell != null)
            {
                cell.SetHighlight(false);
                cell.HidePlacementIndicator();
            }
        }
    }


    private Color GetPlacementGlowColor(
        GridCell.BuildingType buildingType)
    {
        switch (buildingType)
        {
            case GridCell.BuildingType.Blue:
                return bluePlacementColor;

            case GridCell.BuildingType.Red:
                return redPlacementColor;

            case GridCell.BuildingType.Green:
                return greenPlacementColor;

            case GridCell.BuildingType.Yellow:
                return yellowPlacementColor;

            default:
                return Color.white;
        }
    }


    // =========================================================
    // NORMAL PLACEMENT
    // =========================================================

    public bool CanPlaceBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null ||
            !currentlySelectedCell.gameObject.activeInHierarchy ||
            currentlySelectedCell.IsOccupied)
        {
            return false;
        }

        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        return CanPlaceBuilding(currentlySelectedCell, buildingType);
    }


    private bool CanPlaceBuilding(
        GridCell cell,
        GridCell.BuildingType buildingType)
    {
        if (cell == null ||
            !cell.gameObject.activeInHierarchy ||
            cell.IsOccupied)
        {
            return false;
        }

        return CanBuildingExistAtCell(cell, buildingType);
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
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Blue) &&
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Red);

            case GridCell.BuildingType.Yellow:
                return
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Blue) &&
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Red) &&
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Green);

            default:
                return false;
        }
    }


    public bool HasValidPlacement(int buildingIndex)
    {
        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        foreach (GridCell cell in GetAllActiveCells())
        {
            if (CanPlaceBuilding(cell, buildingType))
            {
                return true;
            }
        }

        return false;
    }


    // =========================================================
    // PLACEMENT OR REPLACEMENT
    // =========================================================

    public bool HasValidPlacementOrReplacement(int buildingIndex)
    {
        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        if (buildingType == GridCell.BuildingType.None)
        {
            return false;
        }

        RefreshBoardAnalysis();

        if (HasValidPlacement(buildingIndex))
        {
            Debug.Log(
                "BoardManager: Valid empty-cell placement exists for " +
                buildingType
            );

            return true;
        }

        foreach (GridCell cell in GetAllActiveCells())
        {
            if (!cell.IsOccupied)
            {
                continue;
            }

            if (CanReplaceBuildingAtCell(cell, buildingType))
            {
                Debug.Log(
                    "BoardManager: Valid replacement exists. Replace " +
                    cell.CurrentBuilding + " at (" +
                    cell.X + ", " + cell.Y + ") with " +
                    buildingType
                );

                return true;
            }
        }

        Debug.Log(
            "BoardManager: No valid placement OR replacement exists for " +
            buildingType
        );

        return false;
    }


    // =========================================================
    // NORMAL PLACEMENT EXECUTION
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

        if (!CanPlaceBuilding(buildingIndex))
        {
            Debug.Log(
                "BoardManager: Building cannot be placed here."
            );

            return false;
        }

        Sprite spriteToPlace = GetBuildingSprite(buildingIndex);

        if (spriteToPlace == null)
        {
            Debug.LogWarning(
                "BoardManager: Building sprite is missing."
            );

            return false;
        }

        GridCell.BuildingType buildingType =
            GetBuildingType(buildingIndex);

        int population = GridCell.GetBuildingScore(buildingType);

        currentlySelectedCell.SetBuilding(
            spriteToPlace,
            buildingType,
            population
        );

        EndPlacementMode();
        UpdateSelectedBuildingScore();

        Debug.Log(
            "BoardManager: " + buildingType +
            " placed at (" + currentlySelectedCell.X +
            ", " + currentlySelectedCell.Y + ")" +
            " | Population: " + population
        );

        return true;
    }


    // =========================================================
    // REPLACEMENT
    // =========================================================

    public bool CanReplaceBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null ||
            !currentlySelectedCell.gameObject.activeInHierarchy ||
            !currentlySelectedCell.IsOccupied)
        {
            return false;
        }

        GridCell.BuildingType replacementType =
            GetBuildingType(buildingIndex);

        if (replacementType == GridCell.BuildingType.None)
        {
            return false;
        }

        RefreshBoardAnalysis();

        return CanReplaceBuildingAtCell(
            currentlySelectedCell,
            replacementType
        );
    }


    public bool CanPlaceOrReplaceBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null ||
            !currentlySelectedCell.gameObject.activeInHierarchy)
        {
            return false;
        }

        RefreshBoardAnalysis();

        if (!currentlySelectedCell.IsOccupied)
        {
            return CanPlaceBuilding(buildingIndex);
        }

        return CanReplaceBuilding(buildingIndex);
    }


    // =========================================================
    // REPLACEMENT VALIDATION
    // =========================================================

    private bool CanReplaceBuildingAtCell(
        GridCell replacementCell,
        GridCell.BuildingType replacementType)
    {
        if (replacementCell == null ||
            !replacementCell.gameObject.activeInHierarchy ||
            !replacementCell.IsOccupied ||
            replacementType == GridCell.BuildingType.None)
        {
            return false;
        }

        return CanBuildingExistForReplacement(
            replacementCell,
            replacementType
        );
    }


    private bool CanBuildingExistForReplacement(
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
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Blue) &&
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Red);

            case GridCell.BuildingType.Yellow:
                return
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Blue) &&
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Red) &&
                    HasNeighbourBuilding(cell, GridCell.BuildingType.Green);

            default:
                return false;
        }
    }


    // =========================================================
    // REPLACEMENT EXECUTION
    // =========================================================

    public bool ReplaceBuilding(int buildingIndex)
    {
        if (currentlySelectedCell == null)
        {
            Debug.LogWarning(
                "BoardManager: No cell is currently selected."
            );

            return false;
        }

        if (!currentlySelectedCell.IsOccupied)
        {
            Debug.LogWarning(
                "BoardManager: Selected cell is empty. " +
                "Replacement cannot be performed."
            );

            return false;
        }

        GridCell.BuildingType replacementType =
            GetBuildingType(buildingIndex);

        if (replacementType == GridCell.BuildingType.None)
        {
            return false;
        }

        if (!CanReplaceBuildingAtCell(
            currentlySelectedCell,
            replacementType))
        {
            Debug.Log(
                "BoardManager: " + replacementType +
                " cannot replace " + currentlySelectedCell.CurrentBuilding +
                " at (" + currentlySelectedCell.X +
                ", " + currentlySelectedCell.Y +
                ") because the NEW building's " +
                "placement requirements are not satisfied."
            );

            return false;
        }

        Sprite spriteToPlace = GetBuildingSprite(buildingIndex);

        if (spriteToPlace == null)
        {
            Debug.LogWarning(
                "BoardManager: Building sprite is missing."
            );

            return false;
        }

        GridCell.BuildingType oldBuildingType =
            currentlySelectedCell.CurrentBuilding;

        int oldPopulation =
            currentlySelectedCell.BuildingPopulation;

        int newPopulation =
            GridCell.GetBuildingScore(replacementType);

        currentlySelectedCell.SetBuilding(
            spriteToPlace,
            replacementType,
            newPopulation
        );

        EndPlacementMode();
        UpdateSelectedBuildingScore();

        Debug.Log(
            "BoardManager: Replaced " + oldBuildingType +
            " with " + replacementType +
            " at (" + currentlySelectedCell.X +
            ", " + currentlySelectedCell.Y + ")" +
            " | Old Population: " + oldPopulation +
            " | New Population: " + newPopulation
        );

        return true;
    }


    // =========================================================
    // PLACEMENT HIGHLIGHT REFRESH
    // =========================================================

    public void RefreshPlacementHighlights(int buildingIndex)
    {
        if (!placementModeActive)
        {
            ClearPlacementHighlights();
            return;
        }

        ShowPlacementHighlights(buildingIndex);
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
        return currentlySelectedCell != null &&
               currentlySelectedCell.IsOccupied;
    }


    // =========================================================
    // BUILDING COUNTS
    // =========================================================

    public int GetBuildingCount(GridCell.BuildingType buildingType)
    {
        int count = 0;

        foreach (GridCell cell in GetAllActiveCells())
        {
            if (cell.CurrentBuilding == buildingType)
            {
                count++;
            }
        }

        return count;
    }


    public int GetBlueCount()
    {
        return GetBuildingCount(GridCell.BuildingType.Blue);
    }


    public int GetRedCount()
    {
        return GetBuildingCount(GridCell.BuildingType.Red);
    }


    public int GetGreenCount()
    {
        return GetBuildingCount(GridCell.BuildingType.Green);
    }


    public int GetYellowCount()
    {
        return GetBuildingCount(GridCell.BuildingType.Yellow);
    }


    public int GetTotalBuildingCount()
    {
        return GetBlueCount() +
               GetRedCount() +
               GetGreenCount() +
               GetYellowCount();
    }


    // =========================================================
    // POPULATION
    // =========================================================

    public int GetPopulation(GridCell.BuildingType buildingType)
    {
        int population = 0;

        foreach (GridCell cell in GetAllActiveCells())
        {
            if (cell.CurrentBuilding == buildingType)
            {
                population += cell.BuildingPopulation;
            }
        }

        return population;
    }


    public int GetBluePopulation()
    {
        return GetPopulation(GridCell.BuildingType.Blue);
    }


    public int GetRedPopulation()
    {
        return GetPopulation(GridCell.BuildingType.Red);
    }


    public int GetGreenPopulation()
    {
        return GetPopulation(GridCell.BuildingType.Green);
    }


    public int GetYellowPopulation()
    {
        return GetPopulation(GridCell.BuildingType.Yellow);
    }


    public int GetTotalPopulation()
    {
        return GetBluePopulation() +
               GetRedPopulation() +
               GetGreenPopulation() +
               GetYellowPopulation();
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ClearAllBuildings()
    {
        foreach (GridCell cell in GetAllActiveCells())
        {
            cell.ClearBuilding();
            cell.SetHighlight(false);
            cell.HidePlacementIndicator();
        }

        currentlySelectedCell = null;

        EndPlacementMode();

        if (BuildingScoreManager.Instance != null)
        {
            BuildingScoreManager.Instance.ClearScores();
        }

        Debug.Log(
            "BoardManager: All buildings cleared."
        );
    }


    // =========================================================
    // NORMAL NEIGHBOUR CHECK
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

        // RIGHT
        neighbour = GetCell(cell.X + 1, cell.Y);

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        // LEFT
        neighbour = GetCell(cell.X - 1, cell.Y);

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        // UP
        neighbour = GetCell(cell.X, cell.Y + 1);

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        // DOWN
        neighbour = GetCell(cell.X, cell.Y - 1);

        if (neighbour != null &&
            neighbour.CurrentBuilding == buildingType)
        {
            return true;
        }

        return false;
    }


    // =========================================================
    // BUILDING TYPE
    // =========================================================

    private GridCell.BuildingType GetBuildingType(int buildingIndex)
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
                return null;
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

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


    [ContextMenu("Refresh Board Analysis")]
    private void DebugRefreshBoard()
    {
        RefreshBoardAnalysis();
    }


    // =========================================================
    // RESTORE BUILDING
    // =========================================================

    public void RestoreBuilding(
        GridCell cell,
        GridCell.BuildingType buildingType,
        int population)
    {
        if (cell == null)
        {
            Debug.LogWarning(
                "BoardManager: Cannot restore building. " +
                "GridCell is null."
            );

            return;
        }

        Sprite buildingSprite =
            GetBuildingSprite(GetBuildingIndex(buildingType));

        if (buildingType == GridCell.BuildingType.None)
        {
            cell.ClearBuilding();
            return;
        }

        if (buildingSprite == null)
        {
            Debug.LogWarning(
                "BoardManager: Cannot restore " + buildingType +
                " because its sprite is missing."
            );

            return;
        }

        cell.SetBuilding(
            buildingSprite,
            buildingType,
            population
        );
    }


    private int GetBuildingIndex(GridCell.BuildingType buildingType)
    {
        switch (buildingType)
        {
            case GridCell.BuildingType.Blue:
                return 0;

            case GridCell.BuildingType.Red:
                return 1;

            case GridCell.BuildingType.Green:
                return 2;

            case GridCell.BuildingType.Yellow:
                return 3;

            default:
                return -1;
        }
    }
}
