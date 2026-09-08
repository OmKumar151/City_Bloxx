using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public enum NavigationMode
    {
        BuildingSelection,
        BoardPlacement
    }

    [Header("References")]
    [SerializeField] private BuildingSelectionUI buildingSelectionUI;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private InfoPanelUI infoPanel;

    [Header("Building Selection")]
    [SerializeField] private int numberOfBuildings = 4;

    private int selectedBuilding = 0;

    private GridCell currentBoardCell;

    private NavigationMode currentMode =
        NavigationMode.BuildingSelection;

    public int SelectedBuilding =>
        selectedBuilding;

    public NavigationMode CurrentMode =>
        currentMode;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentMode =
            NavigationMode.BuildingSelection;

        selectedBuilding = 0;

        currentBoardCell = null;

        UpdateBuildingSelectionVisual();

        ShowBuildingSelected();

        Debug.Log(
            "Navigation started. Building Selection Mode."
        );
    }

    // =========================================================
    // D-PAD
    // =========================================================

    public void Up()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            SelectPreviousBuilding();
        }
        else
        {
            MoveBoard(0, -1);
        }
    }

    public void Down()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            SelectNextBuilding();
        }
        else
        {
            MoveBoard(0, 1);
        }
    }

    public void Left()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            SelectPreviousBuilding();
        }
        else
        {
            MoveBoard(-1, 0);
        }
    }

    public void Right()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            SelectNextBuilding();
        }
        else
        {
            MoveBoard(1, 0);
        }
    }

    // =========================================================
    // BUILDING SELECTION
    // =========================================================

    private void SelectPreviousBuilding()
    {
        selectedBuilding--;

        if (selectedBuilding < 0)
        {
            selectedBuilding = 0;
        }

        UpdateBuildingSelectionVisual();

        ShowBuildingSelected();

        Debug.Log(
            "Selected Building: " +
            selectedBuilding
        );
    }

    private void SelectNextBuilding()
    {
        selectedBuilding++;

        if (selectedBuilding >= numberOfBuildings)
        {
            selectedBuilding =
                numberOfBuildings - 1;
        }

        UpdateBuildingSelectionVisual();

        ShowBuildingSelected();

        Debug.Log(
            "Selected Building: " +
            selectedBuilding
        );
    }

    private void UpdateBuildingSelectionVisual()
    {
        if (buildingSelectionUI != null)
        {
            buildingSelectionUI.SetSelectedBuilding(
                selectedBuilding
            );
        }
        else
        {
            Debug.LogWarning(
                "NavigationManager: " +
                "BuildingSelectionUI is not assigned."
            );
        }
    }

    // =========================================================
    // OK BUTTON
    // =========================================================

    public void OK()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            EnterBoardPlacementMode();
        }
        else
        {
            ConfirmBoardPosition();
        }
    }

    // =========================================================
    // ENTER BOARD MODE
    // =========================================================

    private void EnterBoardPlacementMode()
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "NavigationManager: " +
                "BoardManager is not assigned."
            );

            return;
        }

        /*
         * The selected building must have at least one
         * legal operation somewhere on the board.
         *
         * This can be:
         *
         * 1. Normal placement on an empty cell.
         * 2. Replacement of an existing building.
         *
         * BoardManager performs the complete validation.
         */
        if (!boardManager.HasValidPlacementOrReplacement(
            selectedBuilding))
        {
            ShowNoValidPlacement();

            Debug.Log(
                "No valid placement or replacement exists for " +
                GetBuildingName(selectedBuilding)
            );

            return;
        }

        /*
         * Start at the first actual GridCell.
         *
         * It may be occupied because replacement is allowed.
         */
        currentBoardCell =
            boardManager.GetFirstBoardCell();

        if (currentBoardCell == null)
        {
            ShowNoValidPlacement();

            Debug.LogWarning(
                "NavigationManager: " +
                "No active GridCells exist."
            );

            return;
        }

        currentMode =
            NavigationMode.BoardPlacement;

        boardManager.SetSelectedCell(
            currentBoardCell
        );

        ShowPlacementStatus();

        Debug.Log(
            "Entered Board Mode at (" +
            currentBoardCell.X +
            ", " +
            currentBoardCell.Y +
            ")."
        );
    }

    // =========================================================
    // BOARD MOVEMENT
    // =========================================================

    private void MoveBoard(
        int directionX,
        int directionY)
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "NavigationManager: " +
                "BoardManager is not assigned."
            );

            return;
        }

        if (currentBoardCell == null)
        {
            currentBoardCell =
                boardManager.GetFirstBoardCell();

            if (currentBoardCell == null)
            {
                Debug.LogWarning(
                    "NavigationManager: " +
                    "No active GridCells exist."
                );

                return;
            }

            boardManager.SetSelectedCell(
                currentBoardCell
            );

            ShowPlacementStatus();

            return;
        }

        /*
         * Only an actual neighbouring GridCell can be
         * selected.
         *
         * This means gaps in an irregular board cannot
         * be skipped.
         */
        GridCell nextCell =
            boardManager.GetNeighbour(
                currentBoardCell,
                directionX,
                directionY
            );

        if (nextCell == null)
        {
            Debug.Log(
                "No GridCell in requested direction."
            );

            return;
        }

        currentBoardCell =
            nextCell;

        boardManager.SetSelectedCell(
            currentBoardCell
        );

        ShowPlacementStatus();

        Debug.Log(
            "Board Position: " +
            currentBoardCell.X +
            ", " +
            currentBoardCell.Y
        );
    }

    // =========================================================
    // CONFIRM BOARD POSITION
    // =========================================================

    private void ConfirmBoardPosition()
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "NavigationManager: " +
                "BoardManager is not assigned."
            );

            return;
        }

        if (currentBoardCell == null)
        {
            Debug.LogWarning(
                "NavigationManager: " +
                "No board cell selected."
            );

            return;
        }

        /*
         * TEMPORARY POPULATION
         *
         * This will eventually come from the completed
         * building gameplay level.
         */
        int population = 100;

        // =====================================================
        // EMPTY CELL
        // NORMAL PLACEMENT
        // =====================================================

        if (!currentBoardCell.IsOccupied)
        {
            bool placementSuccessful =
                boardManager.PlaceBuilding(
                    selectedBuilding,
                    population
                );

            if (placementSuccessful)
            {
                AddPopulation(population);

                ShowBuildingPlaced();

                ReturnToBuildingSelection();

                Debug.Log(
                    "Building placed successfully. " +
                    "Returned to Building Selection Mode."
                );
            }
            else
            {
                ShowBuildingCannotBePlaced();

                Debug.Log(
                    "Building placement failed. " +
                    "Remaining in Board Mode."
                );
            }

            return;
        }

        // =====================================================
        // OCCUPIED CELL
        // REPLACEMENT
        // =====================================================

        int oldPopulation =
            currentBoardCell.BuildingPopulation;

        GridCell.BuildingType oldBuilding =
            currentBoardCell.CurrentBuilding;

        bool replacementSuccessful =
            boardManager.ReplaceBuilding(
                selectedBuilding,
                population
            );

        if (replacementSuccessful)
        {
            /*
             * Remove the population belonging to the old
             * building, then add the new building's population.
             */
            RemovePopulation(oldPopulation);

            AddPopulation(population);

            ShowBuildingPlaced();

            ReturnToBuildingSelection();

            Debug.Log(
                "Building replaced successfully. " +
                "Old Building: " +
                oldBuilding +
                " | Old Population: " +
                oldPopulation +
                " | New Population: " +
                population
            );
        }
        else
        {
            /*
             * Replacement failed because the resulting
             * board would violate one or more building rules.
             */
            ShowBuildingCannotBePlaced();

            Debug.Log(
                "Building replacement failed. " +
                "Remaining in Board Mode."
            );
        }
    }

    // =========================================================
    // POPULATION
    // =========================================================

    private void AddPopulation(
        int amount)
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.AddPopulation(
                amount
            );
        }
        else
        {
            Debug.LogWarning(
                "NavigationManager: PopulationManager " +
                "instance was not found."
            );
        }
    }

    private void RemovePopulation(
        int amount)
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.RemovePopulation(
                amount
            );
        }
        else
        {
            Debug.LogWarning(
                "NavigationManager: PopulationManager " +
                "instance was not found."
            );
        }
    }

    // =========================================================
    // RETURN TO BUILDING SELECTION
    // =========================================================

    private void ReturnToBuildingSelection()
    {
        currentMode =
            NavigationMode.BuildingSelection;

        currentBoardCell = null;

        UpdateBuildingSelectionVisual();
    }

    // =========================================================
    // PLACEMENT STATUS
    // =========================================================

    private void ShowPlacementStatus()
    {
        if (infoPanel == null ||
            boardManager == null ||
            currentBoardCell == null)
        {
            return;
        }

        /*
         * Empty cell:
         *     Checks normal placement rules.
         *
         * Occupied cell:
         *     Checks complete-board replacement rules.
         */
        bool canOperate =
            boardManager.CanPlaceOrReplaceBuilding(
                selectedBuilding
            );

        if (canOperate)
        {
            infoPanel.ShowBuildingCanBePlaced();
        }
        else
        {
            infoPanel.ShowBuildingCannotBePlaced();
        }
    }

    // =========================================================
    // CANCEL / DISCARD
    // =========================================================

    public void CancelPlacement()
    {
        if (currentMode !=
            NavigationMode.BoardPlacement)
        {
            return;
        }

        currentMode =
            NavigationMode.BuildingSelection;

        currentBoardCell = null;

        if (boardManager != null)
        {
            GridCell selectedCell =
                boardManager.GetSelectedCell();

            if (selectedCell != null)
            {
                selectedCell.SetHighlight(false);
            }
        }

        if (infoPanel != null)
        {
            infoPanel.ShowBuildingCancelled();
        }

        UpdateBuildingSelectionVisual();

        Debug.Log(
            "Cancelled placement. " +
            "Returned to Building Selection Mode."
        );
    }

    // =========================================================
    // INFO PANEL
    // =========================================================

    private void ShowBuildingSelected()
    {
        if (infoPanel == null)
        {
            return;
        }

        infoPanel.ShowBuildingSelected(
            GetBuildingName(selectedBuilding)
        );
    }

    private void ShowNoValidPlacement()
    {
        if (infoPanel == null)
        {
            return;
        }

        infoPanel.ShowNoValidPlacement(
            GetBuildingName(selectedBuilding)
        );
    }

    private void ShowBuildingPlaced()
    {
        if (infoPanel == null)
        {
            return;
        }

        infoPanel.ShowBuildingPlaced(
            GetBuildingName(selectedBuilding)
        );
    }

    private void ShowBuildingCannotBePlaced()
    {
        if (infoPanel == null)
        {
            return;
        }

        infoPanel.ShowBuildingCannotBePlaced();
    }

    // =========================================================
    // BUILDING NAME
    // =========================================================

    private string GetBuildingName(
        int buildingIndex)
    {
        switch (buildingIndex)
        {
            case 0:
                return "Blue";

            case 1:
                return "Red";

            case 2:
                return "Green";

            case 3:
                return "Yellow";

            default:
                return "Unknown";
        }
    }
}