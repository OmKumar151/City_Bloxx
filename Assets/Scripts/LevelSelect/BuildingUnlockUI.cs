using UnityEngine;

public class BuildingUnlockUI : MonoBehaviour
{
    [Header("Building Objects")]
    [SerializeField] private GameObject[] buildingImages;

    [Header("Star Objects")]
    [SerializeField] private GameObject[] starObjects;

    private void Start()
    {
        UpdateUnlockState();

        if (PopulationProgressionManager.Instance != null)
        {
            PopulationProgressionManager.Instance.OnRewardUnlocked +=
                HandleRewardUnlocked;
        }
    }

    private void OnDestroy()
    {
        if (PopulationProgressionManager.Instance != null)
        {
            PopulationProgressionManager.Instance.OnRewardUnlocked -=
                HandleRewardUnlocked;
        }
    }

    private void HandleRewardUnlocked(
        PopulationProgressionManager.RewardType reward)
    {
        UpdateUnlockState();
    }

    private void UpdateUnlockState()
    {
        if (PopulationProgressionManager.Instance == null)
        {
            return;
        }

        SetBuildingState(
            0,
            PopulationProgressionManager.RewardType.BlueBuilding
        );

        SetBuildingState(
            1,
            PopulationProgressionManager.RewardType.RedBuilding
        );

        SetBuildingState(
            2,
            PopulationProgressionManager.RewardType.GreenBuilding
        );

        SetBuildingState(
            3,
            PopulationProgressionManager.RewardType.YellowBuilding
        );

        SetStarState(
            0,
            PopulationProgressionManager.RewardType.BlueStar
        );

        SetStarState(
            1,
            PopulationProgressionManager.RewardType.RedStar
        );

        SetStarState(
            2,
            PopulationProgressionManager.RewardType.GreenStar
        );

        SetStarState(
            3,
            PopulationProgressionManager.RewardType.YellowStar
        );
    }

    private void SetBuildingState(
        int index,
        PopulationProgressionManager.RewardType reward)
    {
        if (buildingImages == null ||
            index < 0 ||
            index >= buildingImages.Length)
        {
            return;
        }

        if (buildingImages[index] == null)
        {
            return;
        }

        bool unlocked =
            PopulationProgressionManager.Instance
                .IsRewardUnlocked(reward);

        buildingImages[index].SetActive(unlocked);
    }

    private void SetStarState(
        int index,
        PopulationProgressionManager.RewardType reward)
    {
        if (starObjects == null ||
            index < 0 ||
            index >= starObjects.Length)
        {
            return;
        }

        if (starObjects[index] == null)
        {
            return;
        }

        bool unlocked =
            PopulationProgressionManager.Instance
                .IsRewardUnlocked(reward);

        starObjects[index].SetActive(unlocked);
    }
}