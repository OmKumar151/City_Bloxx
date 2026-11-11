
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BuildingTier : MonoBehaviour
{
    [Header("Fallback Sprites")]
    [Tooltip("Used if no BuildingGameplayConfig is selected.")]
    public Sprite groundSprite;

    public Sprite floorSprite;
    public Sprite topSprite;
    public Sprite specialTopSprite;

    [Header("Fallback Settings")]
    public bool useSpecialTop;
    public Color fallbackColor = Color.white;

    [Header("Physics")]
    [Tooltip("Resize the BoxCollider2D to the current sprite so blocks " +
             "land and stack exactly where they are drawn.")]
    public bool matchColliderToSprite = true;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private BuildingGameplayConfig configuration;
    private BuildingGameplayConfig.StackInsets insets;

    // World-space edges of the solid wall (sprite outline minus the soft
    // fringe / corner caps). Blocks stack wall-on-wall using these.
    public float SolidTop =>
        spriteRenderer.bounds.max.y - insets.top * transform.lossyScale.y;

    public float SolidBottom =>
        spriteRenderer.bounds.min.y + insets.bottom * transform.lossyScale.y;

    public bool HasSprite
    {
        get
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            return spriteRenderer != null && spriteRenderer.sprite != null;
        }
    }

    // How much this block adds to the tower once stacked.
    public float StackedHeight => SolidTop - SolidBottom;

    // The prefab's own world scale, captured before the block is attached
    // to the crane; sprite stretching is applied on top of it.
    private Vector3 baseScale;
    private bool hasBaseScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CaptureBaseScale();
    }

    private void CaptureBaseScale()
    {
        if (hasBaseScale)
            return;

        baseScale = transform.lossyScale;
        hasBaseScale = true;
    }

    private void ApplySpriteScale(Sprite sprite)
    {
        CaptureBaseScale();

        Vector2 stretch = configuration != null
            ? configuration.GetSpriteScale(sprite)
            : Vector2.one;

        // The result is the block's size on screen, whatever it is attached
        // to: the parent's scale is cancelled out.
        Vector3 parentScale = transform.parent != null
            ? transform.parent.lossyScale
            : Vector3.one;

        transform.localScale = new Vector3(
            baseScale.x * stretch.x / SafeScale(parentScale.x),
            baseScale.y * stretch.y / SafeScale(parentScale.y),
            baseScale.z
        );
    }

    // The hook's draw order, so blocks can sit just above it.
    // 0 when there is no crane hook in the scene.
    private static int GetHookSortingOrder()
    {
        CraneController crane = CraneController.Instance;

        if (crane == null || crane.hook == null)
            return 0;

        HookController hookController =
            crane.hook.GetComponent<HookController>();

        return hookController != null ? hookController.sortingOrder : 0;
    }

    private static float SafeScale(float value)
    {
        return Mathf.Abs(value) < 0.0001f ? 1f : value;
    }

    public void Configure(BuildingGameplayConfig selectedConfiguration)
    {
        configuration = selectedConfiguration;

        if (configuration == null)
            return;

        groundSprite = configuration.groundSprite;
        floorSprite = configuration.floorSprite;
        topSprite = configuration.standardRoofSprite;
        specialTopSprite = configuration.specialRoofSprite;
    }

    // Sprite.bounds is in the block's local space (relative to the pivot),
    // which is exactly the space BoxCollider2D size/offset use.
    private void MatchColliderToSprite()
    {
        if (!matchColliderToSprite || spriteRenderer.sprite == null)
            return;

        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider == null)
            return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;

        boxCollider.size = spriteBounds.size;
        boxCollider.offset = spriteBounds.center;
    }

    public void ApplyTierForFloor(int floorNumber)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError(
                "BuildingTier: No SpriteRenderer found.",
                this
            );

            return;
        }

        GameManager manager = GameManager.Instance;

        floorNumber = Mathf.Max(1, floorNumber);

        int totalFloors = manager != null
            ? Mathf.Max(1, manager.floorsToComplete)
            : 1;

        Sprite selectedSprite;

        if (floorNumber == 1)
        {
            selectedSprite = groundSprite;
        }
        else if (floorNumber >= totalFloors)
        {
            bool specialRoofAllowed = useSpecialTop;

            // With a selected building, the special roof needs BOTH the
            // LevelSelect unlock flag and the score threshold. GameManager
            // re-applies this tier when the building completes, so the
            // final score (including the roof floor) is what counts.
            if (configuration != null)
            {
                specialRoofAllowed =
                    manager != null &&
                    manager.IsSpecialRoofEarned;
            }

            selectedSprite = specialRoofAllowed && specialTopSprite != null
                ? specialTopSprite
                : topSprite;
        }
        else
        {
            selectedSprite = floorSprite;
        }

        if (selectedSprite != null)
        {
            // Swapping sprites (e.g. roof -> special roof after landing)
            // would move the bottom edge because the pivot is centred.
            // Keep the solid bottom where it was so the block stays seated.
            bool keepBottom =
                spriteRenderer.sprite != null &&
                spriteRenderer.sprite != selectedSprite;

            float previousBottom = keepBottom ? SolidBottom : 0f;

            spriteRenderer.sprite = selectedSprite;
            spriteRenderer.color = Color.white;

            // Before the bottom is re-measured, so the stretched block
            // stays seated.
            ApplySpriteScale(selectedSprite);

            insets = configuration != null
                ? configuration.GetStackInsets(selectedSprite)
                : default;

            if (keepBottom)
                transform.position += Vector3.up * (previousBottom - SolidBottom);

            // Higher floors draw over lower ones where their edges overlap,
            // and every block draws above the crane hook (hook + floor).
            // Exception: the ground floor draws over everything. Some ground
            // sprites (e.g. Red_Ground's gable) have decoration above the
            // solid wall line the next floor stacks on; drawn underneath,
            // that decoration was hidden once floor 2 landed.
            int floorOrder = floorNumber == 1
                ? totalFloors + 1
                : floorNumber;

            spriteRenderer.sortingOrder = GetHookSortingOrder() + floorOrder;

            MatchColliderToSprite();
            Physics2D.SyncTransforms();
        }
        else
        {
            spriteRenderer.color = fallbackColor;

            Debug.LogWarning(
                "BuildingTier: No sprite assigned for floor " +
                floorNumber + ". Check the selected building configuration.",
                this
            );
        }
    }
}
