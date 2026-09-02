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
            MoveBoard(
                0,
                -1
            );
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
            MoveBoard(
                0,
                1
            );
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
            MoveBoard(
                -1,
                0
            );
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
            MoveBoard(
                1,
                0
            );
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


        // First check whether this building has ANY
        // legal position anywhere on the map.

        if (!boardManager.CanPlaceBuilding(
                selectedBuilding))
        {
            ShowNoValidPlacement();

            Debug.Log(
                "No valid placement exists for " +
                boardManager.GetBuildingName(
                    selectedBuilding
                )
            );

            return;
        }


        // Find the first available/legal cell.

        GridCell firstCell =
            GetFirstValidPlacementCell();


        if (firstCell == null)
        {
            ShowNoValidPlacement();
            return;
        }


        currentMode =
            NavigationMode.BoardPlacement;


        currentBoardCell = firstCell;


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
    // FIND FIRST LEGAL PLACEMENT
    // =========================================================

    private GridCell GetFirstValidPlacementCell()
    {
        // Start with the first available cell.
        GridCell firstCell =
            boardManager.GetFirstAvailableCell();


        if (firstCell == null)
        {
            return null;
        }


        // Prefer the first available cell if the building
        // can legally be placed there.

        if (boardManager.CanPlaceBuilding(
                firstCell,
                selectedBuilding))
        {
            return firstCell;
        }


        // If not, search the entire discovered board.

        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                GridCell cell =
                    boardManager.GetCell(x, y);

                if (cell == null)
                    continue;

                if (boardManager.CanPlaceBuilding(
                        cell,
                        selectedBuilding))
                {
                    return cell;
                }
            }
        }


        return null;
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
                "NavigationManager: BoardManager is not assigned."
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
                    "NavigationManager: No GridCells exist."
                );

                return;
            }
        }


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
                "NavigationManager: BoardManager is not assigned."
            );

            return;
        }


        if (currentBoardCell == null)
        {
            Debug.LogWarning(
                "NavigationManager: No board cell selected."
            );

            return;
        }


        Debug.Log(
            "OK pressed at Board Position: " +
            currentBoardCell.X +
            ", " +
            currentBoardCell.Y
        );


        bool placementSuccessful =
            boardManager.PlaceBuilding(
                currentBoardCell,
                selectedBuilding
            );


        if (placementSuccessful)
        {
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
            currentBoardCell == null)
        {
            return;
        }


        bool canPlace =
            boardManager.CanPlaceBuilding(
                currentBoardCell,
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


        string buildingName =
            boardManager != null
                ? boardManager.GetBuildingName(
                    selectedBuilding)
                : "Building";


        currentMode =
            NavigationMode.BuildingSelection;


        currentBoardCell = null;


        if (infoPanel != null)
        {
            infoPanel.ShowBuildingCancelled();
        }


        UpdateBuildingSelectionVisual();


        Debug.Log(
            "Cancelled placement of " +
            buildingName +
            ". Returned to Building Selection Mode."
        );
    }


    // =========================================================
    // INFO PANEL
    // =========================================================

    private void ShowBuildingSelected()
    {
        if (infoPanel == null ||
            boardManager == null)
        {
            return;
        }


        infoPanel.ShowBuildingSelected(
            boardManager.GetBuildingName(
                selectedBuilding
            )
        );
    }


    private void ShowNoValidPlacement()
    {
        if (infoPanel == null ||
            boardManager == null)
        {
            return;
        }


        infoPanel.ShowNoValidPlacement(
            boardManager.GetBuildingName(
                selectedBuilding
            )
        );
    }


    private void ShowBuildingPlaced()
    {
        if (infoPanel == null ||
            boardManager == null)
        {
            return;
        }


        infoPanel.ShowBuildingPlaced(
            boardManager.GetBuildingName(
                selectedBuilding
            )
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
}