
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BlockLanding : MonoBehaviour
{
    [Header("Stacking Rules")]
    [Min(0f)] public float minOverlap = 0.3f;
    [Min(0f)] public float landingHeightTolerance = 0.3f;
    [Min(0f)] public float missedBlockLifetime = 0.5f;
    public float missYPosition = -10f;

    [Header("Miss Detection")]
    [Min(0.1f)] public float unresolvedContactTimeout = 0.75f;
    [Min(0.1f)] public float stationaryMissTimeout = 0.5f;
    [Min(0f)] public float stationaryVelocityThreshold = 0.15f;

    [Header("Debugging")]
    public bool enableDebugLogs = true;

    private bool resolved;
    private bool hasBeenReleased;

    private Collider2D blockCollider;
    private Rigidbody2D body;

    private float unresolvedContactTimer;
    private float stationaryTimer;
    private bool hasUnresolvedContact;

    private void Awake()
    {
        blockCollider = GetComponent<Collider2D>();
        body = GetComponent<Rigidbody2D>();
    }

    public void MarkAsReleased()
    {
        if (resolved)
            return;

        hasBeenReleased = true;
        unresolvedContactTimer = 0f;
        stationaryTimer = 0f;
        hasUnresolvedContact = false;

        Log("Block released. Waiting for a valid landing.");
    }

    private void Update()
    {
        if (resolved || !hasBeenReleased || body == null)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
            return;

        if (body.bodyType != RigidbodyType2D.Dynamic)
            return;

        if (transform.position.y < missYPosition)
        {
            ResolveMiss("fell below the play area");
            return;
        }

        // Quick Play towers get very tall: a block that drops past the top
        // of the tower is a miss now, not after a long fall to the ground.
        if (manager.IsEndless &&
            manager.LastPlacedBlock != null &&
            manager.LastPlacedBlock != gameObject &&
            transform.position.y <
                manager.LastPlacedBlock.transform.position.y - manager.endlessMissDepth)
        {
            ResolveMiss("fell past the top of the tower");
            return;
        }

        if (hasUnresolvedContact)
        {
            unresolvedContactTimer += Time.deltaTime;

            if (unresolvedContactTimer >= unresolvedContactTimeout)
            {
                ResolveMiss("contact occurred without a valid landing");
                return;
            }
        }

        if (body.linearVelocity.sqrMagnitude <=
            stationaryVelocityThreshold * stationaryVelocityThreshold)
        {
            stationaryTimer += Time.deltaTime;

            if (stationaryTimer >= stationaryMissTimeout)
                ResolveMiss("stopped moving without a valid landing");
        }
        else
        {
            stationaryTimer = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        LogCollision(collision, "Enter");
        TryResolveLanding(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryResolveLanding(collision);
    }

    private void TryResolveLanding(Collision2D collision)
    {
        if (resolved ||
            !hasBeenReleased ||
            collision == null ||
            blockCollider == null ||
            body == null ||
            body.bodyType != RigidbodyType2D.Dynamic)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
            return;

        GameObject target = manager.LastPlacedBlock;

        if (target == null || target == gameObject)
        {
            MarkUnresolvedContact();
            return;
        }

        Collider2D[] targetColliders =
            target.GetComponentsInChildren<Collider2D>();

        Collider2D matchedTargetCollider = null;

        foreach (Collider2D targetCollider in targetColliders)
        {
            if (targetCollider == null || !targetCollider.enabled)
                continue;

            if (IsCollisionWithTarget(collision, target, targetCollider))
            {
                matchedTargetCollider = targetCollider;
                break;
            }
        }

        if (matchedTargetCollider == null)
        {
            MarkUnresolvedContact();
            return;
        }

        Bounds blockBounds = blockCollider.bounds;
        Bounds targetBounds = matchedTargetCollider.bounds;

        float overlapWidth = CalculateOverlapWidth(
            blockBounds,
            targetBounds
        );

        float verticalGap = blockBounds.min.y - targetBounds.max.y;

        bool correctHeight =
            verticalGap >= -landingHeightTolerance &&
            verticalGap <= landingHeightTolerance;

        bool contactNearTop = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            bool contactOnBlockBottom =
                contact.point.y <= blockBounds.min.y +
                landingHeightTolerance;

            bool contactOnTargetTop =
                contact.point.y >= targetBounds.max.y -
                landingHeightTolerance;

            if (contactOnBlockBottom && contactOnTargetTop)
            {
                contactNearTop = true;
                break;
            }
        }

        Log(
            $"Landing check: ContactNearTop={contactNearTop}, " +
            $"CorrectHeight={correctHeight}, " +
            $"Overlap={overlapWidth:F3}, " +
            $"RequiredOverlap={minOverlap:F3}"
        );

        if (contactNearTop &&
            correctHeight &&
            overlapWidth >= minOverlap)
        {
            ResolveSuccess(manager, matchedTargetCollider);
            return;
        }

        MarkUnresolvedContact();
    }

    private void ResolveSuccess(
        GameManager manager,
        Collider2D matchedTargetCollider)
    {
        if (resolved)
            return;

        resolved = true;
        hasUnresolvedContact = false;

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.freezeRotation = true;

        transform.rotation = Quaternion.identity;

        Bounds targetBounds = matchedTargetCollider.bounds;
        Bounds blockBounds = blockCollider.bounds;

        float horizontalOffset =
            blockBounds.center.x - targetBounds.center.x;

        float allowedCenteringDistance =
            Mathf.Max(0f, manager.perfectLandingDistance);

        bool shouldAutoCenter =
            Mathf.Abs(horizontalOffset) <= allowedCenteringDistance;

        if (shouldAutoCenter)
        {
            Vector3 position = transform.position;

            position.x += targetBounds.center.x - blockBounds.center.x;

            transform.position = position;

            Log(
                $"Auto-centering applied. Offset={horizontalOffset:F3}, " +
                $"Threshold={allowedCenteringDistance:F3}"
            );
        }
        else
        {
            Log(
                $"Auto-centering skipped. Offset={horizontalOffset:F3}, " +
                $"Threshold={allowedCenteringDistance:F3}"
            );
        }

        // Physics bounds don't update when the transform is moved in code,
        // so push the auto-centre move to the physics engine first.
        Physics2D.SyncTransforms();

        // Stack on what is DRAWN: put this block's visible bottom exactly
        // on the target's visible top. Renderer bounds are always current
        // and every building sprite has a different height, so this keeps
        // floors flush with no gap or overlap.
        float correctionY =
            GetVisualTop(matchedTargetCollider) - GetVisualBottom();

        transform.position += Vector3.up * correctionY;

        Physics2D.SyncTransforms();

        blockBounds = blockCollider.bounds;
        targetBounds = matchedTargetCollider.bounds;

        float finalOverlapWidth = CalculateOverlapWidth(
            blockBounds,
            targetBounds
        );

        float accuracy = blockBounds.size.x > 0f
            ? Mathf.Clamp01(finalOverlapWidth / blockBounds.size.x)
            : 0f;

        if (shouldAutoCenter)
        {
            accuracy = Mathf.Max(
                accuracy,
                manager.perfectAccuracyThreshold
            );
        }

        gameObject.tag = "Building";

        manager.SetLastPlacedBlock(gameObject);

        BuildingTier tier = GetComponent<BuildingTier>();

        if (tier != null)
            tier.ApplyTierForFloor(manager.floorsBuilt + 1);

        Log(
            $"LANDING SUCCESS. Accuracy={accuracy:F2}, " +
            $"HorizontalOffset={horizontalOffset:F3}, " +
            $"Centered={shouldAutoCenter}, " +
            $"PerfectThreshold={manager.perfectAccuracyThreshold:F2}"
        );

        manager.AddFloor(accuracy);

        ContinueCraneIfPlaying();
    }

    // Solid bottom of this block's wall (sprite minus its soft bottom
    // fringe), falling back to the sprite, then the collider.
    private float GetVisualBottom()
    {
        BuildingTier tier = GetComponent<BuildingTier>();

        if (tier != null && tier.HasSprite)
            return tier.SolidBottom;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        return spriteRenderer != null && spriteRenderer.sprite != null
            ? spriteRenderer.bounds.min.y
            : blockCollider.bounds.min.y;
    }

    // Solid top of the block below's wall (sprite minus its corner caps and
    // soft top fringe), falling back to the sprite, then its collider.
    private static float GetVisualTop(Collider2D targetCollider)
    {
        BuildingTier tier = targetCollider.GetComponent<BuildingTier>();

        if (tier != null && tier.HasSprite)
            return tier.SolidTop;

        SpriteRenderer spriteRenderer =
            targetCollider.GetComponent<SpriteRenderer>();

        return spriteRenderer != null && spriteRenderer.sprite != null
            ? spriteRenderer.bounds.max.y
            : targetCollider.bounds.max.y;
    }

    private float CalculateOverlapWidth(
        Bounds blockBounds,
        Bounds targetBounds)
    {
        return Mathf.Max(
            0f,
            Mathf.Min(blockBounds.max.x, targetBounds.max.x) -
            Mathf.Max(blockBounds.min.x, targetBounds.min.x)
        );
    }

    private void MarkUnresolvedContact()
    {
        if (hasUnresolvedContact)
            return;

        hasUnresolvedContact = true;
        unresolvedContactTimer = 0f;

        Log("Contact detected, but it has not qualified as a valid landing.");
    }

    private bool IsCollisionWithTarget(
        Collision2D collision,
        GameObject target,
        Collider2D targetCollider)
    {
        return collision.collider == targetCollider ||
               collision.otherCollider == targetCollider ||
               IsPartOfTarget(collision.collider, target.transform) ||
               IsPartOfTarget(collision.otherCollider, target.transform);
    }

    private bool IsPartOfTarget(
        Collider2D candidate,
        Transform targetRoot)
    {
        return candidate != null &&
               targetRoot != null &&
               (candidate.transform == targetRoot ||
                candidate.transform.IsChildOf(targetRoot));
    }

    private void ResolveMiss(string reason)
    {
        if (resolved || !hasBeenReleased)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
            return;

        resolved = true;

        Log($"BLOCK MISSED: {reason}");

        gameObject.tag = "Untagged";

        if (blockCollider != null)
            blockCollider.enabled = false;

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        // Reset the missed block to an upright rotation.
        transform.rotation = Quaternion.identity;

        body.freezeRotation = true;
        body.bodyType = RigidbodyType2D.Kinematic;

        manager.LoseLife();

        ContinueCraneIfPlaying();

        Destroy(gameObject, missedBlockLifetime);
    }

    private void ContinueCraneIfPlaying()
    {
        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
            return;

        if (CraneController.Instance != null)
            CraneController.Instance.PrepareNextBlock();
    }

    private void LogCollision(Collision2D collision, string phase)
    {
        if (!enableDebugLogs || collision == null)
            return;

        Log(
            $"Collision {phase} with '{collision.gameObject.name}'. " +
            $"Released={hasBeenReleased}, " +
            $"Target={GameManager.Instance?.LastPlacedBlock?.name ?? "NULL"}"
        );
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[BlockLanding: {gameObject.name}] {message}", this);
    }
}
