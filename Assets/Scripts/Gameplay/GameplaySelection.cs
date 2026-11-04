/// <summary>
/// Carries the chosen building from the LevelSelect scene into the
/// Gameplay scene. Static data survives SceneManager.LoadScene.
/// </summary>
public static class GameplaySelection
{
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

    public static bool HasSelection => SelectedBuilding != null;

    public static void Select(
        BuildingGameplayConfig configuration,
        bool specialRoofUnlocked)
    {
        SelectedBuilding = configuration;

        if (configuration == null)
        {
            ClearSelection();
            return;
        }

        BuildingId = configuration.buildingId;
        FloorsToComplete = configuration.floorsToComplete;
        SpecialRoofUnlocked = specialRoofUnlocked;
    }

    // Kept for existing callers (LevelSelectManager).
    public static void Select(BuildingGameplayConfig configuration)
    {
        Select(configuration, false);
    }

    public static void ClearSelection()
    {
        SelectedBuilding = null;
        BuildingId = null;
        FloorsToComplete = 0;
        SpecialRoofUnlocked = false;
    }
}
