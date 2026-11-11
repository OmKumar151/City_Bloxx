using UnityEngine;

/// <summary>
/// A building the player has earned by completing it in Gameplay but has
/// not placed on the board yet. Saved in PlayerPrefs so quitting during
/// placement doesn't lose it. LevelSelect (NavigationManager) opens board
/// placement for it on load.
/// </summary>
public static class PendingPlacementStore
{
    private const string BuildingKey = "CityBloxx_PendingPlacement_Building";
    private const string CityKey = "CityBloxx_PendingPlacement_City";
    private const string ScoreKey = "CityBloxx_PendingPlacement_Score";

    public static bool HasPending =>
        !string.IsNullOrEmpty(PlayerPrefs.GetString(BuildingKey, string.Empty));

    /// <summary>Building id (BuildingGameplayConfig.buildingId).</summary>
    public static string BuildingId => PlayerPrefs.GetString(BuildingKey, string.Empty);

    /// <summary>City the building was earned for.</summary>
    public static string CityId => PlayerPrefs.GetString(CityKey, string.Empty);

    /// <summary>
    /// Gameplay score that was added to population with this building.
    /// Removed from population again if the placement is cancelled.
    /// </summary>
    public static int Score => Mathf.Max(0, PlayerPrefs.GetInt(ScoreKey, 0));

    public static void Set(string buildingId, string cityId, int score)
    {
        if (string.IsNullOrWhiteSpace(buildingId))
            return;

        SaveStore.SetString(BuildingKey, buildingId.Trim());
        SaveStore.SetString(CityKey, Normalize(cityId));
        SaveStore.SetInt(ScoreKey, Mathf.Max(0, score));
        SaveStore.Save();
    }

    public static bool IsForCity(string cityId)
    {
        return HasPending && CityId == Normalize(cityId);
    }

    public static void Clear()
    {
        SaveStore.DeleteKey(BuildingKey);
        SaveStore.DeleteKey(CityKey);
        SaveStore.DeleteKey(ScoreKey);
        SaveStore.Save();
    }

    // Same id rules as PopulationBonusStore.
    private static string Normalize(string cityId)
    {
        return string.IsNullOrWhiteSpace(cityId) ? "default" : cityId.Trim();
    }
}
