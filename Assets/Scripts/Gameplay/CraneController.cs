
using System.Collections;
using UnityEngine;

public class CraneController : MonoBehaviour
{
    public static CraneController Instance { get; private set; }

    [Header("Crane References")]
    public Transform craneRoot;
    public Transform swingPivot;
    public Transform hook;
    public LineRenderer rope;
    public GameObject hangingBlockPrefab;

    [Header("Block Settings")]
    [Min(0f)] public float blockBelowHook = 0.65f;

    [Header("Crane Movement")]
    [Min(0f)] public float verticalClearance = 1.5f;
    [Min(0f)] public float craneMoveSpeed = 3f;

    [Header("Block Sequence")]
    [Min(0f)] public float nextBlockDelay = 0.3f;

    [Header("Initial Gameplay Delay")]
    [Min(0f)] public float initialSwingDelay = 3f;

    private GameObject currentBlock;
    private SwingPivotController swingController;
    private RopeController ropeController;
    private Transform ropeTopPoint;
    private BuildingGameplayConfig activeConfiguration;

    private bool stopped;
    private bool movingCraneUp;
    private bool spawnScheduled;
    private bool isFirstBlock = true;

    private float targetCraneY;
    private int lastHeightFloor;

    private Coroutine spawnCoroutine;
    private Coroutine initialSwingCoroutine;

    public GameObject CurrentBlock => currentBlock;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Duplicate CraneController found. Destroying duplicate.",
                this
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        GameManager manager = GameManager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                "CraneController: GameManager.Instance was not found.",
                this
            );
            return;
        }

        activeConfiguration = manager.ActiveConfiguration;

        if (activeConfiguration != null)
        {
            if (activeConfiguration.hangingBlockPrefab == null)
            {
                Debug.LogError(
                    "CraneController: The selected building configuration " +
                    "has no Hanging Block Prefab assigned.",
                    activeConfiguration
                );
                return;
            }

            hangingBlockPrefab = activeConfiguration.hangingBlockPrefab;
        }

        if (craneRoot == null)
        {
            Debug.LogError(
                "CraneController: Crane Root is not assigned.",
                this
            );
            return;
        }

        if (swingPivot == null)
        {
            Debug.LogError(
                "CraneController: Swing Pivot is not assigned.",
                this
            );
            return;
        }

        if (hook == null)
        {
            Debug.LogError(
                "CraneController: Hook is not assigned.",
                this
            );
            return;
        }

        if (hangingBlockPrefab == null)
        {
            Debug.LogError(
                "CraneController: Hanging Block Prefab is not assigned.",
                this
            );
            return;
        }

        if (manager.LastPlacedBlock == null)
        {
            Debug.LogError(
                "CraneController: GameManager has no starting building. " +
                "Assign the Starting Building in GameManager.",
                this
            );
            return;
        }

        swingController = swingPivot.GetComponent<SwingPivotController>();

        if (swingController == null)
        {
            Debug.LogWarning(
                "CraneController: No SwingPivotController was found " +
                "on the Swing Pivot.",
                swingPivot
            );
        }

        ropeController = GetComponent<RopeController>();

        if (ropeController == null)
            ropeController = GetComponentInChildren<RopeController>();

        ropeTopPoint = craneRoot.Find("Pulley");

        if (ropeTopPoint == null)
            ropeTopPoint = craneRoot;

        targetCraneY = craneRoot.position.y;
        lastHeightFloor = manager.floorsBuilt;

        ConfigureRope();

        // Spawn the first block immediately, but delay its swing.
        SpawnBlock();
    }

    private void Update()
    {
        if (stopped)
            return;

        if (movingCraneUp)
            MoveCraneUp();
    }

    public void ReleaseBlock()
    {
        if (stopped || currentBlock == null)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
        {
            return;
        }

        GameObject releasedBlock = currentBlock;

        Rigidbody2D rb = releasedBlock.GetComponent<Rigidbody2D>();
        BlockLanding landing = releasedBlock.GetComponent<BlockLanding>();

        if (rb == null)
        {
            Debug.LogError(
                "CraneController: Released block has no Rigidbody2D.",
                releasedBlock
            );
            return;
        }

        if (landing == null)
        {
            Debug.LogError(
                "CraneController: Released block has no BlockLanding component.",
                releasedBlock
            );
            return;
        }

        // Pause the perfect-streak timer while the block is falling.
        manager.NotifyBlockReleased();

        currentBlock = null;

        releasedBlock.transform.SetParent(null, true);

        if (swingController != null)
            swingController.StopSwing();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.bodyType = RigidbodyType2D.Dynamic;

        landing.MarkAsReleased();

        ConfigureRope();

        Debug.Log(
            $"CraneController: Released '{releasedBlock.name}'. " +
            "BlockLanding.MarkAsReleased() was called.",
            releasedBlock
        );
    }

    public void PrepareNextBlock()
    {
        if (stopped || currentBlock != null || spawnScheduled)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
        {
            return;
        }

        spawnScheduled = true;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnScheduledBlock());
    }

    private IEnumerator SpawnScheduledBlock()
    {
        yield return new WaitForSeconds(nextBlockDelay);

        spawnScheduled = false;
        spawnCoroutine = null;

        if (stopped || currentBlock != null)
            yield break;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
        {
            yield break;
        }

        SpawnBlock();
    }

    private void SpawnBlock()
    {
        if (stopped || currentBlock != null)
            return;

        if (hook == null || hangingBlockPrefab == null)
        {
            Debug.LogError(
                "CraneController: Cannot spawn block. Hook or prefab is missing.",
                this
            );
            return;
        }

        GameObject block = Instantiate(
            hangingBlockPrefab,
            hook.position,
            Quaternion.identity
        );

        Rigidbody2D rb = block.GetComponent<Rigidbody2D>();
        Collider2D blockCollider = block.GetComponent<Collider2D>();
        BlockLanding landing = block.GetComponent<BlockLanding>();

        if (rb == null || blockCollider == null || landing == null)
        {
            Debug.LogError(
                "CraneController: Hanging Block Prefab must have a " +
                "Rigidbody2D, Collider2D, and BlockLanding on its root.",
                block
            );

            Destroy(block);
            return;
        }

        currentBlock = block;

        block.transform.SetParent(hook, true);
        block.transform.localPosition = new Vector3(
            0f,
            -blockBelowHook,
            0f
        );

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        BuildingTier tier = block.GetComponent<BuildingTier>();

        if (tier != null)
        {
            tier.Configure(activeConfiguration);

            GameManager manager = GameManager.Instance;

            if (manager != null)
                tier.ApplyTierForFloor(manager.floorsBuilt + 1);
        }

        ConfigureRope();

        if (isFirstBlock)
        {
            isFirstBlock = false;

            if (initialSwingCoroutine != null)
                StopCoroutine(initialSwingCoroutine);

            initialSwingCoroutine =
                StartCoroutine(StartInitialSwingAfterDelay());
        }
        else
        {
            // Subsequent blocks swing as soon as they spawn.
            StartBlockSwing();
        }

        Debug.Log(
            $"CraneController: Spawned '{block.name}'.",
            block
        );
    }

    private IEnumerator StartInitialSwingAfterDelay()
    {
        float delay = Mathf.Max(0f, initialSwingDelay);

        yield return new WaitForSeconds(delay);

        initialSwingCoroutine = null;

        if (stopped || currentBlock == null)
            yield break;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
        {
            yield break;
        }

        StartBlockSwing();
    }

    private void StartBlockSwing()
    {
        if (stopped || currentBlock == null)
            return;

        if (swingController != null)
            swingController.StartSwing();

        // The streak timer only counts down while a block is swinging.
        GameManager manager = GameManager.Instance;

        if (manager != null && manager.perfectStreakTimer != null)
            manager.perfectStreakTimer.ResumeTimer();
    }

    public void RaiseCraneForNextFloor(int floorsBuilt)
    {
        if (stopped || craneRoot == null)
            return;

        if (floorsBuilt <= lastHeightFloor)
            return;

        lastHeightFloor = floorsBuilt;

        targetCraneY += verticalClearance;
        movingCraneUp = true;
    }

    private void MoveCraneUp()
    {
        if (craneRoot == null)
        {
            movingCraneUp = false;
            return;
        }

        Vector3 position = craneRoot.position;

        position.y = Mathf.MoveTowards(
            position.y,
            targetCraneY,
            craneMoveSpeed * Time.deltaTime
        );

        craneRoot.position = position;

        ConfigureRope();

        if (Mathf.Approximately(position.y, targetCraneY))
            movingCraneUp = false;
    }

    public void StopCrane()
    {
        stopped = true;
        movingCraneUp = false;
        spawnScheduled = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (initialSwingCoroutine != null)
        {
            StopCoroutine(initialSwingCoroutine);
            initialSwingCoroutine = null;
        }

        if (swingController != null)
            swingController.StopSwing();
    }

    private void ConfigureRope()
    {
        if (ropeController != null)
        {
            ropeController.UpdateRope();
            return;
        }

        if (rope == null || hook == null || ropeTopPoint == null)
            return;

        rope.positionCount = 2;
        rope.useWorldSpace = true;
        rope.SetPosition(0, ropeTopPoint.position);
        rope.SetPosition(1, hook.position);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
