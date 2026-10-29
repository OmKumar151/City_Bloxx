
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

        if (blockCollider == null)
            Debug.LogError("BlockLanding: No Collider2D found.", this);

        if (body == null)
            Debug.LogError("BlockLanding: No Rigidbody2D found.", this);
    }

    // Called by CraneController when the player releases this block.
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
        // Do not resolve a miss while the block is still attached to the crane.
        if (resolved || !hasBeenReleased || body == null)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null)
        {
            Log("Cannot process block: GameManager.Instance is null.");
            return;
        }

        if (manager.isGameOver || manager.isBuildingComplete)
            return;

        if (body.bodyType != RigidbodyType2D.Dynamic)
            return;

        if (transform.position.y < missYPosition)
        {
            ResolveMiss("fell below the play area");
            return;
        }

        if (hasUnresolvedContact)
        {
            unresolvedContactTimer += Time.deltaTime;

            if (unresolvedContactTimer >= unresolvedContactTimeout)
            {
                ResolveMiss(
                    "contact occurred without a valid landing"
                );

                return;
            }
        }

        if (body.linearVelocity.sqrMagnitude <=
            stationaryVelocityThreshold * stationaryVelocityThreshold)
        {
            stationaryTimer += Time.deltaTime;

            if (stationaryTimer >= stationaryMissTimeout)
            {
                ResolveMiss(
                    "stopped moving without a valid landing"
                );
            }
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
        {
            return;
        }

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
        {
            Log(
                "Landing check stopped: game manager is unavailable " +
                "or the game has ended."
            );

            return;
        }

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

            if (IsCollisionWithTarget(
                collision,
                target,
                targetCollider))
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

        float overlapWidth = Mathf.Max(
            0f,
            Mathf.Min(blockBounds.max.x, targetBounds.max.x) -
            Mathf.Max(blockBounds.min.x, targetBounds.min.x)
        );

        float verticalGap =
            blockBounds.min.y - targetBounds.max.y;

        bool correctHeight =
            verticalGap >= -landingHeightTolerance &&
            verticalGap <= landingHeightTolerance;

        // Verify that the contact is near the falling block's
        // bottom and the target's top surface.
        bool contactNearTop = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            bool contactIsOnBlockBottom =
                contact.point.y <= blockBounds.min.y +
                landingHeightTolerance;

            bool contactIsOnTargetTop =
                contact.point.y >= targetBounds.max.y -
                landingHeightTolerance;

            if (contactIsOnBlockBottom && contactIsOnTargetTop)
            {
                contactNearTop = true;
                break;
            }
        }

        Log(
            $"[Landing Check] Target={target.name}, " +
            $"ContactNearTop={contactNearTop}, " +
            $"CorrectHeight={correctHeight}, " +
            $"VerticalGap={verticalGap:F3}, " +
            $"Overlap={overlapWidth:F3}, " +
            $"RequiredOverlap={minOverlap:F3}"
        );

        if (contactNearTop &&
            correctHeight &&
            overlapWidth >= minOverlap)
        {
            float accuracy = blockBounds.size.x > 0f
                ? Mathf.Clamp01(overlapWidth / blockBounds.size.x)
                : 0f;

            // The first placed floor lands on the starting foundation.
            // Prevent that landing from triggering a perfect streak,
            // even when the overlap accuracy would otherwise be 1.0.
            if (manager.floorsBuilt == 0)
            {
                float belowPerfectThreshold =
                    Mathf.Max(
                        0f,
                        manager.perfectAccuracyThreshold - 0.001f
                    );

                accuracy = Mathf.Min(
                    accuracy,
                    belowPerfectThreshold
                );

                Log(
                    "First foundation landing detected. " +
                    "Perfect landing disabled for this placement."
                );
            }

            ResolveSuccess(manager, targetBounds, accuracy);
            return;
        }

        MarkUnresolvedContact();
    }

    private void MarkUnresolvedContact()
    {
        if (hasUnresolvedContact)
            return;

        hasUnresolvedContact = true;
        unresolvedContactTimer = 0f;

        Log(
            "Contact detected, but it has not yet qualified as a valid landing."
        );
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

    private void ResolveSuccess(
        GameManager manager,
        Bounds targetBounds,
        float accuracy)
    {
        if (resolved)
            return;

        resolved = true;
        hasUnresolvedContact = false;

        // Stop all physics movement before correcting the position.
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        // Freeze the block in place before correcting its final pose.
        body.bodyType = RigidbodyType2D.Kinematic;
        body.freezeRotation = true;

        // Reset the world rotation so the block stacks horizontally.
        // This assumes the block's intended upright rotation is 0 degrees.
        transform.rotation = Quaternion.identity;

        // Recalculate the collider bounds after resetting rotation.
        Bounds correctedBlockBounds = blockCollider.bounds;

        // Align the bottom of this block with the top of the target.
        // Both values are world-space coordinates.
        float correctionY =
            targetBounds.max.y - correctedBlockBounds.min.y;

        transform.position += Vector3.up * correctionY;

        gameObject.tag = "Building";

        manager.SetLastPlacedBlock(gameObject);

        BuildingTier tier = GetComponent<BuildingTier>();

        if (tier != null)
            tier.ApplyTierForFloor(manager.floorsBuilt + 1);

        Log(
            $"LANDING SUCCESS. Accuracy={accuracy:F2}. " +
            $"Floors before update={manager.floorsBuilt}. " +
            $"Vertical correction={correctionY:F3}."
        );

        manager.AddFloor(accuracy);

        ContinueCraneIfPlaying();
    }

    private void ResolveMiss(string reason)
    {
        if (resolved || !hasBeenReleased)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null ||
            manager.isGameOver ||
            manager.isBuildingComplete)
        {
            return;
        }

        resolved = true;

        Log($"BLOCK MISSED: {gameObject.name}. Reason: {reason}");

        gameObject.tag = "Untagged";

        if (blockCollider != null)
            blockCollider.enabled = false;

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
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
        {
            Log("Crane will not continue because the game has ended.");
            return;
        }

        if (CraneController.Instance != null)
        {
            Log("Requesting the crane to prepare the next block.");
            CraneController.Instance.PrepareNextBlock();
        }
        else
        {
            Log(
                "Cannot prepare the next block: " +
                "CraneController.Instance is null."
            );
        }
    }

    private void LogCollision(Collision2D collision, string phase)
    {
        if (!enableDebugLogs || collision == null)
            return;

        string targetName =
            GameManager.Instance != null &&
            GameManager.Instance.LastPlacedBlock != null
                ? GameManager.Instance.LastPlacedBlock.name
                : "NULL";

        Log(
            $"Collision {phase} with '{collision.gameObject.name}'. " +
            $"Released={hasBeenReleased}, " +
            $"BodyType={(body != null ? body.bodyType.ToString() : "NULL")}, " +
            $"Target={targetName}, " +
            $"ContactCount={collision.contactCount}."
        );
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BlockLanding: {gameObject.name}] {message}",
                this
            );
        }
    }
}
