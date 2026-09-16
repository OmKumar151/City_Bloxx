using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSaveManager : MonoBehaviour
{
    public static MapSaveManager Instance { get; private set; }

    [Header("Map")]
    [SerializeField] private string mapID = "CityBloxx_Map_01";

    [Header("References")]
    [SerializeField] private BoardManager boardManager;

    [SerializeField] private PopulationManager populationManager;

    [SerializeField]
    private PopulationProgressionManager progressionManager;

    private const string SavePrefix =
        "CityBloxx_Save_";

    private string SaveKey =>
        SavePrefix + mapID;

    private string lastSavedState = "";

    private bool isLoading;

    [Serializable]
    private class MapSaveData
    {
        public int population;

        public List<CellSaveData> cells =
            new List<CellSaveData>();
    }

    [Serializable]
    private class CellSaveData
    {
        public int x;
        public int y;

        public GridCell.BuildingType buildingType;

        public int buildingPopulation;
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        FindReferences();

        LoadMap();
    }

    private void Update()
    {
        if (isLoading)
        {
            return;
        }

        if (boardManager == null)
        {
            return;
        }

        string currentState =
            CreateStateSignature();

        if (currentState != lastSavedState)
        {
            SaveMap();
        }
    }

    // =========================================================
    // REFERENCES
    // =========================================================

    private void FindReferences()
    {
        if (boardManager == null)
        {
            boardManager =
                FindFirstObjectByType<BoardManager>();
        }

        if (populationManager == null)
        {
            populationManager =
                PopulationManager.Instance;
        }

        if (progressionManager == null)
        {
            progressionManager =
                PopulationProgressionManager.Instance;
        }
    }

    // =========================================================
    // SAVE
    // =========================================================

    public void SaveMap()
    {
        if (isLoading)
        {
            return;
        }

        FindReferences();

        if (boardManager == null)
        {
            Debug.LogWarning(
                "MapSaveManager: " +
                "BoardManager is missing."
            );

            return;
        }

        MapSaveData saveData =
            new MapSaveData();

        if (populationManager != null)
        {
            saveData.population =
                populationManager.TotalPopulation;
        }
        else
        {
            saveData.population =
                boardManager.GetTotalPopulation();
        }

        foreach (
            GridCell cell
            in boardManager.GetAllActiveCells())
        {
            if (cell == null)
            {
                continue;
            }

            if (!cell.IsOccupied)
            {
                continue;
            }

            CellSaveData cellData =
                new CellSaveData();

            cellData.x = cell.X;
            cellData.y = cell.Y;
            cellData.buildingType =
                cell.CurrentBuilding;
            cellData.buildingPopulation =
                cell.BuildingPopulation;

            saveData.cells.Add(cellData);
        }

        string json =
            JsonUtility.ToJson(
                saveData
            );

        PlayerPrefs.SetString(
            SaveKey,
            json
        );

        PlayerPrefs.Save();

        lastSavedState =
            CreateStateSignature();

        Debug.Log(
            "MapSaveManager: Map saved. " +
            "Population: " +
            saveData.population +
            " | Buildings: " +
            saveData.cells.Count
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
            Debug.LogWarning(
                "MapSaveManager: " +
                "BoardManager is missing."
            );

            return;
        }

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log(
                "MapSaveManager: No save found for " +
                mapID +
                ". Starting new map."
            );

            lastSavedState =
                CreateStateSignature();

            return;
        }

        string json =
            PlayerPrefs.GetString(
                SaveKey
            );

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning(
                "MapSaveManager: Save data is empty."
            );

            return;
        }

        MapSaveData saveData;

        try
        {
            saveData =
                JsonUtility.FromJson<MapSaveData>(
                    json
                );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "MapSaveManager: Failed to load save. " +
                exception.Message
            );

            return;
        }

        if (saveData == null)
        {
            Debug.LogWarning(
                "MapSaveManager: Save data could not be read."
            );

            return;
        }

        isLoading = true;

        // Clear current board first.
        boardManager.ClearAllBuildings();

        // Restore population.
        if (populationManager != null)
        {
            populationManager.SetPopulation(
                saveData.population
            );
        }

        // Restore buildings.
        if (saveData.cells != null)
        {
            foreach (
                CellSaveData cellData
                in saveData.cells)
            {
                GridCell cell =
                    boardManager.GetCell(
                        cellData.x,
                        cellData.y
                    );

                if (cell == null)
                {
                    Debug.LogWarning(
                        "MapSaveManager: Saved cell (" +
                        cellData.x +
                        ", " +
                        cellData.y +
                        ") no longer exists."
                    );

                    continue;
                }

                boardManager.RestoreBuilding(
                    cell,
                    cellData.buildingType,
                    cellData.buildingPopulation
                );
            }
        }

        // Make sure population matches the actual board.
        if (populationManager != null)
        {
            populationManager.SetPopulation(
                boardManager.GetTotalPopulation()
            );
        }

        isLoading = false;

        // Recalculate progression from restored population.
        FindReferences();

        if (progressionManager != null)
        {
            progressionManager.ForceCheckProgression();
        }

        lastSavedState =
            CreateStateSignature();

        Debug.Log(
            "MapSaveManager: Map loaded. " +
            "Population: " +
            boardManager.GetTotalPopulation() +
            " | Buildings: " +
            boardManager.GetTotalBuildingCount()
        );
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetMap()
    {
        FindReferences();

        isLoading = true;

        if (boardManager != null)
        {
            boardManager.ClearAllBuildings();
        }

        if (populationManager != null)
        {
            populationManager.SetPopulation(0);
        }

        if (progressionManager != null)
        {
            progressionManager.ResetProgression();
        }

        isLoading = false;

        SaveMap();

        Debug.Log(
            "MapSaveManager: Map reset to State 0."
        );
    }

    // =========================================================
    // SAVE EXISTENCE
    // =========================================================

    public bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    // =========================================================
    // DELETE SAVE
    // =========================================================

    public void DeleteSave()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        lastSavedState = "";

        Debug.Log(
            "MapSaveManager: Save deleted for " +
            mapID
        );
    }

    // =========================================================
    // STATE SIGNATURE
    // =========================================================

    private string CreateStateSignature()
    {
        if (boardManager == null)
        {
            return "";
        }

        string state = "";

        foreach (
            GridCell cell
            in boardManager.GetAllActiveCells())
        {
            if (cell == null)
            {
                continue;
            }

            state +=
                cell.X +
                "," +
                cell.Y +
                ":" +
                (int)cell.CurrentBuilding +
                ":" +
                cell.BuildingPopulation +
                ";";
        }

        state +=
            "|Population:" +
            boardManager.GetTotalPopulation();

        return state;
    }
}