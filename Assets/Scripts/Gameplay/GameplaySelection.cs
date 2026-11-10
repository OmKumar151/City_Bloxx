/// <summary>
/// Carries the chosen building from the LevelSelect scene into the
/// Gameplay scene. Static data survives SceneManager.LoadScene.
/// </summary>
public static class GameplaySelection
{
    /// <summary>
    /// Floor count used by Quick Play: the building never reaches its roof,
    /// so the player keeps stacking until they run out of lives.
    /// </summary>
    public const int EndlessFloors = int.MaxValue;

    /// <summary>The full configuration (sprites, prefab, thresholds).</summary>
    public static BuildingGameplayConfig SelectedBuilding { get; private set; }

    /// <summary>Building id, e.g. "blue", "red", "green", "yellow".</summary>
    public static string BuildingId { get; private set; }

    /// <summary>Number of floors the player must stack to finish.</summary>
    public static int FloorsToComplete { get; private set; }

    /// <summary>
    /// True when LevelSelect says the special roof is unlocked for this
    /// building (its colour's Star reward). Gameplay still requires the
    /// score threshold to be reached before the special roof is shown.
    /// </summary>
    public static bool SpecialRoofUnlocked { get; private set; }

    /// <summary>
    /// City the player launched from. A win's score is added to this
    /// city's population (CityManager doesn't survive the scene change).
    /// </summary>
    public static string CityId { get; private set; }

    /// <summary>
    /// True when the game was started from the main menu's Quick Play
    /// button: endless floors, no city, no roof.
    /// </summary>
    public static bool IsQuickPlay { get; private set; }

    public static bool HasSelection => SelectedBuilding != null;

    public static void Select(
        BuildingGameplayConfig configuration,
        bool specialRoofUnlocked,
        string cityId = null)
    {
        // Any normal (LevelSelect) launch leaves Quick Play.
        IsQuickPlay = false;

        SelectedBuilding = configuration;

        if (configuration == null)
        {
            ClearSelection();
            return;
        }

        BuildingId = configuration.buildingId;
        FloorsToComplete = configuration.floorsToComplete;
        SpecialRoofUnlocked = specialRoofUnlocked;
        CityId = cityId;
    }

    // Kept for existing callers (LevelSelectManager).
    public static void Select(BuildingGameplayConfig configuration)
    {
        Select(configuration, false);
    }

    /// <summary>
    /// Main menu Quick Play: no building yet. The Gameplay scene shows the
    /// colour picker and calls SelectQuickPlayBuilding.
    /// </summary>
    public static void StartQuickPlay()
    {
        ClearSelection();
        IsQuickPlay = true;
    }

    /// <summary>Quick Play colour chosen: endless floors of this building.</summary>
    public static void SelectQuickPlayBuilding(BuildingGameplayConfig configuration)
    {
        if (configuration == null)
        {
            StartQuickPlay();
            return;
        }

        SelectedBuilding = configuration;
        BuildingId = configuration.buildingId;
        FloorsToComplete = EndlessFloors;
        SpecialRoofUnlocked = false;
        CityId = null;
        IsQuickPlay = true;
    }

    public static void ClearSelection()
    {
        SelectedBuilding = null;
        BuildingId = null;
        FloorsToComplete = 0;
        SpecialRoofUnlocked = false;
        CityId = null;
        IsQuickPlay = false;
    }
}
