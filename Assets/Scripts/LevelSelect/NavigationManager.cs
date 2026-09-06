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
    // ENTER BOARD PLACEMENT
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
         * Check whether this building can be placed
         * anywhere on the board.
         */
        if (!boardManager.HasValidPlacement(
            selectedBuilding))
        {
            ShowNoValidPlacement();

            Debug.Log(
                "No valid placement exists for " +
                GetBuildingName(selectedBuilding)
            );

            return;
        }

        /*
         * Start the cursor at the first active cell.
         *
         * The player can then navigate through the actual
         * irregular board using BoardManager.GetNeighbour().
         */
        currentBoardCell =
            boardManager.GetFirstAvailableCell();

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
            "Entered Board Placement Mode at (" +
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
                boardManager.GetFirstAvailableCell();

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

        GridCell nextCell =
            boardManager.GetNeighbour(
                currentBoardCell,
                directionX,
                directionY
            );

        /*
         * No cell exists in that direction.
         *
         * This is expected on an irregular map.
         * Simply remain on the current cell.
         */
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
         * Population is temporarily 0.
         *
         * Later this value will come from the gameplay
         * scene after the player completes the building level.
         */
        // TEMPORARY: This will eventually come from the gameplay scene.
        int population = 100;

        bool placementSuccessful =
            boardManager.PlaceBuilding(
                selectedBuilding,
                population
            );

        if (placementSuccessful)
        {
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.AddPopulation(
                    population
                );
            }
            else
            {
                Debug.LogWarning(
                    "NavigationManager: PopulationManager instance was not found."
                );
            }

            ShowBuildingPlaced();

            currentMode =
                NavigationMode.BuildingSelection;

            currentBoardCell = null;

            UpdateBuildingSelectionVisual();

            Debug.Log(
                "Building placed successfully. " +
                "Returned to Building Selection Mode."
            );
        }
        else
        {
            ShowBuildingCannotBePlaced();

            /*
             * Stay in Board Placement Mode.
             *
             * The player can move to another cell.
             */
            Debug.Log(
                "Building placement failed. " +
                "Remaining in Board Placement Mode."
            );
        }
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

        bool canPlace =
            boardManager.CanPlaceBuilding(
                selectedBuilding
            );

        if (canPlace)
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