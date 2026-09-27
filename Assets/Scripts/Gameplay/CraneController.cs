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

    public float movementRange = 3.2f;
    public float movementCenterX = 0f;

    [Header("Block Position")]
    public float blockBelowHook = 0.65f;

    [Header("Camera Follow")]
    public Transform cameraTransform;

    // Distance between crane and camera
    public float cameraOffsetY = 4f;

    // How smoothly crane follows camera
    public float craneFollowSpeed = 5f;

    private GameObject currentBlock;

    private bool movingRight = true;
    private bool stopped = false;

    private float initialCameraY;
    private float initialCraneY;

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

        // Automatically find main camera if not assigned
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Remember starting positions
        if (cameraTransform != null)
        {
            initialCameraY = cameraTransform.position.y;
            initialCraneY = transform.position.y;
        }

        SpawnBlock();
    }

    private void Update()
    {
        if (stopped)
            return;

        MoveHook();
        UpdateRope();
        FollowCamera();
    }

    private void MoveHook()
    {
        if (hook == null)
            return;

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

    private void FollowCamera()
    {
        if (cameraTransform == null)
            return;

        // How much the camera has moved upward
        float cameraMovementY =
            cameraTransform.position.y - initialCameraY;

        // Crane follows the camera upward
        float targetY =
            initialCraneY + cameraMovementY;

        Vector3 cranePosition = transform.position;

        cranePosition.y = Mathf.Lerp(
            cranePosition.y,
            targetY,
            craneFollowSpeed * Time.deltaTime
        );

        transform.position = cranePosition;
    }

    private void UpdateRope()
    {
        if (rope == null || hook == null)
            return;

        Vector3 ropePosition = rope.position;

        // Rope follows hook horizontally
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
            Debug.LogError(
                "CraneController: Hanging Block Prefab is not assigned!"
            );
            return;
        }

        if (hook == null)
        {
            Debug.LogError(
                "CraneController: Hook is not assigned!"
            );
            return;
        }

        stopped = false;

        GameObject newBlock = Instantiate(
            hangingBlockPrefab,
            hook.position,
            Quaternion.identity
        );

        currentBlock = newBlock;

        // Make block follow hook
        currentBlock.transform.SetParent(hook);

        currentBlock.transform.localPosition =
            new Vector3(0f, -blockBelowHook, 0f);

        currentBlock.transform.localRotation =
            Quaternion.identity;

        Rigidbody2D rb =
            currentBlock.GetComponent<Rigidbody2D>();

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

        Rigidbody2D rb =
            currentBlock.GetComponent<Rigidbody2D>();

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
