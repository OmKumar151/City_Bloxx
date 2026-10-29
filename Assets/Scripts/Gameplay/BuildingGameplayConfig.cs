
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

    [Header("Special Roof")]
    public Sprite standardRoofSprite;
    public Sprite specialRoofSprite;

    [Min(0)]
    public int specialRoofScoreThreshold = 500;

    [Header("Population Progression")]
    public PopulationProgressionManager.RewardType requiredPopulationReward =
        PopulationProgressionManager.RewardType.BlueBuilding;

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
