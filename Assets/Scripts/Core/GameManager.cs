
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [Min(1)] public int startingLives = 3;
    [Tooltip("Only used when Gameplay1 is opened directly. Normally the " +
             "selected building's config sets this.")]
    [Min(1)] public int floorsToComplete = 30;

    // Floor points = base + accuracy bonus x accuracy (+ perfect bonus if
    // perfect), then x the combo multiplier while a perfect streak is running.
    // Defaults give 20-30 points a floor: the residents one floor houses. before the multiplier.
    [Header("Scoring")]
    [Tooltip("Points for every landed floor.")]
    [Min(0)] public int baseScorePerFloor = 20;
    [Tooltip("Extra points for accuracy: the full amount for a dead-centre " +
             "landing, less the further off-centre it lands.")]
    [Min(0)] public int accuracyBonusPerFloor = 10;
    [Tooltip("Extra points for a perfect landing (before the combo multiplier).")]
    [Min(0)] public int perfectLandingBonus = 0;
    [Tooltip("Accuracy (overlap / block width) needed for a perfect. " +
             "Auto-centred landings always count as 1.")]
    [Range(0f, 1f)] public float perfectAccuracyThreshold = 0.9f;
    [Tooltip("Longest perfect streak that keeps raising the multiplier.")]
    [Min(1)] public int maxCombo = 5;
    [Tooltip("Each perfect in a row adds this much to the multiplier: " +
             "x1, x1.25, x1.5 ... capped at Max Combo (x2 at 5).")]
    [Min(0f)] public float comboMultiplierStep = 0.25f;
    [Tooltip("Horizontal offset (world units, blocks are 1 wide) that " +
             "still counts as a perfect, auto-centred landing.")]
    [Min(0f)] public float perfectLandingDistance = 0.2f;

    [Header("Building")]
    public GameObject startingBuilding;

    [Header("Perfect Streak")]
    public PerfectStreakTimer perfectStreakTimer;

    [Header("Tower Sway")]
    [Tooltip("Sways the tower after off-centre landings. Found or added " +
             "on this object automatically if left empty.")]
    public TowerSway towerSway;

    [Header("Scenes")]
    public string levelSelectScene = "LevelSelect";

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

    /// <summary>Flag sent from LevelSelect (colour's Star unlocked).</summary>
    public bool SpecialRoofUnlocked { get; private set; }

    /// <summary>
    /// True when the special roof is unlocked for this building AND the
    /// current score has reached the building's threshold.
    /// </summary>
    public bool IsSpecialRoofEarned =>
        ActiveConfiguration != null &&
        SpecialRoofUnlocked &&
        ActiveConfiguration.specialRoofSprite != null &&
        score >= ActiveConfiguration.specialRoofScoreThreshold;

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

        if (towerSway == null)
            towerSway = FindObjectOfType<TowerSway>();

        if (towerSway == null)
            towerSway = gameObject.AddComponent<TowerSway>();

        ApplySelectedBuilding();

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

    // Reads the building chosen in LevelSelect. Without a selection
    // (e.g. Gameplay1 opened directly in the editor) the Inspector
    // values and the prefab's fallback sprites are used.
    private void ApplySelectedBuilding()
    {
        ActiveConfiguration = GameplaySelection.SelectedBuilding;

        if (ActiveConfiguration == null)
        {
            SpecialRoofUnlocked = false;

            Debug.Log(
                "GameManager: No building selected. Using Inspector " +
                "floors (" + floorsToComplete + ") and fallback sprites.",
                this
            );

            return;
        }

        floorsToComplete = Mathf.Max(1, GameplaySelection.FloorsToComplete);
        SpecialRoofUnlocked = GameplaySelection.SpecialRoofUnlocked;

        Debug.Log(
            "GameManager: Building '" + GameplaySelection.BuildingId +
            "' | Floors: " + floorsToComplete +
            " | Special roof unlocked: " + SpecialRoofUnlocked +
            " (needs " + ActiveConfiguration.specialRoofScoreThreshold +
            " points)",
            this
        );
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

        // Capture whether this is the foundation landing
        // before incrementing the floor count.
        bool isFoundationLanding = floorsBuilt == 0;

        floorsBuilt++;

        accuracy = Mathf.Clamp01(accuracy);

        // Foundation landing must never count as perfect.
        // Every subsequent floor uses the normal perfect threshold.
        bool isPerfect =
            !isFoundationLanding &&
            accuracy >= perfectAccuracyThreshold;

        int floorPoints =
            baseScorePerFloor +
            Mathf.RoundToInt(accuracyBonusPerFloor * accuracy);

        float multiplier = 1f;

        if (isPerfect)
        {
            comboCount = Mathf.Min(comboCount + 1, maxCombo);
            totalPerfectLandings++;

            floorPoints += perfectLandingBonus;

            // Additive streak bonus. A straight xCombo multiplier let one
            // streak outweigh the rest of the run (and every checkpoint).
            multiplier = 1f + comboMultiplierStep * Mathf.Max(0, comboCount - 1);

            if (perfectStreakTimer != null)
                perfectStreakTimer.StartOrResetTimer();
        }
        else
        {
            ResetPerfectStreak();
        }

        // Off-centre landings make the tower sway more, central ones calm it.
        if (towerSway != null)
            towerSway.RegisterLanding(LastPlacedBlock, floorsBuilt, accuracy, isPerfect);

        score += Mathf.RoundToInt(floorPoints * multiplier);

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

        RefreshRoofSprite();

        if (CraneController.Instance != null)
            CraneController.Instance.StopCrane();

        RefreshUI();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowBuildingComplete(score);

        Time.timeScale = 0f;
    }

    // The roof block lands before its floor points are added, so the
    // roof is re-evaluated here against the final score. If the special
    // roof is unlocked and the threshold was reached, the roof sprite
    // is swapped for the special roof sprite.
    private void RefreshRoofSprite()
    {
        if (LastPlacedBlock == null)
            return;

        BuildingTier roofTier = LastPlacedBlock.GetComponent<BuildingTier>();

        if (roofTier == null)
            return;

        roofTier.ApplyTierForFloor(floorsToComplete);

        if (IsSpecialRoofEarned)
        {
            Debug.Log(
                "GameManager: Special roof earned with score " + score + ".",
                this
            );
        }
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

    // =========================================================
    // END PANEL ACTIONS
    // =========================================================

    private bool isLeavingScene;

    /// <summary>
    /// Replays the same building. GameplaySelection is static, so the
    /// reloaded scene uses the same sprites, floors and roof flag.
    /// </summary>
    public void RestartGame()
    {
        if (isLeavingScene)
            return;

        isLeavingScene = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Win panel "Next": the completed building goes to LevelSelect to be
    /// placed, carrying its final score. The score becomes that building's
    /// population when it is placed (nothing is added before then).
    /// Only works after a completed building.
    /// </summary>
    public void BankScoreAndReturnToLevelSelect()
    {
        if (isLeavingScene || !isBuildingComplete)
            return;

        string cityId = GameplaySelection.CityId;

        if (string.IsNullOrWhiteSpace(cityId))
            cityId = PopulationBonusStore.ActiveCityId;

        // LevelSelect opens board placement for it on load.
        if (ActiveConfiguration != null)
            PendingPlacementStore.Set(ActiveConfiguration.buildingId, cityId, score);

        LoadLevelSelect();
    }

    /// <summary>Fail panel: back to LevelSelect without adding score.</summary>
    public void ReturnToLevelSelect()
    {
        if (isLeavingScene)
            return;

        LoadLevelSelect();
    }

    private void LoadLevelSelect()
    {
        if (string.IsNullOrWhiteSpace(levelSelectScene) ||
            !Application.CanStreamedLevelBeLoaded(levelSelectScene))
        {
            Debug.LogError(
                "GameManager: Scene '" + levelSelectScene +
                "' cannot be loaded. Check its name and Build Settings.",
                this
            );
            return;
        }

        isLeavingScene = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(levelSelectScene);
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
