
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [Min(1)] public int startingLives = 3;
    [Min(1)] public int floorsToComplete = 30;

    [Header("Scoring")]
    [Min(0)] public int baseScorePerFloor = 100;
    [Min(0)] public int perfectLandingBonus = 100;
    [Range(0f, 1f)] public float perfectAccuracyThreshold = 0.9f;
    [Min(1)] public int maxCombo = 5;
    [Min(0f)] public float perfectLandingDistance = 0.25f;

    [Header("Building")]
    public GameObject startingBuilding;

    [Header("Perfect Streak")]
    public PerfectStreakTimer perfectStreakTimer;

    [Header("Runtime State")]
    public int score;
    public int lives;
    public int floorsBuilt;
    public int comboCount;
    public int totalPerfectLandings;
    public bool isGameOver;
    public bool isBuildingComplete;

    public GameObject LastPlacedBlock { get; private set; }
    public BuildingGameplayConfig ActiveConfiguration { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (perfectStreakTimer == null)
            perfectStreakTimer = FindObjectOfType<PerfectStreakTimer>();

        ActiveConfiguration = GameplaySelection.SelectedBuilding;

        ResetGameState();
        FindStartingBuilding();
        RefreshUI();
    }

    private void Start()
    {
        if (startingBuilding == null)
            FindStartingBuilding();

        if (startingBuilding != null)
            LastPlacedBlock = startingBuilding;
    }

    public void ResetGameState()
    {
        score = 0;
        lives = startingLives;
        floorsBuilt = 0;
        comboCount = 0;
        totalPerfectLandings = 0;

        isGameOver = false;
        isBuildingComplete = false;

        Time.timeScale = 1f;

        if (perfectStreakTimer != null)
            perfectStreakTimer.StopTimer();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideEndPanels();
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateLives(lives);
            UIManager.Instance.UpdateFloors(floorsBuilt, floorsToComplete);
        }
    }

    private void FindStartingBuilding()
    {
        if (startingBuilding != null)
        {
            LastPlacedBlock = startingBuilding;
            return;
        }

        GameObject[] buildings;

        try
        {
            buildings = GameObject.FindGameObjectsWithTag("Building");
        }
        catch (UnityException)
        {
            Debug.LogWarning(
                "GameManager: The Building tag does not exist. " +
                "Assign Starting Building in the Inspector.",
                this
            );
            return;
        }

        if (buildings == null || buildings.Length == 0)
        {
            Debug.LogWarning(
                "GameManager: No starting building was found.",
                this
            );
            return;
        }

        GameObject highestBuilding = buildings[0];

        foreach (GameObject building in buildings)
        {
            if (building.transform.position.y >
                highestBuilding.transform.position.y)
            {
                highestBuilding = building;
            }
        }

        startingBuilding = highestBuilding;
        LastPlacedBlock = highestBuilding;
    }

    public void SetLastPlacedBlock(GameObject block)
    {
        if (block != null)
            LastPlacedBlock = block;
    }

    public void AddFloor(float accuracy)
    {
        if (isGameOver || isBuildingComplete)
            return;

        floorsBuilt++;
        accuracy = Mathf.Clamp01(accuracy);

        bool isPerfect = accuracy >= perfectAccuracyThreshold;

        int floorPoints =
            baseScorePerFloor +
            Mathf.RoundToInt(baseScorePerFloor * accuracy);

        int multiplier = 1;

        if (isPerfect)
        {
            comboCount = Mathf.Min(comboCount + 1, maxCombo);
            totalPerfectLandings++;

            floorPoints += perfectLandingBonus;
            multiplier = Mathf.Max(1, comboCount);

            if (perfectStreakTimer != null)
                perfectStreakTimer.StartOrResetTimer();
        }
        else
        {
            ResetPerfectStreak();
        }

        score += floorPoints * multiplier;

        RefreshUI();

        if (CraneController.Instance != null)
            CraneController.Instance.RaiseCraneForNextFloor(floorsBuilt);

        if (floorsBuilt >= floorsToComplete)
            BuildingComplete();
    }

    public void EndPerfectStreak()
    {
        comboCount = 0;
    }

    public void NotifyBlockReleased()
    {
        if (perfectStreakTimer != null)
            perfectStreakTimer.PauseTimer();
    }

    private void ResetPerfectStreak()
    {
        comboCount = 0;

        if (perfectStreakTimer != null)
            perfectStreakTimer.StopTimer();
    }

    public void LoseLife()
    {
        if (isGameOver || isBuildingComplete)
            return;

        lives = Mathf.Max(0, lives - 1);
        ResetPerfectStreak();
        RefreshUI();

        if (lives <= 0)
            GameOver();
    }

    public void BuildingComplete()
    {
        if (isGameOver || isBuildingComplete)
            return;

        isBuildingComplete = true;

        if (perfectStreakTimer != null)
            perfectStreakTimer.StopTimer();

        if (CraneController.Instance != null)
            CraneController.Instance.StopCrane();

        RefreshUI();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowBuildingComplete(score);

        Time.timeScale = 0f;
    }

    public void GameOver()
    {
        if (isGameOver || isBuildingComplete)
            return;

        isGameOver = true;
        ResetPerfectStreak();

        if (CraneController.Instance != null)
            CraneController.Instance.StopCrane();

        RefreshUI();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(score);

        Time.timeScale = 0f;
    }

    public void RefreshUI()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.UpdateScore(score);
        UIManager.Instance.UpdateLives(lives);
        UIManager.Instance.UpdateFloors(floorsBuilt, floorsToComplete);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
