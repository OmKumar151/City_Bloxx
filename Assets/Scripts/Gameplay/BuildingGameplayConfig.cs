
using UnityEngine;

[CreateAssetMenu(
    fileName = "BuildingGameplayConfig",
    menuName = "City Bloxx/Building Gameplay Configuration"
)]
public class BuildingGameplayConfig : ScriptableObject
{
    [Header("Building Identity")]
    public string buildingId = "blue";
    public string displayName = "Blue Building";

    [Header("Gameplay Rules")]
    [Min(1)] public int floorsToComplete = 10;
    public GameObject hangingBlockPrefab;

    [Header("Building Appearance")]
    public Sprite groundSprite;
    public Sprite floorSprite;

    [Tooltip("Stretch floor blocks to exactly the ground sprite's width and " +
             "height. The floor art is scaled, not cropped.")]
    public bool stretchFloorToGroundSize;

    [Header("Special Roof")]
    public Sprite standardRoofSprite;
    public Sprite specialRoofSprite;

    [Min(0)]
    [Tooltip("Gameplay score needed at completion to show the special roof.")]
    public int specialRoofScoreThreshold = 500;

    [Tooltip("LevelSelect reward that unlocks the special roof for this building.")]
    public PopulationProgressionManager.RewardType specialRoofUnlockReward =
        PopulationProgressionManager.RewardType.BlueStar;

    [Header("Population Progression")]
    public PopulationProgressionManager.RewardType requiredPopulationReward =
        PopulationProgressionManager.RewardType.BlueBuilding;

    // How far the solid wall edge sits inside the sprite outline, in the
    // sprite's local units. The art has corner caps and soft anti-aliased
    // rows above/below the wall, so stacking outline-on-outline leaves a
    // visible gap. Blocks stack on these solid edges instead.
    [System.Serializable]
    public struct StackInsets
    {
        [Min(0f)] public float top;
        [Min(0f)] public float bottom;
    }

    [Header("Stacking (auto-measured from the sprites in the editor)")]
    public StackInsets groundInsets;
    public StackInsets floorInsets;
    public StackInsets standardRoofInsets;
    public StackInsets specialRoofInsets;

    public StackInsets GetStackInsets(Sprite sprite)
    {
        if (sprite == null)
            return default;

        if (sprite == groundSprite) return groundInsets;
        if (sprite == floorSprite) return floorInsets;
        if (sprite == standardRoofSprite) return standardRoofInsets;
        if (sprite == specialRoofSprite) return specialRoofInsets;

        return default;
    }

    /// <summary>
    /// Extra scale for a block showing this sprite. Floors are stretched to
    /// the ground sprite's size when stretchFloorToGroundSize is on;
    /// every other sprite keeps its own size.
    /// </summary>
    public Vector2 GetSpriteScale(Sprite sprite)
    {
        if (!stretchFloorToGroundSize ||
            sprite == null ||
            sprite != floorSprite ||
            groundSprite == null)
        {
            return Vector2.one;
        }

        Vector2 floorSize = sprite.bounds.size;
        Vector2 groundSize = groundSprite.bounds.size;

        if (floorSize.x <= 0f || floorSize.y <= 0f)
            return Vector2.one;

        return new Vector2(
            groundSize.x / floorSize.x,
            groundSize.y / floorSize.y
        );
    }

#if UNITY_EDITOR
    // A row counts as solid wall once its average opacity reaches this.
    private const float SolidRowAlpha = 0.5f;

    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += MeasureStackInsets;
    }

    [ContextMenu("Measure Stack Insets")]
    private void MeasureStackInsets()
    {
        if (this == null)
            return;

        StackInsets ground = Measure(groundSprite);
        StackInsets floor = Measure(floorSprite);
        StackInsets roof = Measure(standardRoofSprite);
        StackInsets special = Measure(specialRoofSprite);

        if (ground.Equals(groundInsets) && floor.Equals(floorInsets) &&
            roof.Equals(standardRoofInsets) && special.Equals(specialRoofInsets))
            return;

        groundInsets = ground;
        floorInsets = floor;
        standardRoofInsets = roof;
        specialRoofInsets = special;

        UnityEditor.EditorUtility.SetDirty(this);
    }

    // Reads the source PNG (the imported texture isn't CPU-readable) and
    // counts the mostly-transparent rows at the top and bottom of the
    // sprite rect.
    private static StackInsets Measure(Sprite sprite)
    {
        if (sprite == null)
            return default;

        string path = UnityEditor.AssetDatabase.GetAssetPath(sprite);

        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            return default;

        Texture2D texture = new Texture2D(2, 2);

        try
        {
            if (!texture.LoadImage(System.IO.File.ReadAllBytes(path)))
                return default;

            Rect rect = sprite.rect;

            // sprite.rect is in imported-texture pixels; scale to the PNG.
            float scaleX = texture.width / (float)sprite.texture.width;
            float scaleY = texture.height / (float)sprite.texture.height;

            int xMin = Mathf.RoundToInt(rect.xMin * scaleX);
            int xMax = Mathf.RoundToInt(rect.xMax * scaleX);
            int yMin = Mathf.RoundToInt(rect.yMin * scaleY);
            int yMax = Mathf.RoundToInt(rect.yMax * scaleY);
            int width = Mathf.Max(1, xMax - xMin);

            Color32[] pixels = texture.GetPixels32();

            bool IsSolidRow(int y)
            {
                int sum = 0;

                for (int x = xMin; x < xMax; x++)
                    sum += pixels[y * texture.width + x].a;

                return sum / (255f * width) >= SolidRowAlpha;
            }

            int topRows = 0;

            for (int y = yMax - 1; y >= yMin && !IsSolidRow(y); y--)
                topRows++;

            int bottomRows = 0;

            for (int y = yMin; y < yMax && !IsSolidRow(y); y++)
                bottomRows++;

            // Convert source pixels to the sprite's local units.
            float unitsPerPixel = 1f / (sprite.pixelsPerUnit * scaleY);

            return new StackInsets
            {
                top = topRows * unitsPerPixel,
                bottom = bottomRows * unitsPerPixel
            };
        }
        finally
        {
            DestroyImmediate(texture);
        }
    }
#endif

    public bool HasValidGameplaySetup()
    {
        bool valid = true;

        if (hangingBlockPrefab == null)
        {
            Debug.LogError(
                displayName + ": Hanging Block Prefab is not assigned.",
                this
            );

            return false;
        }

        if (hangingBlockPrefab.GetComponent<BuildingTier>() == null)
        {
            Debug.LogError(
                displayName +
                ": Hanging Block Prefab needs a BuildingTier component.",
                this
            );

            valid = false;
        }

        if (groundSprite == null)
        {
            Debug.LogWarning(
                displayName + ": Ground Sprite is not assigned.",
                this
            );
        }

        if (floorSprite == null)
        {
            Debug.LogWarning(
                displayName + ": Floor Sprite is not assigned.",
                this
            );
        }

        if (standardRoofSprite == null)
        {
            Debug.LogWarning(
                displayName + ": Standard Roof Sprite is not assigned.",
                this
            );
        }

        return valid;
    }
}
