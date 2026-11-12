
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

        // The guide and messages quote floors and roof scores from these.
        GuideContent.RegisterBuildingConfigs(buildingConfigurations);
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

        // The new city's map is loaded during the switch; if a building
        // was earned for this city, go straight to placing it.
        StartCoroutine(OpenPendingPlacementNextFrame());
    }

    private void Start()
    {
        FindReferences();
        SubscribeToCityManager();

        ResetNavigationToBlue();

        if (infoPanel != null)
            infoPanel.SetIdleHint(GetNextStepHint);

        Debug.Log("Navigation started. Building Selection Mode.");

        StartCoroutine(OpenPendingPlacementNextFrame());
    }

    // =========================================================
    // PENDING PLACEMENT (building earned in Gameplay)
    // =========================================================

    private bool HasPendingPlacementForActiveCity =>
        PendingPlacementStore.IsForCity(PopulationBonusStore.ActiveCityId);

    // Waits a frame so MapSaveManager has restored the board first;
    // placement validity depends on the buildings already placed.
    private System.Collections.IEnumerator OpenPendingPlacementNextFrame()
    {
        yield return null;

        TryOpenPendingPlacement();
    }

    // Completing a building in Gameplay earns it: LevelSelect opens
    // board placement for that building straight away.
    private bool TryOpenPendingPlacement()
    {
        if (isLoadingGameplay || !HasPendingPlacementForActiveCity)
            return false;

        if (currentMode == NavigationMode.BoardPlacement)
            return true;

        int buildingIndex = GetBuildingIndex(PendingPlacementStore.BuildingId);

        if (buildingIndex < 0)
        {
            Debug.LogWarning(
                "NavigationManager: Pending building '" +
                PendingPlacementStore.BuildingId +
                "' has no entry in Building Configurations. Discarding it.",
                this
            );

            PendingPlacementStore.Clear();
            return false;
        }

        selectedBuilding = buildingIndex;

        UpdateBuildingSelectionVisual();
        UpdateCurrentBuildingScore();

        EnterBoardPlacementMode(requireUnlock: false);

        if (currentMode == NavigationMode.BoardPlacement)
        {
            Debug.Log(
                "NavigationManager: Placing earned " +
                GetBuildingName(buildingIndex) + " building."
            );

            if (infoPanel != null)
                infoPanel.ShowPlacingBuilding(buildingIndex, PendingPlacementStore.Score);

            // First earned building: explain placing, replacing and Cancel Build.
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.TryShowTutorial(GuideContent.PlaceBuilding, buildingIndex);

            return true;
        }

        // Nowhere on the board can take it (EnterBoardPlacementMode has
        // already shown "no valid placement"). Drop it so the player
        // isn't locked in; the score was still added to population.
        Debug.LogWarning(
            "NavigationManager: No valid cell for earned " +
            GetBuildingName(buildingIndex) + " building. Discarding it.",
            this
        );

        PendingPlacementStore.Clear();
        return false;
    }

    private int GetBuildingIndex(string buildingId)
    {
        if (string.IsNullOrWhiteSpace(buildingId) ||
            buildingConfigurations == null)
        {
            return -1;
        }

        for (int i = 0; i < buildingConfigurations.Length; i++)
        {
            BuildingGameplayConfig configuration = buildingConfigurations[i];

            if (configuration != null &&
                string.Equals(
                    configuration.buildingId,
                    buildingId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
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

        // Don't cover a "placed" / "reset" message; between messages the
        // panel shows what to do next.
        if (infoPanel != null)
            infoPanel.RefreshIdleHint();

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

        // A building is worth the score it was built with, so there is only
        // a number to show once one has been earned and is being placed.
        if (HasPendingPlacementForActiveCity &&
            GetBuildingIndex(PendingPlacementStore.BuildingId) == selectedBuilding)
        {
            BuildingScoreManager.Instance.SetCurrentBuildingScore(
                PendingPlacementStore.Score
            );
        }
        else
        {
            BuildingScoreManager.Instance.ClearCurrentBuildingScore();
        }
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

        // The score that building was built with.
        BuildingScoreManager.Instance.SetExistingBuildingScore(
            currentBoardCell.BuildingPopulation
        );
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
        {
            // An earned building must be placed before playing again.
            // If it couldn't be placed, that press only shows the message.
            if (HasPendingPlacementForActiveCity)
            {
                if (!TryOpenPendingPlacement() && HasPendingPlacementForActiveCity && infoPanel != null)
                    infoPanel.ShowMustPlaceFirst(selectedBuilding);

                return;
            }

            LaunchGameplayForSelectedBuilding();
        }
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

        // The finished building has to go on the board, so don't start a
        // game for one that has nowhere to be placed.
        FindReferences();

        if (boardManager == null ||
            !boardManager.HasValidPlacementOrReplacement(selectedBuilding))
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

        GameplaySelection.Select(
            configuration,
            specialRoofUnlocked,
            PopulationBonusStore.ActiveCityId
        );

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

    // requireUnlock is false for a building earned in Gameplay: the player
    // already built it, so it is placed regardless of unlock state.
    private void EnterBoardPlacementMode(bool requireUnlock = true)
    {
        FindReferences();

        if (boardManager == null)
        {
            Debug.LogWarning("NavigationManager: BoardManager is missing.");
            return;
        }

        if (requireUnlock && !IsBuildingUnlocked(selectedBuilding))
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

        // The building's population is the score earned building it.
        int earnedScore = HasPendingPlacementForActiveCity
            ? PendingPlacementStore.Score
            : 0;

        bool replacing = currentBoardCell.IsOccupied;
        int oldBuilding = GuideContent.IndexOf(currentBoardCell.CurrentBuilding);
        int oldResidents = currentBoardCell.BuildingPopulation;

        bool successful;

        if (replacing)
            successful = boardManager.ReplaceBuilding(selectedBuilding, earnedScore);
        else
            successful = boardManager.PlaceBuilding(selectedBuilding, earnedScore);

        if (!successful)
        {
            ShowBuildingCannotBePlaced();
            return;
        }

        // The earned building is now on the board.
        PendingPlacementStore.Clear();

        SyncPopulationManager();
        SaveMap();

        if (infoPanel != null)
        {
            if (replacing)
                infoPanel.ShowBuildingReplaced(selectedBuilding, oldBuilding, earnedScore - oldResidents);
            else
                infoPanel.ShowBuildingPlaced(selectedBuilding, earnedScore);
        }

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
        {
            if (currentBoardCell.IsOccupied)
            {
                infoPanel.ShowBuildingCanReplace(
                    selectedBuilding,
                    GuideContent.IndexOf(currentBoardCell.CurrentBuilding),
                    currentBoardCell.BuildingPopulation
                );
            }
            else
            {
                infoPanel.ShowBuildingCanBePlaced(selectedBuilding);
            }

            return;
        }

        // Say why: which neighbours this square is missing.
        infoPanel.ShowBuildingCannotBePlaced(
            selectedBuilding,
            ToIndexes(boardManager.GetMissingNeighbours(currentBoardCell, selectedBuilding))
        );
    }

    public void CancelPlacement()
    {
        if (currentMode != NavigationMode.BoardPlacement)
            return;

        int lostResidents = HasPendingPlacementForActiveCity
            ? PendingPlacementStore.Score
            : 0;

        bool discardedEarnedBuilding = DiscardPendingPlacement();

        currentMode = NavigationMode.BuildingSelection;
        currentBoardCell = null;

        ClearPlacementVisuals();
        ClearExistingBuildingScore();
        UpdateCurrentBuildingScore();
        UpdateBuildingSelectionVisual();

        if (discardedEarnedBuilding)
        {
            if (infoPanel != null)
                infoPanel.ShowBuildingDiscarded(selectedBuilding, lostResidents);

            return;
        }

        ShowBuildingSelected();
    }

    // Cancelling an earned building throws it away, and the score it would
    // have added to population with it. Nothing was added yet, so there is
    // nothing to take back.
    private bool DiscardPendingPlacement()
    {
        if (!HasPendingPlacementForActiveCity)
            return false;

        int lostScore = PendingPlacementStore.Score;

        PendingPlacementStore.Clear();
        SaveMap();

        Debug.Log(
            "NavigationManager: Earned " + GetBuildingName(selectedBuilding) +
            " building discarded with its " + lostScore + " population."
        );

        return true;
    }

    // =========================================================
    // INFO PANEL
    // =========================================================

    private void ShowBuildingSelected()
    {
        if (infoPanel != null)
            infoPanel.ShowBuildingSelected(selectedBuilding);
    }

    private void ShowNoValidPlacement()
    {
        if (infoPanel == null)
            return;

        System.Collections.Generic.List<int> missingFromCity =
            boardManager != null
                ? ToIndexes(boardManager.GetRequiredTypesMissingFromBoard(selectedBuilding))
                : new System.Collections.Generic.List<int>();

        infoPanel.ShowNoValidPlacement(selectedBuilding, missingFromCity);
    }

    private void ShowBuildingCannotBePlaced()
    {
        if (infoPanel == null)
            return;

        System.Collections.Generic.List<int> missing =
            boardManager != null && currentBoardCell != null
                ? ToIndexes(boardManager.GetMissingNeighbours(currentBoardCell, selectedBuilding))
                : null;

        infoPanel.ShowBuildingCannotBePlaced(selectedBuilding, missing);
    }

    private static System.Collections.Generic.List<int> ToIndexes(
        System.Collections.Generic.List<GridCell.BuildingType> types)
    {
        System.Collections.Generic.List<int> indexes = new System.Collections.Generic.List<int>();

        if (types == null)
            return indexes;

        foreach (GridCell.BuildingType type in types)
            indexes.Add(GuideContent.IndexOf(type));

        return indexes;
    }

    /// <summary>
    /// The standing hint in the Info Panel between messages: the one thing
    /// the player should do next.
    /// </summary>
    private string GetNextStepHint()
    {
        if (currentMode == NavigationMode.BoardPlacement)
        {
            return "Arrows: pick a square. OK: place it. " +
                   "Cancel Build: throw this building away.";
        }

        if (HasPendingPlacementForActiveCity)
        {
            int pending = GetBuildingIndex(PendingPlacementStore.BuildingId);
            return "Your new " + GuideContent.Coloured(pending) +
                   " building is waiting. Press OK to place it.";
        }

        if (boardManager != null && boardManager.GetTotalBuildingCount() == 0)
        {
            return "Start here: press OK to build a " + GuideContent.Coloured(0) + " building (" +
                   GuideContent.Floors(0) + " floors), then place it in your city.";
        }

        int population = GuideContent.CurrentPopulation();

        if (GuideContent.TryGetNextReward(
                out PopulationProgressionManager.RewardType reward, out int needed))
        {
            return "Next goal: " + GuideContent.RewardName(reward) + " at " + needed.ToString("N0") +
                   " residents (you have " + population.ToString("N0") +
                   "). Pick a building and press OK.";
        }

        if (CityManager.Instance != null &&
            CityManager.Instance.TryGetNextCity(out string cityName, out int cityPopulation, out bool cityUnlocked) &&
            !cityUnlocked)
        {
            return "Next goal: reach " + cityPopulation.ToString("N0") + " residents to unlock " +
                   cityName + " (you have " + population.ToString("N0") + ").";
        }

        return "Everything here is unlocked! Replace small buildings with bigger ones to grow even more.";
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
