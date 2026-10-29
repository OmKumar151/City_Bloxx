
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

    private SpriteRenderer spriteRenderer;
    private BuildingGameplayConfig configuration;

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

            if (configuration != null)
            {
                specialRoofAllowed =
                    manager != null &&
                    manager.score >= configuration.specialRoofScoreThreshold;
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
            spriteRenderer.sprite = selectedSprite;
            spriteRenderer.color = Color.white;
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
