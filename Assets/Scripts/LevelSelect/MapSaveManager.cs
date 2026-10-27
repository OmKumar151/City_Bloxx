
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSaveManager : MonoBehaviour
{
    public static MapSaveManager Instance { get; private set; }

    private const string SaveKeyPrefix = "CityBloxx_MapSave_";
    private const string DefaultCityId = "default";
    private const string LegacySaveKey = "CityBloxx_MapSave";

    [Serializable]
    private class MapSaveData
    {
        public List<BuildingSaveData> buildings =
            new List<BuildingSaveData>();
    }

    [Serializable]
    private class BuildingSaveData
    {
        public int x;
        public int y;
        public GridCell.BuildingType buildingType;
        public int population;
    }

    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private InfoPanelUI infoPanel;
    [SerializeField] private bool loadOnStart = false;

    public string ActiveSaveKey => SaveKeyPrefix + GetCurrentCityId();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        FindReferences();
    }

    private void Start()
    {
        if (loadOnStart)
            LoadMap();
    }

    private void FindReferences()
    {
        if (boardManager == null)
            boardManager = FindAnyObjectByType<BoardManager>();

        if (infoPanel == null)
            infoPanel = FindAnyObjectByType<InfoPanelUI>();
    }

    private string GetCurrentCityId()
    {
        if (CityManager.Instance == null)
            return DefaultCityId;

        string id = CityManager.Instance.ActiveCityId;

        return string.IsNullOrWhiteSpace(id)
            ? DefaultCityId
            : id.Trim();
    }

    // =========================================================
    // SAVE
    // =========================================================

    public void SaveMap()
    {
        FindReferences();

        if (boardManager == null)
        {
            Debug.LogWarning("MapSaveManager: BoardManager is missing.");
            return;
        }

        MapSaveData data = new MapSaveData();

        foreach (GridCell cell in boardManager.GetAllActiveCells())
        {
            if (cell == null || !cell.IsOccupied)
                continue;

            data.buildings.Add(new BuildingSaveData
            {
                x = cell.X,
                y = cell.Y,
                buildingType = cell.CurrentBuilding,
                population = cell.BuildingPopulation
            });
        }

        PlayerPrefs.SetString(ActiveSaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();

        Debug.Log(
            "MapSaveManager: Saved city '" + GetCurrentCityId() +
            "' with " + data.buildings.Count + " buildings."
        );
    }

    // =========================================================
    // LOAD
    // =========================================================

    public void LoadMap()
    {
        FindReferences();

        if (boardManager == null)
        {
            Debug.LogWarning("MapSaveManager: BoardManager is missing.");
            return;
        }

        string key = ActiveSaveKey;

        if (!PlayerPrefs.HasKey(key) &&
            CityManager.Instance != null &&
            CityManager.Instance.ActiveCityIndex == 0 &&
            PlayerPrefs.HasKey(LegacySaveKey))
        {
            PlayerPrefs.SetString(
                key,
                PlayerPrefs.GetString(LegacySaveKey)
            );

            PlayerPrefs.Save();
        }

        if (!PlayerPrefs.HasKey(key))
        {
            ClearGridAndResetPopulation();

            Debug.Log(
                "MapSaveManager: No saved map for city '" +
                GetCurrentCityId() + "'. Started empty."
            );

            return;
        }

        MapSaveData data;

        try
        {
            data = JsonUtility.FromJson<MapSaveData>(
                PlayerPrefs.GetString(key)
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "MapSaveManager: Could not read city save. " +
                exception.Message
            );
            return;
        }

        if (data == null)
        {
            Debug.LogWarning(
                "MapSaveManager: Save data was invalid. Clearing the map."
            );

            ClearGridAndResetPopulation();
            return;
        }

        boardManager.ClearAllBuildings();

        if (data.buildings != null)
        {
            foreach (BuildingSaveData building in data.buildings)
            {
                if (building == null ||
                    building.buildingType == GridCell.BuildingType.None)
                {
                    continue;
                }

                GridCell cell = boardManager.GetCell(building.x, building.y);

                if (cell == null)
                {
                    Debug.LogWarning(
                        "MapSaveManager: Saved cell (" +
                        building.x + ", " + building.y +
                        ") does not exist in the active city."
                    );
                    continue;
                }

                boardManager.RestoreBuilding(
                    cell,
                    building.buildingType,
                    Mathf.Max(0, building.population)
                );
            }
        }

        boardManager.RefreshBoardAnalysis();

        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.SetPopulation(
                boardManager.GetTotalPopulation()
            );
        }

        RefreshProgression();

        Debug.Log(
            "MapSaveManager: Loaded city '" + GetCurrentCityId() +
            "' with " +
            (data.buildings == null ? 0 : data.buildings.Count) +
            " saved building entries."
        );
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetMap()
    {
        FindReferences();

        if (boardManager == null)
        {
            Debug.LogWarning(
                "MapSaveManager: BoardManager is missing; reset cancelled."
            );
            return;
        }

        ClearGridAndResetPopulation();

        NavigationManager navigation =
            FindAnyObjectByType<NavigationManager>();

        if (navigation != null)
            navigation.ResetNavigationToBlue();

        SaveMap();

        if (infoPanel != null)
            infoPanel.ShowMapReset();

        Debug.Log(
            "MapSaveManager: Reset city '" + GetCurrentCityId() +
            "'. The map was saved empty."
        );
    }

    private void ClearGridAndResetPopulation()
    {
        FindReferences();

        if (boardManager != null)
            boardManager.ClearAllBuildings();

        if (PopulationManager.Instance != null)
            PopulationManager.Instance.SetPopulation(0);

        RefreshProgression();
    }

    private void RefreshProgression()
    {
        if (PopulationProgressionManager.Instance != null)
            PopulationProgressionManager.Instance.ResetProgression();
    }

    // =========================================================
    // DELETE SAVE
    // =========================================================

    public void DeleteMap()
    {
        PlayerPrefs.DeleteKey(ActiveSaveKey);
        PlayerPrefs.Save();

        Debug.Log(
            "MapSaveManager: Deleted save for city '" +
            GetCurrentCityId() + "'."
        );
    }

    public bool HasSavedMap()
    {
        return PlayerPrefs.HasKey(ActiveSaveKey);
    }

    public void ClearCurrentGridBuildings()
    {
        FindReferences();

        if (boardManager == null)
            return;

        boardManager.ClearAllBuildings();
        boardManager.RefreshBoardAnalysis();
    }

    public void LoadMapForActiveCity()
    {
        LoadMap();
    }

    public void SaveCurrentCity()
    {
        SaveMap();
    }
}
