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
        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();

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
        UpdateCurrentBuildingScore();

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
        UpdateCurrentBuildingScore();

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
    // SCORE
    // =========================================================

    private void UpdateCurrentBuildingScore()
    {
        if (BuildingScoreManager.Instance == null)
        {
            return;
        }

        GridCell.BuildingType buildingType =
            GetBuildingType(selectedBuilding);

        int score =
            GridCell.GetBuildingScore(
                buildingType
            );

        BuildingScoreManager.Instance
            .SetCurrentBuildingScore(score);

        Debug.Log(
            "Current Building: " +
            buildingType +
            " | Current Score: " +
            score
        );
    }

    private void UpdateExistingBuildingScore()
    {
        if (BuildingScoreManager.Instance == null)
        {
            return;
        }

        if (currentBoardCell == null)
        {
            ClearExistingBuildingScore();
            return;
        }

        if (!currentBoardCell.IsOccupied)
        {
            ClearExistingBuildingScore();

            Debug.Log(
                "Selected cell is empty. " +
                "Existing Building Score cleared."
            );

            return;
        }

        int existingScore =
            GridCell.GetBuildingScore(
                currentBoardCell.CurrentBuilding
            );

        BuildingScoreManager.Instance
            .SetExistingBuildingScore(
                existingScore
            );

        Debug.Log(
            "Existing Building: " +
            currentBoardCell.CurrentBuilding +
            " | Existing Score: " +
            existingScore
        );
    }

    private void ClearExistingBuildingScore()
    {
        if (BuildingScoreManager.Instance != null)
        {
            BuildingScoreManager.Instance
                .ClearExistingBuildingScore();
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
         * Ask BoardManager whether this building has at least
         * one legal operation somewhere on the CURRENT board.
         *
         * This includes:
         *
         * 1. Empty-cell placement
         * 2. Valid replacement
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

        UpdateExistingBuildingScore();
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

            UpdateExistingBuildingScore();
            ShowPlacementStatus();

            return;
        }

        /*
         * IMPORTANT:
         *
         * GetNeighbour only checks:
         *
         * X + 1
         * X - 1
         * Y + 1
         * Y - 1
         *
         * Therefore gaps in an irregular map cannot be skipped.
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

        UpdateExistingBuildingScore();
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

        // =====================================================
        // EMPTY CELL
        // =====================================================

        if (!currentBoardCell.IsOccupied)
        {
            bool placementSuccessful =
                boardManager.PlaceBuilding(
                    selectedBuilding
                );

            if (placementSuccessful)
            {
                /*
                 * BoardManager already stores the building's
                 * correct population.
                 *
                 * We no longer use the old temporary value of 100.
                 */

                SyncPopulationManager();

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

        GridCell.BuildingType oldBuilding =
            currentBoardCell.CurrentBuilding;

        int oldPopulation =
            currentBoardCell.BuildingPopulation;

        bool replacementSuccessful =
            boardManager.ReplaceBuilding(
                selectedBuilding
            );

        if (replacementSuccessful)
        {
            SyncPopulationManager();

            ShowBuildingPlaced();

            // Store the new building BEFORE
            // ReturnToBuildingSelection() clears currentBoardCell.
            GridCell.BuildingType newBuilding =
                currentBoardCell.CurrentBuilding;

            ReturnToBuildingSelection();

            Debug.Log(
                "Building replaced successfully. " +
                "Old Building: " +
                oldBuilding +
                " | Old Population: " +
                oldPopulation +
                " | New Building: " +
                newBuilding
            );
        }
        else
        {
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

    private void SyncPopulationManager()
    {
        if (PopulationManager.Instance == null)
        {
            return;
        }

        /*
         * BoardManager is the source of truth for the current
         * board population.
         */

        PopulationManager.Instance.SetPopulation(
            boardManager.GetTotalPopulation()
        );
    }

    // =========================================================
    // RETURN TO BUILDING SELECTION
    // =========================================================

    private void ReturnToBuildingSelection()
    {
        currentMode =
            NavigationMode.BuildingSelection;

        currentBoardCell = null;

        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();
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
    // CANCEL
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

        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();
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
