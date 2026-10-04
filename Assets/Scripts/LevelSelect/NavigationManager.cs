using Unity.VisualScripting;
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

    public int SelectedBuilding
    {
        get { return selectedBuilding; }
    }

    public NavigationMode CurrentMode
    {
        get { return currentMode; }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentMode =
            NavigationMode.BuildingSelection;

        selectedBuilding = 0;

        currentBoardCell = null;

        // Building Selection Mode must NEVER show
        // board placement glow or placement marker.
        ClearPlacementVisuals();

        UpdateBuildingSelectionVisual();

        // Safety clear in case the selection UI changes
        // anything during its visual update.
        ClearPlacementVisuals();

        ShowBuildingSelected();
        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();

        Debug.Log(
            "Navigation started. Building Selection Mode. " +
            "Board placement visuals are OFF."
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
        int nextBuilding =
            GetPreviousUnlockedBuilding(
                selectedBuilding
            );

        if (nextBuilding == selectedBuilding)
        {
            return;
        }

        selectedBuilding =
            nextBuilding;

        // Only change the building selection UI.
        UpdateBuildingSelectionVisual();

        // Do NOT show board placement visuals while
        // selecting a building.
        ClearPlacementVisuals();

        ShowBuildingSelected();
        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();

        Debug.Log(
            "Selected Building: " +
            selectedBuilding +
            " | Board placement visuals OFF."
        );
    }


    private void SelectNextBuilding()
    {
        int nextBuilding =
            GetNextUnlockedBuilding(
                selectedBuilding
            );

        if (nextBuilding == selectedBuilding)
        {
            return;
        }

        selectedBuilding =
            nextBuilding;

        // Only change the building selection UI.
        UpdateBuildingSelectionVisual();

        // Do NOT show board placement visuals while
        // selecting a building.
        ClearPlacementVisuals();

        ShowBuildingSelected();
        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();

        Debug.Log(
            "Selected Building: " +
            selectedBuilding +
            " | Board placement visuals OFF."
        );
    }


    // =========================================================
    // UNLOCKED BUILDING NAVIGATION
    // =========================================================

    private int GetNextUnlockedBuilding(
        int currentBuilding)
    {
        if (PopulationProgressionManager.Instance == null)
        {
            return currentBuilding;
        }

        for (
            int i = currentBuilding + 1;
            i < numberOfBuildings;
            i++)
        {
            if (IsBuildingUnlocked(i))
            {
                return i;
            }
        }

        return currentBuilding;
    }


    private int GetPreviousUnlockedBuilding(
        int currentBuilding)
    {
        if (PopulationProgressionManager.Instance == null)
        {
            return currentBuilding;
        }

        for (
            int i = currentBuilding - 1;
            i >= 0;
            i--)
        {
            if (IsBuildingUnlocked(i))
            {
                return i;
            }
        }

        return currentBuilding;
    }


    private bool IsBuildingUnlocked(
        int buildingIndex)
    {
        if (PopulationProgressionManager.Instance == null)
        {
            return buildingIndex == 0;
        }

        PopulationProgressionManager.RewardType reward;

        switch (buildingIndex)
        {
            case 0:
                reward =
                    PopulationProgressionManager
                        .RewardType.BlueBuilding;
                break;

            case 1:
                reward =
                    PopulationProgressionManager
                        .RewardType.RedBuilding;
                break;

            case 2:
                reward =
                    PopulationProgressionManager
                        .RewardType.GreenBuilding;
                break;

            case 3:
                reward =
                    PopulationProgressionManager
                        .RewardType.YellowBuilding;
                break;

            default:
                return false;
        }

        return PopulationProgressionManager.Instance
            .IsRewardUnlocked(reward);
    }


    // =========================================================
    // BUILDING SELECTION VISUAL
    // =========================================================

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

        // Check whether ANY valid placement or replacement exists.
        if (!boardManager.HasValidPlacementOrReplacement(
            selectedBuilding))
        {
            ShowNoValidPlacement();

            ClearPlacementVisuals();

            Debug.Log(
                "No valid placement or replacement exists for " +
                GetBuildingName(selectedBuilding)
            );

            return;
        }

        // We are now officially entering Board Placement Mode.
        currentMode =
            NavigationMode.BoardPlacement;

        // Board glow and marker are allowed to appear HERE.
        boardManager.ShowPlacementHighlights(
            selectedBuilding
        );

        currentBoardCell =
            boardManager.GetSelectedCell();

        if (currentBoardCell == null)
        {
            currentBoardCell =
                boardManager.GetFirstBoardCell();
        }

        if (currentBoardCell == null)
        {
            ShowNoValidPlacement();

            currentMode =
                NavigationMode.BuildingSelection;

            ClearPlacementVisuals();

            Debug.LogWarning(
                "NavigationManager: " +
                "No active GridCells exist."
            );

            return;
        }

        // Synchronize BoardManager with our selected cell.
        boardManager.SetSelectedCell(
            currentBoardCell
        );

        UpdateExistingBuildingScore();
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

        if (currentMode !=
            NavigationMode.BoardPlacement)
        {
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

                ClearPlacementVisuals();

                return;
            }

            boardManager.SetSelectedCell(
                currentBoardCell
            );

            UpdateExistingBuildingScore();
            ShowPlacementStatus();

            return;
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
            PlayDeniedSound();
            return;
        }

        if (currentBoardCell == null)
        {
            PlayDeniedSound();
            ShowBuildingCannotBePlaced();
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
                PlayPlacementConfirmedSound();

                SyncPopulationManager();
                SaveMap();

                ShowBuildingPlaced();

                ReturnToBuildingSelectionAfterPlacement();

                Debug.Log(
                    "Building placed successfully. " +
                    "Map automatically saved. " +
                    "Returned to Building Selection Mode."
                );
            }
            else
            {
                PlayDeniedSound();
                ShowBuildingCannotBePlaced();

                Debug.Log(
                    "Building placement failed. " +
                    "Remaining in Board Placement Mode."
                );
            }

            return;
        }


        // =====================================================
        // OCCUPIED CELL / REPLACEMENT
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
            PlayPlacementConfirmedSound();

            SyncPopulationManager();
            SaveMap();

            ShowBuildingPlaced();

            GridCell.BuildingType newBuilding =
                currentBoardCell.CurrentBuilding;

            ReturnToBuildingSelectionAfterPlacement();

            Debug.Log(
                "Building replaced successfully. " +
                "Map automatically saved. " +
                "Returned to Building Selection Mode. " +
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
            PlayDeniedSound();
            ShowBuildingCannotBePlaced();

            Debug.Log(
                "Building replacement failed. " +
                "Remaining in Board Placement Mode."
            );
        }
    }


    // =========================================================
    // SAVE
    // =========================================================

    private void SaveMap()
    {
        if (MapSaveManager.Instance == null)
        {
            Debug.LogWarning(
                "NavigationManager: " +
                "MapSaveManager is not available. " +
                "The map was NOT saved."
            );

            return;
        }

        MapSaveManager.Instance.SaveMap();
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

        PopulationManager.Instance.SetPopulation(
            boardManager.GetTotalPopulation()
        );
    }


    // =========================================================
    // RETURN TO BUILDING SELECTION
    // =========================================================

    private void ReturnToBuildingSelectionAfterPlacement()
    {
        currentMode =
            NavigationMode.BuildingSelection;

        currentBoardCell = null;

        // Remove ALL board placement visuals first.
        ClearPlacementVisuals();

        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();

        // Update ONLY the building selection visual.
        UpdateBuildingSelectionVisualOnly();

        // Final safety clear.
        ClearPlacementVisuals();

        Debug.Log(
            "Returned to Building Selection Mode. " +
            "Board placement glow and marker are OFF."
        );
    }


    // =========================================================
    // CLEAR PLACEMENT VISUALS
    // =========================================================

    private void ClearPlacementVisuals()
    {
        if (boardManager != null)
        {
            boardManager.ClearPlacementHighlights();
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

        // Completely remove board glow and marker.
        ClearPlacementVisuals();

        if (infoPanel != null)
        {
            infoPanel.ShowBuildingCancelled();
        }

        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();

        // Restore building-selection UI only.
        UpdateBuildingSelectionVisual();

        // Final safety clear.
        ClearPlacementVisuals();

        Debug.Log(
            "Cancelled placement. " +
            "Returned to Building Selection Mode. " +
            "Board placement glow and marker are OFF."
        );
    }


    // =========================================================
    // SAFETY SAVE
    // =========================================================

    private void OnApplicationPause(
        bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveMap();
        }
    }


    private void OnApplicationQuit()
    {
        SaveMap();
    }


    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayNavigationSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayNavigationButton();
    }


    private void PlayMajorButtonSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayMajorButton();
    }


    private void PlayDeniedSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayDenied();
    }


    private void PlayPlacementConfirmedSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayPlacementConfirmed();
    }


    private void PlayButtonSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayButtonClick();
    }


    // =========================================================
    // BUILDING SELECTION VISUAL ONLY
    // =========================================================

    private void UpdateBuildingSelectionVisualOnly()
    {
        if (buildingSelectionUI != null)
        {
            buildingSelectionUI.SetSelectedBuildingVisualOnly(
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