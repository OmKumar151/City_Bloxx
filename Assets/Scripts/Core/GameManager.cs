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
    public int floorsBuilt = 0;
    public int targetFloors = 10;

    [HideInInspector]
    public bool isGameOver = false;

    private GameObject lastPlacedBlock;
    private bool roofSpawned = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        lastPlacedBlock = GameObject.Find("Foundation");
    }

    public void SpawnNextBlock()
    {
        if (isGameOver)
            return;

        float spawnY = lastPlacedBlock.transform.position.y + 3f;

        GameObject prefabToSpawn = blockPrefab;

        if (floorsBuilt >= targetFloors && !roofSpawned)
        {
            if (roofPrefab != null)
            {
                prefabToSpawn = roofPrefab;
                roofSpawned = true;
            }
        }

        GameObject newBlock = Instantiate(
            prefabToSpawn,
            new Vector3(0, spawnY, 0),
            Quaternion.identity
        );

        Rigidbody2D rb = newBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
        }

        BlockSwing swing = newBlock.GetComponent<BlockSwing>();

        if (swing != null)
        {
            swing.enabled = true;
        }
    }

    public void SetLastPlacedBlock(GameObject block)
    {
        lastPlacedBlock = block;

        if (cameraFollow != null)
        {
            cameraFollow.target = block.transform;
        }
    }

    public void AddScore(int points)
    {
        score += points;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(score);

        Debug.Log("Score: " + score);
    }

    public void AddFloor()
    {
        floorsBuilt++;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateFloors(floorsBuilt);

        Debug.Log("Floors Built: " + floorsBuilt);
    }

    public void LoseLife()
    {
        if (isGameOver)
            return;

        lives--;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateLives(lives);

        Debug.Log("Lives Remaining: " + lives);

        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void BuildingComplete()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("BUILDING COMPLETE!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowBuildingComplete(score);
        }

        Time.timeScale = 0f;
    }

    private void GameOver()
    {
        isGameOver = true;

        Debug.Log("GAME OVER");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        Time.timeScale = 0f;
    }
}