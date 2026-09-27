using UnityEngine;

public class BlockLanding : MonoBehaviour
{
    private bool hasLanded = false;

    [Header("Stacking")]
    [Tooltip("Minimum horizontal overlap (world units) with the block below required to count as a valid stack.")]
    public float minOverlap = 0.3f;

    [Tooltip("Seconds before a missed block is removed from the scene.")]
    public float missedBlockLifetime = 0.4f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded)
            return;

        // React to touching the tower, the foundation, or the ground —
        // anything relevant is tagged "Building".
        if (!collision.gameObject.CompareTag("Building"))
            return;

        if (GameManager.Instance == null)
            return;

        hasLanded = true;

        GameObject expectedTarget = GameManager.Instance.LastPlacedBlock;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D targetCollider =
            expectedTarget != null ? expectedTarget.GetComponent<Collider2D>() : null;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;
        }

        bool validStack = false;
        float accuracy = 0f;

        // Evaluate against the REAL current top block's bounds, no matter
        // what object we actually collided with (foundation, ground, side
        // piece, etc). This is what makes a wide-miss get detected instead
        // of silently doing nothing.
        if (myCollider != null && targetCollider != null)
        {
            Bounds myBounds = myCollider.bounds;
            Bounds targetBounds = targetCollider.bounds;

            float overlapMin = Mathf.Max(myBounds.min.x, targetBounds.min.x);
            float overlapMax = Mathf.Min(myBounds.max.x, targetBounds.max.x);
            float overlapWidth = Mathf.Max(0f, overlapMax - overlapMin);

            // Accuracy = how much of this block's width overlaps the block
            // below it. 1.0 = perfectly centered/flush, 0 = barely touching.
            accuracy = myBounds.size.x > 0f
                ? Mathf.Clamp01(overlapWidth / myBounds.size.x)
                : 0f;

            // Guard against a block that rolled down to ground level and
            // happens to x-overlap the target's span even though it's
            // clearly not sitting on top of it.
            bool atRightHeight = myBounds.min.y >= targetBounds.min.y;

            validStack = atRightHeight && overlapWidth >= minOverlap;

            if (validStack)
            {
                // Snap flush on top of the block below — the actual "stacking".
                float topOfTarget = targetBounds.max.y;
                float myHalfHeight = myBounds.extents.y;

                Vector3 pos = transform.position;
                pos.y = topOfTarget + myHalfHeight;
                transform.position = pos;
            }
        }

        if (validStack)
        {
            Debug.Log("BLOCK STACKED: " + gameObject.name + " (accuracy " + accuracy.ToString("P0") + ")");

            gameObject.tag = "Building";

            GameManager.Instance.SetLastPlacedBlock(gameObject);
            GameManager.Instance.AddFloor(accuracy);

            // Apply the placeholder "building set" look for this floor.
            BuildingTier tier = GetComponent<BuildingTier>();
            if (tier != null)
            {
                tier.ApplyTierForFloor(GameManager.Instance.floorsBuilt);
            }
        }
        else
        {
            Debug.Log("BLOCK MISSED (off to the side): " + gameObject.name);

            // Don't leave it tagged "Building" — stops a missed block from
            // being used as a landing surface for the next one.
            gameObject.tag = "Untagged";

            GameManager.Instance.LoseLife();

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic; // let it topple away
            }

            Destroy(gameObject, missedBlockLifetime);
        }

        // Always tell the crane to continue, whether this was a hit or a miss.
        if (CraneController.Instance != null)
        {
            CraneController.Instance.PrepareNextBlock();
        }
    }
}