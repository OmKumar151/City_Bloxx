using UnityEngine;

public class CraneController : MonoBehaviour
{
    public static CraneController Instance;

    [Header("References")]
    public Transform hook;
    public Transform rope;
    public GameObject hangingBlockPrefab;

    [Header("Horizontal Movement")]
    public float moveSpeed = 2.5f;

    // How far left/right the hook travels from the center
    public float movementRange = 3.2f;

    // Center of the gameplay area
    public float movementCenterX = 0f;

    [Header("Block Position")]
    public float blockBelowHook = 0.65f;

    private GameObject currentBlock;

    private bool movingRight = true;
    private bool stopped = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (hook == null)
        {
            Debug.LogError("CraneController: Hook is not assigned!");
            return;
        }

        SpawnBlock();
    }

    private void Update()
    {
        if (stopped || hook == null)
            return;

        MoveHook();
        UpdateRope();
    }

    private void MoveHook()
    {
        Vector3 position = hook.position;

        float leftLimit = movementCenterX - movementRange;
        float rightLimit = movementCenterX + movementRange;

        if (movingRight)
        {
            position.x += moveSpeed * Time.deltaTime;

            if (position.x >= rightLimit)
            {
                position.x = rightLimit;
                movingRight = false;
            }
        }
        else
        {
            position.x -= moveSpeed * Time.deltaTime;

            if (position.x <= leftLimit)
            {
                position.x = leftLimit;
                movingRight = true;
            }
        }

        hook.position = position;
    }

    private void UpdateRope()
    {
        if (rope == null)
            return;

        Vector3 ropePosition = rope.position;

        // Rope follows the hook horizontally
        ropePosition.x = hook.position.x;

        rope.position = ropePosition;

        // Keep rope vertical
        rope.rotation = Quaternion.identity;
    }

    public void SpawnBlock()
    {
        if (currentBlock != null)
            return;

        if (hangingBlockPrefab == null)
        {
            Debug.LogError("CraneController: Hanging Block Prefab is not assigned!");
            return;
        }

        if (hook == null)
        {
            Debug.LogError("CraneController: Hook is not assigned!");
            return;
        }

        stopped = false;

        GameObject newBlock = Instantiate(
            hangingBlockPrefab,
            hook.position,
            Quaternion.identity
        );

        currentBlock = newBlock;

        // Make block follow the hook while hanging
        currentBlock.transform.SetParent(hook);

        currentBlock.transform.localPosition =
            new Vector3(0f, -blockBelowHook, 0f);

        currentBlock.transform.localRotation = Quaternion.identity;

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
        }

        UpdateRope();
    }

    public void ReleaseBlock()
    {
        if (currentBlock == null)
            return;

        stopped = true;

        // Remove block from hook
        currentBlock.transform.SetParent(null);

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
        }

        currentBlock = null;
    }

    public void PrepareNextBlock()
    {
        stopped = false;

        Invoke(nameof(SpawnBlock), 0.3f);
    }
}