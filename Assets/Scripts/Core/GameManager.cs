using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting State")]
    [Min(1)] public int startingLives = 3;
    [Min(1)] public int floorsToComplete = 30;

    [Header("Scoring")]
    [Min(0)] public int baseScorePerFloor = 100;
    [Min(0)] public int perfectLandingBonus = 100;
    [Range(0f, 1f)] public float perfectAccuracyThreshold = 0.9f;
    [Min(1)] public int maxCombo = 5;

    [Header("Scene References")]
    public GameObject startingBuilding;

    [Header("Runtime State")]
    public int score;
    public int lives;
    public int floorsBuilt;
    public int comboCount;
    public bool isGameOver;
    public bool isBuildingComplete;

    public GameObject LastPlacedBlock { get; private set; }

    public BuildingGameplayConfig ActiveConfiguration { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameManager found.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ActiveConfiguration = GameplaySelection.SelectedBuilding;

        if (ActiveConfiguration != null)
        {
            floorsToComplete = Mathf.Max(
                1,
                ActiveConfiguration.floorsToComplete
            );

            Debug.Log(
                "GameManager: Loaded configuration '" +
                ActiveConfiguration.displayName +
                "' with " + floorsToComplete + " floors.",
                this
            );
        }
        else
        {
            Debug.LogWarning(
                "GameManager: No building configuration was selected. " +
                "Using the Inspector defaults for direct gameplay testing.",
                this
            );
        }

        ResetGameState();

        LastPlacedBlock = FindStartingBuilding();

        if (LastPlacedBlock == null)
        {
            Debug.LogError(
                "Starting building not found. Assign Starting Building " +
                "or ensure a valid object has the Building tag.",
                this
            );
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void ResetGameState()
    {
        score = 0;
        lives = Mathf.Max(1, startingLives);
        floorsBuilt = 0;
        comboCount = 0;
        isGameOver = false;
        isBuildingComplete = false;

        Time.timeScale = 1f;
    }

    private GameObject FindStartingBuilding()
    {
        if (startingBuilding != null)
            return startingBuilding;

        GameObject[] buildings;

        try
        {
            buildings = GameObject.FindGameObjectsWithTag("Building");
        }
        catch (UnityException)
        {
            Debug.LogError(
                "The 'Building' tag does not exist. Create it in Tags and Layers.",
                this
            );

            return null;
        }

        GameObject highest = null;
        float highestY = float.NegativeInfinity;

        foreach (GameObject building in buildings)
        {
            if (building == null)
                continue;

            Collider2D col = building.GetComponent<Collider2D>();

            float y = col != null
                ? col.bounds.max.y
                : building.transform.position.y;

            if (y > highestY)
            {
                highest = building;
                highestY = y;
            }
        }

        return highest;
    }

    public void SetLastPlacedBlock(GameObject block)
    {
        if (block != null)
            LastPlacedBlock = block;
    }

    public void SpawnNextBlock()
    {
        if (isGameOver || isBuildingComplete)
            return;

        if (CraneController.Instance != null)
            CraneController.Instance.PrepareNextBlock();
    }

    public void AddFloor(float accuracy)
    {
        if (isGameOver || isBuildingComplete)
            return;

        floorsBuilt++;
        accuracy = Mathf.Clamp01(accuracy);

        // Update the displayed floor count.
        RefreshFloorUI();

        bool perfect = accuracy >= perfectAccuracyThreshold;

        int floorPoints =
            baseScorePerFloor +
            Mathf.RoundToInt(baseScorePerFloor * accuracy);

        if (perfect)
        {
            comboCount = Mathf.Min(comboCount + 1, maxCombo);
            floorPoints += perfectLandingBonus;
        }
        else
        {
            comboCount = 0;
        }

        int multiplier = 1 +
            (perfect ? Mathf.Min(comboCount - 1, maxCombo - 1) : 0);

        AddScore(floorPoints * multiplier);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCombo(
                comboCount,
                maxCombo,
                perfect
            );
        }

        if (CraneController.Instance != null)
            CraneController.Instance.RaiseCraneForNextFloor(floorsBuilt);

        if (floorsBuilt >= floorsToComplete)
            BuildingComplete();
    }

    public void AddScore(int amount)
    {
        if (isGameOver || isBuildingComplete)
            return;

        score = Mathf.Max(0, score + amount);

        // Update the displayed score.
        RefreshScoreUI();
    }

    public void LoseLife()
    {
        if (isGameOver || isBuildingComplete)
            return;

        lives = Mathf.Max(0, lives - 1);
        comboCount = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLives(lives);
            UIManager.Instance.UpdateCombo(
                comboCount,
                maxCombo,
                false
            );
        }

        if (lives <= 0)
            GameOver();
    }

    public void BuildingComplete()
    {
        if (isGameOver || isBuildingComplete)
            return;

        isBuildingComplete = true;
        StopCrane();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowBuildingComplete(score);
    }

    public void GameOver()
    {
        if (isGameOver || isBuildingComplete)
            return;

        isGameOver = true;
        StopCrane();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver();

        Time.timeScale = 0f;
    }

    private void RefreshUI()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.UpdateScore(score);
        UIManager.Instance.UpdateLives(lives);
        UIManager.Instance.UpdateFloors(floorsBuilt);
        UIManager.Instance.UpdateCombo(comboCount, maxCombo, false);
    }

    private void RefreshScoreUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(score);
    }

    private void RefreshFloorUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateFloors(floorsBuilt);
    }

    private void StopCrane()
    {
        if (CraneController.Instance != null)
            CraneController.Instance.StopCrane();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;
        Time.timeScale = 1f;
    }
}