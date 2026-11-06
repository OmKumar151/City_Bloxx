
using UnityEngine;

/// <summary>
/// The crane hook. Works out where the rope attaches (top of the hook
/// sprite) and where the block hangs (the hook's tip), in world space.
/// The hook can be scaled freely to size its sprite: blocks are not
/// parented to it, so they never inherit that scale.
/// </summary>
public class HookController : MonoBehaviour
{
    [Header("Hook Sprite")]
    [Tooltip("Optional. Found on this object if left empty.")]
    [SerializeField] private SpriteRenderer hookRenderer;

    [Tooltip("Draw order of the hook. Keep it above the rope. Building " +
             "blocks draw at this + their floor number, so they are " +
             "always in front of the hook.")]
    public int sortingOrder = 101;

    [Header("Attachment (world units)")]
    [Tooltip("How far the rope's end tucks down into the hook's top loop.")]
    [Min(0f)] public float ropeOverlap = 0.08f;

    [Tooltip("How far the hanging block's top edge rises over the hook's " +
             "tip. Higher = block hangs higher, covering more of the hook.")]
    [Min(0f)] public float blockOverlap = 0.2f;

    [Tooltip("Used only when the hook has no sprite: distance from the " +
             "hook's origin down to the block.")]
    [Min(0f)] public float blockOffset = 0.65f;

    private void Awake()
    {
        FindRenderer();

        if (HasSprite)
        {
            hookRenderer.enabled = true;
            hookRenderer.sortingOrder = sortingOrder;
        }
    }

    private void FindRenderer()
    {
        if (hookRenderer == null)
            hookRenderer = GetComponent<SpriteRenderer>();
    }

    private bool HasSprite =>
        hookRenderer != null && hookRenderer.sprite != null;

    /// <summary>Top of the hook, where the rope ends.</summary>
    public Vector3 RopeAttachPoint
    {
        get
        {
            FindRenderer();

            if (!HasSprite)
                return transform.position;

            // Local space, so it follows the hook's swing and scale.
            Vector3 top = transform.TransformPoint(
                new Vector3(0f, hookRenderer.sprite.bounds.max.y, 0f)
            );

            return top - transform.up * ropeOverlap;
        }
    }

    /// <summary>Tip of the hook, where the block's top edge hangs.</summary>
    public Vector3 BlockAttachPoint
    {
        get
        {
            FindRenderer();

            if (!HasSprite)
                return transform.position - transform.up * blockOffset;

            Vector3 tip = transform.TransformPoint(
                new Vector3(0f, hookRenderer.sprite.bounds.min.y, 0f)
            );

            return tip + transform.up * blockOverlap;
        }
    }

    // Kept for older callers.
    public Vector3 GetBlockPosition()
    {
        return BlockAttachPoint;
    }
}
