using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public GameObject blockPrefab;
    public GameObject roofPrefab;
    public CameraFollow cameraFollow;
    public CraneController craneController;

    [Header("Game Settings")]
    public int lives = 3;
    public int score = 0;

    [Header("Building")]
    public int floorsBuilt = 0;
    public int targetFloors = 10;

    [Header("Combo / Accuracy")]
    [Range(0f, 1f)]
    public float perfectAccuracyThreshold = 0.85f;

    public int maxCombo = 5;
    public int baseFloorScore = 50;

    [HideInInspector]
    public int comboCount = 0;

    [HideInInspector]
    public bool isGameOver = false;

    private GameObject lastPlacedBlock;

    public GameObject LastPlacedBlock => lastPlacedBlock;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Reset time in case the previous game ended.
        Time.timeScale = 1f;

        // Find the foundation.
        lastPlacedBlock = GameObject.Find("Foundation");

        if (lastPlacedBlock == null)
        {
            Debug.LogError(
                "GameManager: Foundation was not found!"
            );
            enabled = false;
            return;
        }

        // Find the camera-follow component if not assigned.
        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow =
                Camera.main.GetComponent<CameraFollow>();
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(lastPlacedBlock.transform);
        }

        // Find the crane if not assigned.
        if (craneController == null)
        {
            craneController =
                FindFirstObjectByType<CraneController>();
        }

        // Reset game values.
        floorsBuilt = 0;
        score = 0;
        lives = 3;
        comboCount = 0;
        isGameOver = false;

        // Update the UI.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateLives(lives);
            UIManager.Instance.UpdateFloors(floorsBuilt);
            UIManager.Instance.UpdateCombo(
                comboCount,
                maxCombo,
                false
            );
        }

        if (craneController == null)
        {
            Debug.LogError(
                "GameManager: No CraneController found!"
            );
        }

        // IMPORTANT:
        // CraneController is responsible for spawning
        // and moving the first hanging block.
    }

    // =====================================================
    // SPAWN NEXT BLOCK
    // =====================================================

    public void SpawnNextBlock()
    {
        if (isGameOver)
            return;

        if (floorsBuilt >= targetFloors)
        {
            BuildingComplete();
            return;
        }

        if (craneController == null)
        {
            Debug.LogError(
                "GameManager: Cannot spawn block because " +
                "CraneController is missing."
            );
            return;
        }

        // The crane owns the hanging block lifecycle.
        // Do not Instantiate another block here.
        craneController.PrepareNextBlock();
    }

    // =====================================================
    // UPDATE TOP BLOCK AND CAMERA TARGET
    // =====================================================

    public void SetLastPlacedBlock(GameObject block)
    {
        if (block == null)
            return;

        lastPlacedBlock = block;

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(block.transform);
        }
    }

    // =====================================================
    // FLOOR COUNT, COMBO AND SCORING
    // =====================================================

    public void AddFloor(float accuracy)
    {
        if (isGameOver)
            return;

        // Prevent counting more than the target number.
        if (floorsBuilt >= targetFloors)
            return;

        floorsBuilt++;
        if (craneController != null)
{
    craneController.RaiseCraneForNextFloor(floorsBuilt);
}

        bool wasPerfect =
            accuracy >= perfectAccuracyThreshold;

        if (wasPerfect)
        {
            comboCount = Mathf.Min(
                comboCount + 1,
                maxCombo
            );
        }
        else
        {
            comboCount = 0;
        }

        int multiplier = comboCount + 1;
        int floorScore = baseFloorScore * multiplier;

        AddScore(floorScore);

        Debug.Log(
            "Floors Built: " + floorsBuilt +
            " | Accuracy: " + accuracy.ToString("P0") +
            " | Combo: x" + multiplier
        );

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateFloors(floorsBuilt);

            UIManager.Instance.UpdateCombo(
                comboCount,
                maxCombo,
                wasPerfect
            );
        }

        // Finish after the tenth successfully placed block.
        if (floorsBuilt >= targetFloors)
        {
            BuildingComplete();
        }
    }

    // =====================================================
    // SCORE
    // =====================================================

    public void AddScore(int points)
    {
        if (isGameOver)
            return;

        score += points;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }

        Debug.Log("Score: " + score);
    }

    // =====================================================
    // LOSE LIFE
    // =====================================================

    public void LoseLife()
    {
        if (isGameOver)
            return;

        lives = Mathf.Max(0, lives - 1);

        // A missed block breaks the combo.
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

        Debug.Log("Lives Remaining: " + lives);

        if (lives <= 0)
        {
            GameOver();
        }
    }

    // =====================================================
    // BUILDING COMPLETE
    // =====================================================

    private void BuildingComplete()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("BUILDING COMPLETE!");
        Debug.Log("Floors Built: " + floorsBuilt);
        Debug.Log("Final Score: " + score);

        // Stop the crane from spawning more blocks.
        if (craneController != null)
        {
            craneController.CancelInvoke();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowBuildingComplete(score);
        }

        Time.timeScale = 0f;
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    private void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("GAME OVER");

        // Stop pending block spawns.
        if (craneController != null)
        {
            craneController.CancelInvoke();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}