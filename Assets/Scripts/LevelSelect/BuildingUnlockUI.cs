
using UnityEngine;

public class BuildingUnlockUI : MonoBehaviour
{
    [Header("Building Objects")]
    [SerializeField] private GameObject[] buildingImages;

    [Header("Star Objects")]
    [SerializeField] private GameObject[] starObjects;

    [Header("Info Panel")]
    [SerializeField] private InfoPanelUI infoPanelUI;

    private PopulationProgressionManager subscribedManager;

    private void Start()
    {
        if (infoPanelUI == null)
            infoPanelUI = FindObjectOfType<InfoPanelUI>();

        SubscribeToProgressionManager();
        UpdateUnlockState();
    }

    private void OnEnable()
    {
        // Start may not have run yet, so subscription is also attempted there.
        SubscribeToProgressionManager();
    }

    private void OnDisable()
    {
        UnsubscribeFromProgressionManager();
    }

    private void SubscribeToProgressionManager()
    {
        PopulationProgressionManager manager =
            PopulationProgressionManager.Instance;

        if (manager == null || subscribedManager == manager)
            return;

        UnsubscribeFromProgressionManager();

        subscribedManager = manager;
        subscribedManager.OnRewardUnlocked += HandleRewardUnlocked;
        subscribedManager.OnProgressionReset += HandleProgressionReset;
    }

    private void UnsubscribeFromProgressionManager()
    {
        if (subscribedManager == null)
            return;

        subscribedManager.OnRewardUnlocked -= HandleRewardUnlocked;
        subscribedManager.OnProgressionReset -= HandleProgressionReset;
        subscribedManager = null;
    }

    private void HandleRewardUnlocked(
        PopulationProgressionManager.RewardType reward)
    {
        UpdateUnlockState();

        // The starting blue building does not need a reward notification.
        if (reward ==
            PopulationProgressionManager.RewardType.BlueBuilding)
        {
            return;
        }

        if (infoPanelUI != null)
            infoPanelUI.ShowRewardUnlocked(reward);
    }

    private void HandleProgressionReset()
    {
        UpdateUnlockState();
    }

    private void UpdateUnlockState()
    {
        PopulationProgressionManager manager =
            PopulationProgressionManager.Instance;

        if (manager == null)
            return;

        SetBuildingState(
            0, PopulationProgressionManager.RewardType.BlueBuilding, manager
        );

        SetBuildingState(
            1, PopulationProgressionManager.RewardType.RedBuilding, manager
        );

        SetBuildingState(
            2, PopulationProgressionManager.RewardType.GreenBuilding, manager
        );

        SetBuildingState(
            3, PopulationProgressionManager.RewardType.YellowBuilding, manager
        );

        SetStarState(
            0, PopulationProgressionManager.RewardType.BlueStar, manager
        );

        SetStarState(
            1, PopulationProgressionManager.RewardType.RedStar, manager
        );

        SetStarState(
            2, PopulationProgressionManager.RewardType.GreenStar, manager
        );

        SetStarState(
            3, PopulationProgressionManager.RewardType.YellowStar, manager
        );
    }

    private void SetBuildingState(
        int index,
        PopulationProgressionManager.RewardType reward,
        PopulationProgressionManager manager)
    {
        if (buildingImages == null ||
            index < 0 ||
            index >= buildingImages.Length ||
            buildingImages[index] == null)
        {
            return;
        }

        buildingImages[index].SetActive(manager.IsRewardUnlocked(reward));
    }

    private void SetStarState(
        int index,
        PopulationProgressionManager.RewardType reward,
        PopulationProgressionManager manager)
    {
        if (starObjects == null ||
            index < 0 ||
            index >= starObjects.Length ||
            starObjects[index] == null)
        {
            return;
        }

        starObjects[index].SetActive(manager.IsRewardUnlocked(reward));
    }
}
