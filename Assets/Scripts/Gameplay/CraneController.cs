
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
    [Tooltip("Used only when Hang Block From Sprite Top is off: gap " +
             "between the hook's tip and the block's origin.")]
    [Min(0f)] public float blockBelowHook = 0.65f;

    [Tooltip("Hang the block so its sprite's top edge sits at the hook's " +
             "tip. When off, Block Below Hook is used.")]
    public bool hangBlockFromSpriteTop = true;

    [Header("Crane Movement")]
    [Tooltip("Landings before the crane (and the camera following it) " +
             "starts rising. The first rise happens on the next landing.")]
    [Min(0)] public int floorsBeforeCraneRises = 2;

    [Min(0f)] public float verticalClearance = 1.5f;
    [Min(0f)] public float craneMoveSpeed = 3f;

    [Header("Block Sequence")]
    [Min(0f)] public float nextBlockDelay = 0.3f;

    [Header("Initial Gameplay Delay")]
    [Min(0f)] public float initialSwingDelay = 3f;

    [Header("Anti-Spam")]
    [Tooltip("How long a block must swing before it can be dropped. Stops " +
             "dropping a block the instant it appears, from the same spot " +
             "as the last one (which stacked perfectly every time).")]
    [Min(0f)] public float minSwingTimeBeforeRelease = 0.35f;

    [Tooltip("A tap while dropping is locked restarts this cooldown, so " +
             "spamming never gets a block through. The player has to stop " +
             "tapping briefly and time a real drop.")]
    [Min(0f)] public float spamCooldown = 0.2f;

    // Time.time the current block started swinging (-1 = not yet).
    private float swingStartTime = -1f;
    private float lastRejectedPressTime = float.NegativeInfinity;

    private GameObject currentBlock;
    private SwingPivotController swingController;
    private RopeController ropeController;
    private HookController hookController;
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

        // The first block appears only once the camera's opening pan has
        // arrived at the crane.
        spawnCoroutine = StartCoroutine(SpawnFirstBlockAfterOpeningPan());
    }

    private IEnumerator SpawnFirstBlockAfterOpeningPan()
    {
        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();

        if (cameraFollow != null)
        {
            yield return new WaitUntil(
                () => cameraFollow == null || cameraFollow.IsOpeningPanFinished);
        }

        spawnCoroutine = null;

        if (stopped)
            yield break;

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

        if (!CanReleaseNow())
        {
            // Every early tap pushes the next allowed drop back.
            lastRejectedPressTime = Time.time;
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

        manager.NotifyBlockReleased();

        currentBlock = null;
        swingStartTime = -1f;

        // Detach while preserving the current world position.
        releasedBlock.transform.SetParent(null, true);

        // Stop the rope swinging.
        if (swingController != null)
            swingController.StopSwing();

        // Reset the released block's rotation.
        releasedBlock.transform.rotation = Quaternion.identity;
        rb.rotation = 0f;

        // Reset physics before enabling gravity.
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.freezeRotation = true;
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.bodyType = RigidbodyType2D.Dynamic;

        landing.MarkAsReleased();

        ConfigureRope();

        Debug.Log(
            $"CraneController: Released '{releasedBlock.name}'. " +
            "Block rotation reset to zero.",
            releasedBlock
        );
    }

    // A block can drop only once it has been swinging for a moment and
    // the player hasn't tapped too early in the last Spam Cooldown.
    private bool CanReleaseNow()
    {
        if (swingStartTime < 0f)
            return false;

        if (swingController != null && !swingController.IsSwinging)
            return false;

        if (Time.time - swingStartTime < minSwingTimeBeforeRelease)
            return false;

        return Time.time - lastRejectedPressTime >= spamCooldown;
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

        // The swing is NOT reset: the new block hangs at the angle the rope
        // stopped at and the swing carries on from there in the same
        // direction (StartSwing picks up the paused phase).

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
        swingStartTime = -1f;

        // Attach to the swing pivot, not the hook: the hook is scaled to
        // size its sprite, and a child would inherit that scale. The
        // block still swings with the hook because both share the pivot.
        block.transform.SetParent(GetBlockParent(), true);

        // Line the block up with the rope, which may be part-way through a
        // swing: upright relative to the pivot, so it tilts with the hook
        // exactly as it would have if it had hung there from the start.
        block.transform.localRotation = Quaternion.identity;
        rb.rotation = block.transform.eulerAngles.z;

        block.transform.position =
            GetHookBlockPoint() - block.transform.up * blockBelowHook;

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

        // Now the final sprite is known, hang it from its top edge.
        HangBlockFromSpriteTop(block);

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
            StartBlockSwing();
        }

        Debug.Log(
            $"CraneController: Spawned '{block.name}'. " +
            $"Transform Z: {block.transform.eulerAngles.z}, " +
            $"Rigidbody2D rotation: {rb.rotation}.",
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

        swingStartTime = Time.time;

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

        // The first floors fill the gap below the hook; the crane and
        // camera hold still until they are placed.
        if (floorsBuilt <= floorsBeforeCraneRises)
            return;

        targetCraneY += GetLandedBlockHeight();
        movingCraneUp = true;
    }

    // Building sprites differ in height, so rise by the block that just
    // landed. This keeps the same gap above the tower on every floor.
    // Falls back to Vertical Clearance if the height can't be read.
    private float GetLandedBlockHeight()
    {
        GameManager manager = GameManager.Instance;
        GameObject landedBlock = manager != null ? manager.LastPlacedBlock : null;

        if (landedBlock != null)
        {
            // Floors overlap by their soft edges, so use the stacked
            // (solid wall) height, not the full sprite height.
            BuildingTier tier = landedBlock.GetComponent<BuildingTier>();

            if (tier != null && tier.HasSprite)
                return tier.StackedHeight;

            SpriteRenderer spriteRenderer =
                landedBlock.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null)
                return spriteRenderer.bounds.size.y;
        }

        return verticalClearance;
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

    // Places the block directly under the hook so the rope ends at the
    // top of the visible sprite. Works for any sprite height (floors,
    // taller roofs) and any pivot, because it uses the rendered bounds.
    private void HangBlockFromSpriteTop(GameObject block)
    {
        if (!hangBlockFromSpriteTop || block == null || hook == null)
            return;

        SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        // Distance from the block's origin up to the sprite's top edge.
        // Worked out through the transform, not spriteRenderer.bounds:
        // the block was just scaled (floor stretch) and the renderer's
        // bounds can still hold the old size this frame, which left a gap
        // between the hook and the block.
        Vector3 spriteTop = block.transform.TransformPoint(
            new Vector3(0f, spriteRenderer.sprite.bounds.max.y, 0f)
        );

        // Full vector, not just height: the block may be tilted with the
        // rope, so its top-centre is not straight above its origin.
        Vector3 originToTop = spriteTop - block.transform.position;

        Vector3 hangPoint = GetHookBlockPoint();
        Vector3 position = hangPoint - originToTop;
        position.z = block.transform.position.z;

        block.transform.position = position;
    }

    private Transform GetBlockParent()
    {
        return swingPivot != null ? swingPivot : hook;
    }

    // The hook's tip when it has a HookController, else the hook origin.
    private Vector3 GetHookBlockPoint()
    {
        if (hookController == null && hook != null)
            hookController = hook.GetComponent<HookController>();

        return hookController != null
            ? hookController.BlockAttachPoint
            : hook.position;
    }

    private Vector3 GetHookRopePoint()
    {
        if (hookController == null && hook != null)
            hookController = hook.GetComponent<HookController>();

        return hookController != null
            ? hookController.RopeAttachPoint
            : hook.position;
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
        rope.SetPosition(1, GetHookRopePoint());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
