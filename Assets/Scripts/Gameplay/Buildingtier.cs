
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BuildingTier : MonoBehaviour
{
    [System.Serializable]
    public class TierData
    {
        public string tierName = "Tier";
        [Min(1)] public int floorsPerTier = 3;
        public Sprite[] sprites;
        public Color fallbackColor = Color.white;
    }

    [Header("Tier Definitions (bottom to top)")]
    public TierData[] tiers =
    {
        new TierData
        {
            tierName = "Base",
            floorsPerTier = 3,
            fallbackColor = new Color(0.82f, 0.71f, 0.55f)
        },
        new TierData
        {
            tierName = "Mid-Rise",
            floorsPerTier = 3,
            fallbackColor = new Color(0.55f, 0.65f, 0.75f)
        },
        new TierData
        {
            tierName = "High-Rise",
            floorsPerTier = 3,
            fallbackColor = new Color(0.85f, 0.55f, 0.35f)
        },
        new TierData
        {
            tierName = "Penthouse",
            floorsPerTier = 3,
            fallbackColor = new Color(0.65f, 0.85f, 0.95f)
        }
    };

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ApplyTierForFloor(int floorNumber)
    {
        if (spriteRenderer == null ||
            tiers == null || tiers.Length == 0)
            return;

        int remaining = Mathf.Max(1, floorNumber);
        int tierIndex = 0;

        for (int i = 0; i < tiers.Length; i++)
        {
            TierData tier = tiers[i];

            if (tier == null)
                continue;

            int floorsInTier = Mathf.Max(1, tier.floorsPerTier);

            if (remaining <= floorsInTier)
            {
                tierIndex = i;
                break;
            }

            remaining -= floorsInTier;
            tierIndex = i;
        }

        TierData selected = tiers[tierIndex];

        if (selected == null)
            return;

        if (selected.sprites != null &&
            selected.sprites.Length > 0)
        {
            spriteRenderer.sprite =
                selected.sprites[Random.Range(0, selected.sprites.Length)];
            spriteRenderer.color = Color.white;
        }
        else
        {
            spriteRenderer.color = selected.fallbackColor;
        }
    }
}
