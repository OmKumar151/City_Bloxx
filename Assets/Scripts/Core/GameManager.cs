using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public GameObject blockPrefab;
    public GameObject roofPrefab;
    public CameraFollow cameraFollow;

    [Header("Game Settings")]
    public int lives = 3;
    public int score = 0;

    [Header("Building")]
    public int floorsBuilt = 0;
    public int targetFloors = 10;

    [HideInInspector]
    public bool isGameOver = false;

    private GameObject lastPlacedBlock;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Find the foundation.
        lastPlacedBlock = GameObject.Find("Foundation");

        if (lastPlacedBlock == null)
        {
            Debug.LogError("Foundation not found!");
            return;
        }

        // Start camera at the foundation.
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(lastPlacedBlock.transform);
        }

        // Reset game values.
        floorsBuilt = 0;
        score = 0;
        lives = 3;
        isGameOver = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateLives(lives);
            UIManager.Instance.UpdateFloors(floorsBuilt);
        }
    }

    // =========================================================
    // SPAWN NEXT BLOCK
    // =========================================================

    public void SpawnNextBlock()
    {
        if (isGameOver)
            return;

        // STOP spawning after 10 floors.
        if (floorsBuilt >= targetFloors)
        {
            BuildingComplete();
            return;
        }

        if (lastPlacedBlock == null)
        {
            Debug.LogError("Last placed block is missing!");
            return;
        }

        float spawnY = lastPlacedBlock.transform.position.y + 3f;

        GameObject newBlock = Instantiate(
            blockPrefab,
            new Vector3(0f, spawnY, 0f),
            Quaternion.identity
        );

        Rigidbody2D rb = newBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
        }

        // IMPORTANT:
        // The block can move left/right while hanging.
        BlockSwing swing = newBlock.GetComponent<BlockSwing>();

        if (swing != null)
        {
            swing.enabled = true;
        }
    }

    // =========================================================
    // BLOCK SUCCESSFULLY PLACED
    // =========================================================

    public void SetLastPlacedBlock(GameObject block)
    {
        if (block == null)
            return;

        lastPlacedBlock = block;

        // Move camera target to the NEW highest block.
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(block.transform);
        }
    }

    // =========================================================
    // FLOOR COUNT
    // =========================================================

    public void AddFloor()
    {
        if (isGameOver)
            return;

        floorsBuilt++;

        Debug.Log("Floors Built: " + floorsBuilt);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateFloors(floorsBuilt);
        }

        // EXACTLY 10 BLOCKS = COMPLETE
        if (floorsBuilt >= targetFloors)
        {
            BuildingComplete();
        }
    }

    // =========================================================
    // SCORE
    // =========================================================

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

    // =========================================================
    // LOSE LIFE
    // =========================================================

    public void LoseLife()
    {
        if (isGameOver)
            return;

        lives--;

        lives = Mathf.Max(lives, 0);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLives(lives);
        }

        Debug.Log("Lives Remaining: " + lives);

        if (lives <= 0)
        {
            GameOver();
        }
    }

    // =========================================================
    // BUILDING COMPLETE
    // =========================================================

    private void BuildingComplete()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("=================================");
        Debug.Log("BUILDING COMPLETE!");
        Debug.Log("10 FLOORS BUILT!");
        Debug.Log("FINAL SCORE: " + score);
        Debug.Log("=================================");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowBuildingComplete(score);
        }

        Time.timeScale = 0f;
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    private void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("GAME OVER");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        Time.timeScale = 0f;
    }
}