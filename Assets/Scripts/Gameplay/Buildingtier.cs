
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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

            insets = configuration != null
                ? configuration.GetStackInsets(selectedSprite)
                : default;

            if (keepBottom)
                transform.position += Vector3.up * (previousBottom - SolidBottom);

            // Higher floors draw over lower ones where their edges overlap.
            spriteRenderer.sortingOrder = floorNumber;

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
