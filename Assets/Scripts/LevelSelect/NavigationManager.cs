
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Gameplay Launch")]
    [Tooltip("One config per building index: 0 Blue, 1 Red, 2 Green, 3 Yellow.")]
    [SerializeField] private BuildingGameplayConfig[] buildingConfigurations =
        new BuildingGameplayConfig[4];

    [SerializeField] private string gameplaySceneName = "Gameplay1";

    private bool isLoadingGameplay;

    private int selectedBuilding = 0;
    private GridCell currentBoardCell;

    private NavigationMode currentMode =
        NavigationMode.BuildingSelection;

    public int SelectedBuilding => selectedBuilding;
    public NavigationMode CurrentMode => currentMode;

    private void Awake()
    {
        FindReferences();
    }

    private void OnEnable()
    {
        SubscribeToCityManager();
    }

    private void OnDisable()
    {
        if (CityManager.Instance != null)
            CityManager.Instance.OnCityChanged -= HandleCityChanged;
    }

    private void FindReferences()
    {
        if (buildingSelectionUI == null)
            buildingSelectionUI = FindAnyObjectByType<BuildingSelectionUI>();

        if (boardManager == null)
            boardManager = FindAnyObjectByType<BoardManager>();

        if (infoPanel == null)
            infoPanel = FindAnyObjectByType<InfoPanelUI>();
    }

    private void SubscribeToCityManager()
    {
        if (CityManager.Instance == null)
            return;

        CityManager.Instance.OnCityChanged -= HandleCityChanged;
        CityManager.Instance.OnCityChanged += HandleCityChanged;
    }

    private void HandleCityChanged(int previousCityIndex, int newCityIndex)
    {
        ResetNavigationToBlue();

        Debug.Log(
            "NavigationManager: Navigation reset after switching city."
        );
    }

    private void Start()
    {
        FindReferences();
        SubscribeToCityManager();

        ResetNavigationToBlue();

        Debug.Log("Navigation started. Building Selection Mode.");
    }

    // =========================================================
    // RESET NAVIGATION
    // =========================================================

    public void ResetNavigationToBlue()
    {
        selectedBuilding = 0;
        currentMode = NavigationMode.BuildingSelection;
        currentBoardCell = null;

        if (boardManager != null)
        {
            boardManager.EndPlacementMode();
            boardManager.SetSelectedCell((GridCell)null);
        }

        ClearExistingBuildingScore();
        UpdateBuildingSelectionVisual();
        UpdateCurrentBuildingScore();
        ShowBuildingSelected();

        Debug.Log(
            "NavigationManager: Reset to Blue and Building Selection Mode."
        );
    }

    // =========================================================
    // D-PAD
    // =========================================================

    public void Up()
    {
        if (currentMode == NavigationMode.BuildingSelection)
            SelectPreviousBuilding();
        else
            MoveBoard(0, -1);
    }

    public void Down()
    {
        if (currentMode == NavigationMode.BuildingSelection)
            SelectNextBuilding();
        else
            MoveBoard(0, 1);
    }

    public void Left()
    {
        if (currentMode == NavigationMode.BuildingSelection)
            SelectPreviousBuilding();
        else
            MoveBoard(-1, 0);
    }

    public void Right()
    {
        if (currentMode == NavigationMode.BuildingSelection)
            SelectNextBuilding();
        else
            MoveBoard(1, 0);
    }

    // =========================================================
    // BUILDING SELECTION
    // =========================================================

    private void SelectPreviousBuilding()
    {
        int nextBuilding = GetPreviousUnlockedBuilding(selectedBuilding);

        if (nextBuilding == selectedBuilding)
            return;

        selectedBuilding = nextBuilding;

        ClearPlacementVisuals();
        UpdateBuildingSelectionVisual();
        ShowBuildingSelected();
        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();
    }

    private void SelectNextBuilding()
    {
        int nextBuilding = GetNextUnlockedBuilding(selectedBuilding);

        if (nextBuilding == selectedBuilding)
            return;

        selectedBuilding = nextBuilding;

        ClearPlacementVisuals();
        UpdateBuildingSelectionVisual();
        ShowBuildingSelected();
        UpdateCurrentBuildingScore();
        ClearExistingBuildingScore();
    }

    private int GetNextUnlockedBuilding(int currentBuilding)
    {
        for (int i = currentBuilding + 1; i < numberOfBuildings; i++)
        {
            if (IsBuildingUnlocked(i))
                return i;
        }

        return currentBuilding;
    }

    private int GetPreviousUnlockedBuilding(int currentBuilding)
    {
        for (int i = currentBuilding - 1; i >= 0; i--)
        {
            if (IsBuildingUnlocked(i))
                return i;
        }

        return currentBuilding;
    }

    private bool IsBuildingUnlocked(int buildingIndex)
    {
        if (buildingIndex == 0)
            return true;

        if (PopulationProgressionManager.Instance == null)
            return false;

        PopulationProgressionManager.RewardType reward;

        switch (buildingIndex)
        {
            case 1:
                reward = PopulationProgressionManager.RewardType.RedBuilding;
                break;

            case 2:
                reward = PopulationProgressionManager.RewardType.GreenBuilding;
                break;

            case 3:
                reward = PopulationProgressionManager.RewardType.YellowBuilding;
                break;

            default:
                return false;
        }

        return PopulationProgressionManager.Instance.IsRewardUnlocked(reward);
    }

    // =========================================================
    // BUILDING SELECTION VISUAL
    // =========================================================

    private void UpdateBuildingSelectionVisual()
    {
        if (buildingSelectionUI != null)
        {
            buildingSelectionUI.SetSelectedBuildingVisualOnly(
                selectedBuilding
            );
        }
    }

    // =========================================================
    // SCORES
    // =========================================================

    private void UpdateCurrentBuildingScore()
    {
        if (BuildingScoreManager.Instance == null)
            return;

        GridCell.BuildingType buildingType = GetBuildingType(selectedBuilding);

        BuildingScoreManager.Instance.SetCurrentBuildingScore(
            GridCell.GetBuildingScore(buildingType)
        );
    }

    private void UpdateExistingBuildingScore()
    {
        if (BuildingScoreManager.Instance == null)
            return;

        if (currentBoardCell == null || !currentBoardCell.IsOccupied)
        {
            ClearExistingBuildingScore();
            return;
        }

        int score = GridCell.GetBuildingScore(
            currentBoardCell.CurrentBuilding
        );

        BuildingScoreManager.Instance.SetExistingBuildingScore(score);
    }

    private void ClearExistingBuildingScore()
    {
        if (BuildingScoreManager.Instance != null)
            BuildingScoreManager.Instance.ClearExistingBuildingScore();
    }

    // =========================================================
    // OK
    // =========================================================

    public void OK()
    {
        if (currentMode == NavigationMode.BuildingSelection)
            LaunchGameplayForSelectedBuilding();
        else
            ConfirmBoardPosition();
    }

    // =========================================================
    // GAMEPLAY LAUNCH
    // =========================================================

    // Sends the selected building's config, floor count and special
    // roof flag to the Gameplay scene, then loads it.
    private void LaunchGameplayForSelectedBuilding()
    {
        if (isLoadingGameplay)
            return;

        if (!IsBuildingUnlocked(selectedBuilding))
        {
            ShowNoValidPlacement();
            return;
        }

        BuildingGameplayConfig configuration =
            GetBuildingConfiguration(selectedBuilding);

        if (configuration == null)
        {
            Debug.LogError(
                "NavigationManager: No BuildingGameplayConfig assigned for " +
                GetBuildingName(selectedBuilding) + " (index " +
                selectedBuilding + "). Assign it in Building Configurations.",
                this
            );
            return;
        }

        if (!configuration.HasValidGameplaySetup())
            return;

        if (string.IsNullOrWhiteSpace(gameplaySceneName) ||
            !Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogError(
                "NavigationManager: Scene '" + gameplaySceneName +
                "' cannot be loaded. Check its name and Build Settings.",
                this
            );
            return;
        }

        bool specialRoofUnlocked = IsSpecialRoofUnlocked(configuration);

        GameplaySelection.Select(configuration, specialRoofUnlocked);

        Debug.Log(
            "NavigationManager: Launching " + configuration.buildingId +
            " | Floors: " + configuration.floorsToComplete +
            " | Special roof unlocked: " + specialRoofUnlocked
        );

        SaveMap();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMajorButton();

        isLoadingGameplay = true;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private BuildingGameplayConfig GetBuildingConfiguration(int buildingIndex)
    {
        if (buildingConfigurations == null ||
            buildingIndex < 0 ||
            buildingIndex >= buildingConfigurations.Length)
        {
            return null;
        }

        return buildingConfigurations[buildingIndex];
    }

    private bool IsSpecialRoofUnlocked(BuildingGameplayConfig configuration)
    {
        if (configuration == null ||
            configuration.specialRoofSprite == null ||
            PopulationProgressionManager.Instance == null)
        {
            return false;
        }

        return PopulationProgressionManager.Instance.IsRewardUnlocked(
            configuration.specialRoofUnlockReward
        );
    }

    private void EnterBoardPlacementMode()
    {
        FindReferences();

        if (boardManager == null)
        {
            Debug.LogWarning("NavigationManager: BoardManager is missing.");
            return;
        }

        if (!IsBuildingUnlocked(selectedBuilding))
        {
            ShowNoValidPlacement();
            return;
        }

        if (!boardManager.HasValidPlacementOrReplacement(selectedBuilding))
        {
            ShowNoValidPlacement();
            return;
        }

        boardManager.BeginPlacementMode(selectedBuilding);

        if (!boardManager.IsPlacementModeActive())
            return;

        currentBoardCell = boardManager.GetSelectedCell();

        if (currentBoardCell == null ||
            !currentBoardCell.gameObject.activeInHierarchy)
        {
            currentBoardCell = boardManager.GetFirstBoardCell();
        }

        if (currentBoardCell == null)
        {
            boardManager.EndPlacementMode();
            ShowNoValidPlacement();
            return;
        }

        currentMode = NavigationMode.BoardPlacement;

        boardManager.SetSelectedCell(currentBoardCell);

        UpdateExistingBuildingScore();
        ShowPlacementStatus();
    }

    // =========================================================
    // BOARD MOVEMENT
    // =========================================================

    private void MoveBoard(int directionX, int directionY)
    {
        if (boardManager == null)
            return;

        if (currentBoardCell != null)
        {
            GridCell activeCell = boardManager.GetCell(
                currentBoardCell.X,
                currentBoardCell.Y
            );

            if (activeCell != currentBoardCell)
                currentBoardCell = activeCell;
        }

        if (currentBoardCell == null)
        {
            currentBoardCell = boardManager.GetFirstBoardCell();

            if (currentBoardCell == null)
                return;

            boardManager.SetSelectedCell(currentBoardCell);
            UpdateExistingBuildingScore();
            ShowPlacementStatus();
            return;
        }

        GridCell nextCell = boardManager.GetNeighbour(
            currentBoardCell,
            directionX,
            directionY
        );

        if (nextCell == null)
            return;

        currentBoardCell = nextCell;

        boardManager.SetSelectedCell(currentBoardCell);
        UpdateExistingBuildingScore();
        ShowPlacementStatus();
    }

    // =========================================================
    // PLACE OR REPLACE
    // =========================================================

    private void ConfirmBoardPosition()
    {
        if (boardManager == null || currentBoardCell == null)
            return;

        if (!currentBoardCell.gameObject.activeInHierarchy)
            return;

        bool successful;

        if (currentBoardCell.IsOccupied)
            successful = boardManager.ReplaceBuilding(selectedBuilding);
        else
            successful = boardManager.PlaceBuilding(selectedBuilding);

        if (!successful)
        {
            ShowBuildingCannotBePlaced();
            return;
        }

        SyncPopulationManager();
        SaveMap();

        ShowBuildingPlaced();

        // A successful placement or replacement always returns
        // the selection to Blue and exits placement mode.
        ResetNavigationToBlue();
    }

    // =========================================================
    // SAVE AND POPULATION
    // =========================================================

    private void SaveMap()
    {
        if (MapSaveManager.Instance != null)
            MapSaveManager.Instance.SaveMap();
        else
            Debug.LogWarning("NavigationManager: MapSaveManager is missing.");
    }

    private void SyncPopulationManager()
    {
        if (PopulationManager.Instance != null && boardManager != null)
        {
            PopulationManager.Instance.SetPopulation(
                boardManager.GetTotalPopulation()
            );
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveMap();
    }

    private void OnApplicationQuit()
    {
        SaveMap();
    }

    // =========================================================
    // PLACEMENT VISUALS AND STATUS
    // =========================================================

    private void ClearPlacementVisuals()
    {
        if (boardManager != null)
            boardManager.EndPlacementMode();
    }

    private void ShowPlacementStatus()
    {
        if (infoPanel == null || boardManager == null ||
            currentBoardCell == null)
            return;

        if (boardManager.CanPlaceOrReplaceBuilding(selectedBuilding))
            infoPanel.ShowBuildingCanBePlaced();
        else
            infoPanel.ShowBuildingCannotBePlaced();
    }

    public void CancelPlacement()
    {
        if (currentMode != NavigationMode.BoardPlacement)
            return;

        currentMode = NavigationMode.BuildingSelection;
        currentBoardCell = null;

        ClearPlacementVisuals();
        ClearExistingBuildingScore();
        UpdateCurrentBuildingScore();
        UpdateBuildingSelectionVisual();

        if (infoPanel != null)
            infoPanel.ShowBuildingCancelled();

        ShowBuildingSelected();
    }

    // =========================================================
    // INFO PANEL
    // =========================================================

    private void ShowBuildingSelected()
    {
        if (infoPanel != null)
            infoPanel.ShowBuildingSelected(GetBuildingName(selectedBuilding));
    }

    private void ShowNoValidPlacement()
    {
        if (infoPanel != null)
            infoPanel.ShowNoValidPlacement(GetBuildingName(selectedBuilding));
    }

    private void ShowBuildingPlaced()
    {
        if (infoPanel != null)
            infoPanel.ShowBuildingPlaced(GetBuildingName(selectedBuilding));
    }

    private void ShowBuildingCannotBePlaced()
    {
        if (infoPanel != null)
            infoPanel.ShowBuildingCannotBePlaced();
    }

    private GridCell.BuildingType GetBuildingType(int buildingIndex)
    {
        switch (buildingIndex)
        {
            case 0: return GridCell.BuildingType.Blue;
            case 1: return GridCell.BuildingType.Red;
            case 2: return GridCell.BuildingType.Green;
            case 3: return GridCell.BuildingType.Yellow;
            default: return GridCell.BuildingType.None;
        }
    }

    private string GetBuildingName(int buildingIndex)
    {
        switch (buildingIndex)
        {
            case 0: return "Blue";
            case 1: return "Red";
            case 2: return "Green";
            case 3: return "Yellow";
            default: return "Unknown";
        }
    }
}
