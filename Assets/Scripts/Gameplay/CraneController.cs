
using UnityEngine;

public class CraneController : MonoBehaviour
{
    public static CraneController Instance;

    [Header("Crane References")]
    public Transform craneRoot;
    public Transform swingPivot;
    public Transform hook;
    public Transform rope;
    public GameObject hangingBlockPrefab;

    [Header("Pendulum Settings")]
    public float blockBelowHook = 0.65f;

    [Header("Crane Height")]
    public float verticalClearance = 1.5f;
    public float craneMoveSpeed = 3f;

    private GameObject currentBlock;
    private bool stopped = false;
    private bool movingCraneUp = false;

    private float targetCraneY;
    private int lastHeightFloor = 0;

    private SwingPivotController swingController;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (craneRoot == null ||
            swingPivot == null ||
            hook == null)
        {
            Debug.LogError(
                "CraneController: Assign Crane Root, " +
                "Swing Pivot, and Hook in the Inspector."
            );

            enabled = false;
            return;
        }

        if (!swingPivot.IsChildOf(craneRoot))
        {
            Debug.LogError(
                "Swing Pivot must be a child of Crane Root."
            );

            enabled = false;
            return;
        }

        swingController =
            swingPivot.GetComponent<SwingPivotController>();

        if (swingController == null)
        {
            Debug.LogError(
                "SwingPivotController is missing from Swing Pivot!"
            );

            enabled = false;
            return;
        }

        targetCraneY = craneRoot.position.y;

        SpawnBlock();
    }

    private void Update()
    {
        if (!stopped)
        {
            KeepHangingBlockUpright();
            UpdateRope();
        }

        MoveCraneUp();
    }

    private void KeepHangingBlockUpright()
    {
        if (currentBlock == null)
            return;

        currentBlock.transform.rotation =
            Quaternion.identity;
    }

    private void UpdateRope()
    {
        if (rope == null || hook == null)
            return;

        RopeController ropeController =
            rope.GetComponent<RopeController>();

        if (ropeController != null)
        {
            // Use the actual pulley as the rope's top endpoint.
            Transform pulley = craneRoot.Find("Pulley");

            if (pulley != null)
                ropeController.topPoint = pulley;

            ropeController.hook = hook;
        }
    }

    public void RaiseCraneForNextFloor(int floorsBuilt)
    {
        if (craneRoot == null ||
            floorsBuilt <= lastHeightFloor)
            return;

        lastHeightFloor = floorsBuilt;

        targetCraneY += verticalClearance;
        movingCraneUp = true;
    }

    private void MoveCraneUp()
    {
        if (!movingCraneUp || craneRoot == null)
            return;

        Vector3 position = craneRoot.position;

        position.y = Mathf.MoveTowards(
            position.y,
            targetCraneY,
            craneMoveSpeed * Time.deltaTime
        );

        craneRoot.position = position;

        if (Mathf.Approximately(position.y, targetCraneY))
            movingCraneUp = false;
    }

    public void SpawnBlock()
    {
        if (currentBlock != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.isGameOver)
            return;

        if (hangingBlockPrefab == null)
        {
            Debug.LogError(
                "Hanging Block Prefab is not assigned!"
            );

            return;
        }

        stopped = false;

        currentBlock = Instantiate(
            hangingBlockPrefab,
            hook.position,
            Quaternion.identity
        );

        currentBlock.transform.SetParent(hook);

        currentBlock.transform.localPosition =
            new Vector3(0f, -blockBelowHook, 0f);

        currentBlock.transform.rotation =
            Quaternion.identity;

        Rigidbody2D rb =
            currentBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
        }

        swingController.StartSwing();
        UpdateRope();
    }

    public void ReleaseBlock()
    {
        if (currentBlock == null)
            return;

        stopped = true;
        swingController.StopSwing();

        currentBlock.transform.rotation =
            Quaternion.identity;

        currentBlock.transform.SetParent(null);

        Rigidbody2D rb =
            currentBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        currentBlock = null;
    }

    public void PrepareNextBlock()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.isGameOver)
            return;

        CancelInvoke(nameof(SpawnBlock));
        Invoke(nameof(SpawnBlock), 0.3f);
    }
}
