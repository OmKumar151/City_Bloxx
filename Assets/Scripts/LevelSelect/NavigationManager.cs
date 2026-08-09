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
    // D-PAD
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

        boardX = 0;
        boardY = 0;

        UpdateBoardCursor();

        Debug.Log("Entered Board Placement Mode");

        LogBoardPosition();
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

        // Actual building placement will be added later.
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

        UpdateBoardCursor();
        LogBoardPosition();
    }


    private void MoveBoardDown()
    {
        if (boardY > 0)
        {
            boardY--;
        }

        UpdateBoardCursor();
        LogBoardPosition();
    }


    private void MoveBoardLeft()
    {
        if (boardX > 0)
        {
            boardX--;
        }

        UpdateBoardCursor();
        LogBoardPosition();
    }


    private void MoveBoardRight()
    {
        if (boardX < boardWidth - 1)
        {
            boardX++;
        }

        UpdateBoardCursor();
        LogBoardPosition();
    }


    private void UpdateBoardCursor()
    {
        if (boardManager != null)
        {
            boardManager.SetSelectedCell(boardX, boardY);
        }
        else
        {
            Debug.LogWarning(
                "NavigationManager: BoardManager is not assigned."
            );
        }
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
    // DISCARD
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