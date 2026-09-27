using UnityEngine;

// Placeholder "building set" system — swaps the block's look every few
// floors, like the different building styles in the real Tower Bloxx.
// Right now it tints the sprite per tier (no art needed). Once you have
// real building sprites, just drop them into each tier's "sprites" array
// in the Inspector — sprites take priority over the fallback color.
public class BuildingTier : MonoBehaviour
{
    [System.Serializable]
    public class TierData
    {
        public string tierName = "Tier";

        [Tooltip("How many floors this tier covers before moving to the next tier.")]
        public int floorsPerTier = 3;

        [Tooltip("Optional. If you have real building sprites for this tier, put them here — one will be picked at random. Leave empty to just use the fallback color.")]
        public Sprite[] sprites;

        [Tooltip("Used only if no sprites are assigned above.")]
        public Color fallbackColor = Color.white;
    }

    [Header("Tier Definitions (bottom to top)")]
    public TierData[] tiers = new TierData[]
    {
        new TierData { tierName = "Base",     floorsPerTier = 3, fallbackColor = new Color(0.82f, 0.71f, 0.55f) }, // sandstone
        new TierData { tierName = "Mid-Rise", floorsPerTier = 3, fallbackColor = new Color(0.55f, 0.65f, 0.75f) }, // blue-grey
        new TierData { tierName = "High-Rise",floorsPerTier = 3, fallbackColor = new Color(0.85f, 0.55f, 0.35f) }, // brick orange
        new TierData { tierName = "Penthouse",floorsPerTier = 3, fallbackColor = new Color(0.65f, 0.85f, 0.95f) }, // glass blue
    };

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // floorNumber is 1-based (1 = first block placed above the foundation).
    public void ApplyTierForFloor(int floorNumber)
    {
        if (sr == null || tiers == null || tiers.Length == 0)
            return;

        int tierIndex = 0;
        int remaining = floorNumber;

        foreach (TierData tier in tiers)
        {
            int span = Mathf.Max(1, tier.floorsPerTier);

            if (remaining <= span)
                break;

            remaining -= span;
            tierIndex++;
        }

        tierIndex = Mathf.Clamp(tierIndex, 0, tiers.Length - 1);
        TierData chosen = tiers[tierIndex];

        if (chosen.sprites != null && chosen.sprites.Length > 0)
        {
            sr.sprite = chosen.sprites[Random.Range(0, chosen.sprites.Length)];
            sr.color = Color.white;
        }
        else
        {
            sr.color = chosen.fallbackColor;
        }
    }
}