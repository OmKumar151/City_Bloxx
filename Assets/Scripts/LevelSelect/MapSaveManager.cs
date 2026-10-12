
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSaveManager : MonoBehaviour
{
    public static MapSaveManager Instance { get; private set; }

    private const string SaveKey = "CityBloxx_MapSave";

    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private InfoPanelUI infoPanelUI;

    [Serializable]
    private class BuildingSaveData
    {
        public int x;
        public int y;
        public GridCell.BuildingType buildingType;
        public int population;
    }

    [Serializable]
    private class MapSaveData
    {
        public List<BuildingSaveData> buildings =
            new List<BuildingSaveData>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (infoPanelUI == null)
        {
            infoPanelUI = FindObjectOfType<InfoPanelUI>();
        }
    }

    private void Start()
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "MapSaveManager: BoardManager is not assigned."
            );
            return;
        }

        LoadMap();
    }

    public void SaveMap()
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "MapSaveManager: Cannot save because BoardManager is not assigned."
            );
            return;
        }

        MapSaveData saveData = new MapSaveData();

        foreach (GridCell cell in boardManager.GetAllActiveCells())
        {
            if (cell == null || !cell.IsOccupied)
            {
                continue;
            }

            BuildingSaveData buildingData = new BuildingSaveData
            {
                x = cell.X,
                y = cell.Y,
                buildingType = cell.CurrentBuilding,
                population = cell.BuildingPopulation
            };

            saveData.buildings.Add(buildingData);
        }

        string json = JsonUtility.ToJson(saveData);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log(
            "MapSaveManager: Map saved. Buildings saved: " +
            saveData.buildings.Count
        );
    }

    public void LoadMap()
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "MapSaveManager: Cannot load because BoardManager is not assigned."
            );
            return;
        }

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log(
                "MapSaveManager: No saved map found. Starting from state 0."
            );

            ResetPopulationAndProgression();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning(
                "MapSaveManager: Saved map data is empty."
            );

            ResetPopulationAndProgression();
            return;
        }

        MapSaveData saveData =
            JsonUtility.FromJson<MapSaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning(
                "MapSaveManager: Could not read saved map data."
            );

            ResetPopulationAndProgression();
            return;
        }

        boardManager.ClearAllBuildings();

        if (saveData.buildings != null)
        {
            foreach (BuildingSaveData buildingData in saveData.buildings)
            {
                GridCell cell = boardManager.GetCell(
                    buildingData.x,
                    buildingData.y
                );

                if (cell == null)
                {
                    Debug.LogWarning(
                        "MapSaveManager: Could not find GridCell at (" +
                        buildingData.x + ", " + buildingData.y + ")."
                    );
                    continue;
                }

                boardManager.RestoreBuilding(
                    cell,
                    buildingData.buildingType,
                    buildingData.population
                );
            }
        }

        SyncPopulation();

        Debug.Log(
            "MapSaveManager: Map loaded. Buildings restored: " +
            (saveData.buildings != null ? saveData.buildings.Count : 0)
        );
    }

    public void ResetMap()
    {
        if (boardManager == null)
        {
            Debug.LogWarning(
                "MapSaveManager: Cannot reset because BoardManager is not assigned."
            );
            return;
        }

        boardManager.ClearAllBuildings();

        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.ResetPopulation();
        }

        if (PopulationProgressionManager.Instance != null)
        {
            PopulationProgressionManager.Instance.ResetProgression();
        }

        SaveMap();

        // Do this last so the reset message and normal panel colour
        // replace any reward message triggered during progression reset.
        if (infoPanelUI == null)
        {
            infoPanelUI = FindObjectOfType<InfoPanelUI>();
        }

        if (infoPanelUI != null)
        {
            infoPanelUI.ShowMapReset();
        }

        Debug.Log("MapSaveManager: Map reset to state 0.");
    }

    private void SyncPopulation()
    {
        if (PopulationManager.Instance == null)
        {
            return;
        }

        PopulationManager.Instance.SetPopulation(
            boardManager.GetTotalPopulation()
        );
    }

    private void ResetPopulationAndProgression()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.ResetPopulation();
        }

        if (PopulationProgressionManager.Instance != null)
        {
            PopulationProgressionManager.Instance.ResetProgression();
        }
    }

    public void DeleteSave()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();

            Debug.Log("MapSaveManager: Save deleted.");
        }
    }
}
