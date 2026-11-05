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

    public static bool HasPending =>
        !string.IsNullOrEmpty(PlayerPrefs.GetString(BuildingKey, string.Empty));

    /// <summary>Building id (BuildingGameplayConfig.buildingId).</summary>
    public static string BuildingId => PlayerPrefs.GetString(BuildingKey, string.Empty);

    /// <summary>City the building was earned for.</summary>
    public static string CityId => PlayerPrefs.GetString(CityKey, string.Empty);

    public static void Set(string buildingId, string cityId)
    {
        if (string.IsNullOrWhiteSpace(buildingId))
            return;

        PlayerPrefs.SetString(BuildingKey, buildingId.Trim());
        PlayerPrefs.SetString(CityKey, Normalize(cityId));
        PlayerPrefs.Save();
    }

    public static bool IsForCity(string cityId)
    {
        return HasPending && CityId == Normalize(cityId);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(BuildingKey);
        PlayerPrefs.DeleteKey(CityKey);
        PlayerPrefs.Save();
    }

    // Same id rules as PopulationBonusStore.
    private static string Normalize(string cityId)
    {
        return string.IsNullOrWhiteSpace(cityId) ? "default" : cityId.Trim();
    }
}
