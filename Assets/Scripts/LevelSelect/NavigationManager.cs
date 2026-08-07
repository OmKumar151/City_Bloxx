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

    [Header("Building Selection")]
    [SerializeField] private int numberOfBuildings = 4;

    [Header("Board")]
    [SerializeField] private int boardWidth = 5;
    [SerializeField] private int boardHeight = 5;

    private int selectedBuilding = 0;

    private int boardX = 0;
    private int boardY = 0;

    private NavigationMode currentMode = NavigationMode.BuildingSelection;

    public int SelectedBuilding => selectedBuilding;

    public int BoardX => boardX;

    public int BoardY => boardY;

    public NavigationMode CurrentMode => currentMode;


    private void Start()
    {
        currentMode = NavigationMode.BuildingSelection;

        selectedBuilding = 0;

        boardX = 0;
        boardY = 0;

        UpdateBuildingSelectionVisual();

        Debug.Log("Navigation started. Building Selection Mode.");
        Debug.Log("Selected Building: " + selectedBuilding);
    }


    // =========================================================
    // BUILDING SELECTION
    // =========================================================

    public void Up()
    {
        if (currentMode == NavigationMode.BuildingSelection)
        {
            SelectPreviousBuilding();
        }
        else if (currentMode == NavigationMode.BoardPlacement)
        {
            MoveBoardUp();
        }
    }


    public void Down()
    {
        if (currentMode == NavigationMode.BuildingSelection)
        {
            SelectNextBuilding();
        }
        else if (currentMode == NavigationMode.BoardPlacement)
        {
            MoveBoardDown();
        }
    }


    public void Left()
    {
        if (currentMode == NavigationMode.BuildingSelection)
        {
            SelectPreviousBuilding();
        }
        else if (currentMode == NavigationMode.BoardPlacement)
        {
            MoveBoardLeft();
        }
    }


    public void Right()
    {
        if (currentMode == NavigationMode.BuildingSelection)
        {
            SelectNextBuilding();
        }
        else if (currentMode == NavigationMode.BoardPlacement)
        {
            MoveBoardRight();
        }
    }


    private void SelectPreviousBuilding()
    {
        selectedBuilding--;

        if (selectedBuilding < 0)
        {
            selectedBuilding = 0;
        }

        Debug.Log("Selected Building: " + selectedBuilding);

        UpdateBuildingSelectionVisual();
    }


    private void SelectNextBuilding()
    {
        selectedBuilding++;

        if (selectedBuilding >= numberOfBuildings)
        {
            selectedBuilding = numberOfBuildings - 1;
        }

        Debug.Log("Selected Building: " + selectedBuilding);

        UpdateBuildingSelectionVisual();
    }


    private void UpdateBuildingSelectionVisual()
    {
        if (buildingSelectionUI != null)
        {
            buildingSelectionUI.SetSelectedBuilding(selectedBuilding);
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
        if (currentMode == NavigationMode.BuildingSelection)
        {
            EnterBoardPlacementMode();
        }
        else if (currentMode == NavigationMode.BoardPlacement)
        {
            ConfirmBoardPosition();
        }
    }


    private void EnterBoardPlacementMode()
    {
        currentMode = NavigationMode.BoardPlacement;

        // Start the board cursor at the first cell.
        boardX = 0;
        boardY = 0;

        Debug.Log("Entered Board Placement Mode");

        Debug.Log(
            "Board Position: " +
            boardX +
            ", " +
            boardY
        );
    }


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

        // Placement rules will be connected here later.
        //
        // For now we only report the position.
    }


    // =========================================================
    // BOARD MOVEMENT
    // =========================================================

    private void MoveBoardUp()
    {
        if (boardY < boardHeight - 1)
        {
            boardY++;
        }

        LogBoardPosition();
    }


    private void MoveBoardDown()
    {
        if (boardY > 0)
        {
            boardY--;
        }

        LogBoardPosition();
    }


    private void MoveBoardLeft()
    {
        if (boardX > 0)
        {
            boardX--;
        }

        LogBoardPosition();
    }


    private void MoveBoardRight()
    {
        if (boardX < boardWidth - 1)
        {
            boardX++;
        }

        LogBoardPosition();
    }


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
        if (currentMode != NavigationMode.BoardPlacement)
        {
            return;
        }

        currentMode = NavigationMode.BuildingSelection;

        Debug.Log("Returned to Building Selection Mode");

        UpdateBuildingSelectionVisual();
    }
}