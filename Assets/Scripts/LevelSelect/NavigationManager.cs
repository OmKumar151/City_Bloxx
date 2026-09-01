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

    private int boardX = 0;
    private int boardY = 0;

    private NavigationMode currentMode =
        NavigationMode.BuildingSelection;


    public int SelectedBuilding => selectedBuilding;
    public int BoardX => boardX;
    public int BoardY => boardY;
    public NavigationMode CurrentMode => currentMode;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentMode =
            NavigationMode.BuildingSelection;

        selectedBuilding = 0;

        boardX = 0;
        boardY = 0;

        UpdateBuildingSelectionVisual();

        Debug.Log(
            "Navigation started. Building Selection Mode."
        );

        Debug.Log(
            "Selected Building: " +
            selectedBuilding
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
        else if (currentMode ==
                 NavigationMode.BoardPlacement)
        {
            MoveBoardUp();
        }
    }


    public void Down()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            SelectNextBuilding();
        }
        else if (currentMode ==
                 NavigationMode.BoardPlacement)
        {
            MoveBoardDown();
        }
    }


    public void Left()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            SelectPreviousBuilding();
        }
        else if (currentMode ==
                 NavigationMode.BoardPlacement)
        {
            MoveBoardLeft();
        }
    }


    public void Right()
    {
        if (currentMode ==
            NavigationMode.BuildingSelection)
        {
            SelectNextBuilding();
        }
        else if (currentMode ==
                 NavigationMode.BoardPlacement)
        {
            MoveBoardRight();
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

        Debug.Log(
            "Selected Building: " +
            selectedBuilding
        );

        UpdateBuildingSelectionVisual();


        if (infoPanel != null &&
            boardManager != null)
        {
            infoPanel.ShowBuildingSelected(
                boardManager.GetBuildingName(
                    selectedBuilding
                )
            );
        }
    }


    private void SelectNextBuilding()
    {
        selectedBuilding++;

        if (selectedBuilding >= numberOfBuildings)
        {
            selectedBuilding =
                numberOfBuildings - 1;
        }

        Debug.Log(
            "Selected Building: " +
            selectedBuilding
        );

        UpdateBuildingSelectionVisual();


        if (infoPanel != null &&
            boardManager != null)
        {
            infoPanel.ShowBuildingSelected(
                boardManager.GetBuildingName(
                    selectedBuilding
                )
            );
        }
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
                "NavigationManager: BuildingSelectionUI is not assigned."
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
        else if (currentMode ==
                 NavigationMode.BoardPlacement)
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
                "NavigationManager: BoardManager is not assigned."
            );

            return;
        }


        bool hasValidPlacement =
            boardManager.HasValidPlacement(
                selectedBuilding
            );


        if (!hasValidPlacement)
        {
            Debug.Log(
                "No valid placement exists for building " +
                selectedBuilding +
                ". Remaining in Building Selection Mode."
            );

            if (infoPanel != null)
            {
                infoPanel.ShowNoValidPlacement(
                    boardManager.GetBuildingName(
                        selectedBuilding
                    )
                );
            }

            return;
        }


        currentMode =
            NavigationMode.BoardPlacement;


        // -----------------------------------------------------
        // Find an ACTUAL existing cell.
        // -----------------------------------------------------

        int validX;
        int validY;

        bool foundValidCell =
            boardManager.FindValidPlacement(
                selectedBuilding,
                out validX,
                out validY
            );


        if (!foundValidCell)
        {
            currentMode =
                NavigationMode.BuildingSelection;

            return;
        }


        boardX = validX;
        boardY = validY;


        UpdateBoardCursor();


        Debug.Log(
            "Entered Board Placement Mode."
        );


        LogBoardPosition();


        if (infoPanel != null)
        {
            infoPanel.ShowBuildingCanBePlaced();
        }
    }


    // =========================================================
    // CONFIRM BOARD POSITION
    // =========================================================

    private void ConfirmBoardPosition()
    {
        Debug.Log(
            "OK pressed at Board Position: " +
            boardX +
            ", " +
            boardY
        );


        Debug.Log(
            "Selected Building: " +
            selectedBuilding
        );


        if (boardManager == null)
        {
            Debug.LogWarning(
                "NavigationManager: BoardManager is not assigned."
            );

            return;
        }


        bool placementSuccessful =
            boardManager.PlaceBuilding(
                selectedBuilding
            );


        if (placementSuccessful)
        {
            if (infoPanel != null)
            {
                infoPanel.ShowBuildingPlaced(
                    boardManager.GetBuildingName(
                        selectedBuilding
                    )
                );
            }


            currentMode =
                NavigationMode.BuildingSelection;


            Debug.Log(
                "Building placed successfully. " +
                "Returned to Building Selection Mode."
            );


            UpdateBuildingSelectionVisual();
        }
        else
        {
            Debug.Log(
                "Building placement failed. " +
                "Remaining in Board Placement Mode."
            );


            if (infoPanel != null)
            {
                infoPanel.ShowBuildingCannotBePlaced();
            }
        }
    }


    // =========================================================
    // BOARD MOVEMENT
    // =========================================================

    private void MoveBoardUp()
    {
        if (boardManager == null)
            return;


        GridCell currentCell =
            boardManager.GetCell(
                boardX,
                boardY
            );


        GridCell nextCell =
            boardManager.GetNeighbour(
                currentCell,
                0,
                -1
            );


        if (nextCell == null)
        {
            Debug.Log(
                "No GridCell above current position."
            );

            return;
        }


        boardX = nextCell.X;
        boardY = nextCell.Y;


        UpdateBoardCursor();

        LogBoardPosition();


        UpdatePlacementMessage();
    }


    private void MoveBoardDown()
    {
        if (boardManager == null)
            return;


        GridCell currentCell =
            boardManager.GetCell(
                boardX,
                boardY
            );


        GridCell nextCell =
            boardManager.GetNeighbour(
                currentCell,
                0,
                1
            );


        if (nextCell == null)
        {
            Debug.Log(
                "No GridCell below current position."
            );

            return;
        }


        boardX = nextCell.X;
        boardY = nextCell.Y;


        UpdateBoardCursor();

        LogBoardPosition();


        UpdatePlacementMessage();
    }


    private void MoveBoardLeft()
    {
        if (boardManager == null)
            return;


        GridCell currentCell =
            boardManager.GetCell(
                boardX,
                boardY
            );


        GridCell nextCell =
            boardManager.GetNeighbour(
                currentCell,
                -1,
                0
            );


        if (nextCell == null)
        {
            Debug.Log(
                "No GridCell to the left."
            );

            return;
        }


        boardX = nextCell.X;
        boardY = nextCell.Y;


        UpdateBoardCursor();

        LogBoardPosition();


        UpdatePlacementMessage();
    }


    private void MoveBoardRight()
    {
        if (boardManager == null)
            return;


        GridCell currentCell =
            boardManager.GetCell(
                boardX,
                boardY
            );


        GridCell nextCell =
            boardManager.GetNeighbour(
                currentCell,
                1,
                0
            );


        if (nextCell == null)
        {
            Debug.Log(
                "No GridCell to the right."
            );

            return;
        }


        boardX = nextCell.X;
        boardY = nextCell.Y;


        UpdateBoardCursor();

        LogBoardPosition();


        UpdatePlacementMessage();
    }


    // =========================================================
    // UPDATE BOARD CURSOR
    // =========================================================

    private void UpdateBoardCursor()
    {
        if (boardManager != null)
        {
            boardManager.SetSelectedCell(
                boardX,
                boardY
            );
        }
        else
        {
            Debug.LogWarning(
                "NavigationManager: BoardManager is not assigned."
            );
        }
    }


    // =========================================================
    // PLACEMENT MESSAGE
    // =========================================================

    private void UpdatePlacementMessage()
    {
        if (infoPanel == null ||
            boardManager == null)
        {
            return;
        }


        if (boardManager.CanPlaceCurrentBuilding(
            selectedBuilding))
        {
            infoPanel.ShowBuildingCanBePlaced();
        }
        else
        {
            infoPanel.ShowBuildingCannotBePlaced();
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void LogBoardPosition()
    {
        Debug.Log(
            "Board Position: " +
            boardX +
            ", " +
            boardY
        );
    }


    // =========================================================
    // DISCARD / CANCEL
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


        Debug.Log(
            "Returned to Building Selection Mode."
        );


        if (infoPanel != null)
        {
            infoPanel.ShowBuildingCancelled();
        }


        UpdateBuildingSelectionVisual();
    }
}
