using UnityEngine;

/// <summary>
/// Population earned from gameplay scores, saved per city.
/// BoardManager.GetTotalPopulation() adds this on top of the population
/// from placed buildings, so it shows everywhere population is used
/// (top bar, progression rewards, city unlocks).
/// </summary>
public static class PopulationBonusStore
{
    private const string KeyPrefix = "CityBloxx_PopulationBonus_";
    private const string DefaultCityId = "default";

    /// <summary>Same id rules as MapSaveManager uses for map saves.</summary>
    public static string ActiveCityId
    {
        get
        {
            string id = CityManager.Instance != null
                ? CityManager.Instance.ActiveCityId
                : null;

            return Normalize(id);
        }
    }

    public static int Get(string cityId)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(KeyPrefix + Normalize(cityId), 0));
    }

    public static int GetForActiveCity()
    {
        return Get(ActiveCityId);
    }

    public static void Add(string cityId, int amount)
    {
        if (amount <= 0)
            return;

        string key = KeyPrefix + Normalize(cityId);

        // long math guards against int overflow on very large totals.
        long total = (long)PlayerPrefs.GetInt(key, 0) + amount;
        PlayerPrefs.SetInt(key, (int)Mathf.Min(total, int.MaxValue));
        PlayerPrefs.Save();

        Debug.Log(
            "PopulationBonusStore: +" + amount + " population for city '" +
            Normalize(cityId) + "'. Bonus total: " + Get(cityId)
        );
    }

    public static void Clear(string cityId)
    {
        PlayerPrefs.DeleteKey(KeyPrefix + Normalize(cityId));
        PlayerPrefs.Save();
    }

    private static string Normalize(string cityId)
    {
        return string.IsNullOrWhiteSpace(cityId)
            ? DefaultCityId
            : cityId.Trim();
    }
}
