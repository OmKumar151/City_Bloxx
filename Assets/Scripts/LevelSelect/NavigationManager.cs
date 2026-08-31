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


    [Header("Board")]
    [SerializeField] private int boardWidth = 5;
    [SerializeField] private int boardHeight = 5;


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
        else
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
        else
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
        else
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

        UpdateBuildingSelectionVisual();
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
    }


    private void UpdateBuildingSelectionVisual()
    {
        if (buildingSelectionUI != null)
        {
            buildingSelectionUI.SetSelectedBuilding(
                selectedBuilding
            );
        }


        // Show building name.
        if (infoPanel != null)
        {
            string buildingName =
                boardManager != null
                    ? boardManager.GetBuildingName(
                        selectedBuilding)
                    : "Building " +
                      selectedBuilding;

            infoPanel.ShowBuildingSelected(
                buildingName
            );
        }
    }


    // =========================================================
    // OK
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
            string buildingName =
                boardManager.GetBuildingName(
                    selectedBuilding
                );


            if (infoPanel != null)
            {
                infoPanel.ShowNoValidPlacement(
                    buildingName
                );
            }


            Debug.Log(
                "No valid placement exists for " +
                buildingName
            );

            return;
        }


        currentMode =
            NavigationMode.BoardPlacement;


        boardX = 0;
        boardY = 0;


        UpdateBoardCursor();


        if (infoPanel != null)
        {
            infoPanel.ShowMessage(
                "Select a location"
            );
        }


        Debug.Log(
            "Entered Board Placement Mode."
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
                "NavigationManager: BoardManager is not assigned."
            );

            return;
        }


        bool canPlace =
            boardManager.CanPlaceCurrentBuilding(
                selectedBuilding
            );


        if (!canPlace)
        {
            if (infoPanel != null)
            {
                infoPanel.ShowBuildingCannotBePlaced();
            }


            Debug.Log(
                "Building cannot be placed here."
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


            UpdateBuildingSelectionVisual();


            Debug.Log(
                "Building placed successfully."
            );
        }
    }


    // =========================================================
    // BOARD MOVEMENT
    // =========================================================

    private void MoveBoardUp()
    {
        if (boardY > 0)
        {
            boardY--;
        }

        UpdateBoardCursor();
    }


    private void MoveBoardDown()
    {
        if (boardY < boardHeight - 1)
        {
            boardY++;
        }

        UpdateBoardCursor();
    }


    private void MoveBoardLeft()
    {
        if (boardX > 0)
        {
            boardX--;
        }

        UpdateBoardCursor();
    }


    private void MoveBoardRight()
    {
        if (boardX < boardWidth - 1)
        {
            boardX++;
        }

        UpdateBoardCursor();
    }


    // =========================================================
    // UPDATE BOARD CURSOR
    // =========================================================

    private void UpdateBoardCursor()
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "NavigationManager: BoardManager is not assigned."
            );

            return;
        }


        boardManager.SetSelectedCell(
            boardX,
            boardY
        );


        // Tell player whether this position is valid.
        bool canPlace =
            boardManager.CanPlaceCurrentBuilding(
                selectedBuilding
            );


        if (infoPanel != null)
        {
            if (canPlace)
            {
                infoPanel.ShowBuildingCanBePlaced();
            }
            else
            {
                infoPanel.ShowBuildingCannotBePlaced();
            }
        }


        LogBoardPosition();
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


        if (infoPanel != null)
        {
            infoPanel.ShowBuildingCancelled();
        }


        Debug.Log(
            "Returned to Building Selection Mode."
        );


        UpdateBuildingSelectionVisual();
    }
}