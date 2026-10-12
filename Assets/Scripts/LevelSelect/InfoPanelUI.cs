using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelUI : MonoBehaviour
{
    public enum MessageType
    {
        Normal,
        ImportantReward,
        PlacementDeclined
    }

    [Header("UI References")]
    [SerializeField] private GameObject background;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image backgroundImage;

    [Header("Message Timing")]
    [Tooltip("How long the info panel stays visible before disappearing.")]
    [SerializeField, Min(0.1f)]
    private float messageDuration = 3.5f;

    private Coroutine hideCoroutine;

    [Header("Panel Colours")]
    [Tooltip("White preserves the original background artwork.")]
    [SerializeField]
    private Color normalColor = Color.white;

    [Tooltip("Deep antique gold for rewards and unlocks.")]
    [SerializeField]
    private Color rewardColor =
        new Color(0.75f, 0.54f, 0.14f, 1f);

    [Tooltip("Warm red for declined or invalid placement.")]
    [SerializeField]
    private Color placementDeclinedColor =
        new Color(0.85f, 0.16f, 0.18f, 1f);

    private void Awake()
    {
        if (backgroundImage == null && background != null)
        {
            backgroundImage = background.GetComponent<Image>();
        }

        RestoreNormalColour();

        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    public void ShowMessage(string message)
    {
        ShowMessage(message, MessageType.Normal);
    }

    public void ShowMessage(string message, MessageType type)
    {
        if (messageText == null)
        {
            Debug.LogWarning(
                "InfoPanelUI: Message Text is not assigned."
            );
            return;
        }

        // Cancel the previous timer when a new message appears.
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        messageText.text = message;

        if (background != null)
        {
            background.SetActive(true);
        }

        if (backgroundImage != null)
        {
            switch (type)
            {
                case MessageType.ImportantReward:
                    backgroundImage.color = rewardColor;
                    break;

                case MessageType.PlacementDeclined:
                    backgroundImage.color = placementDeclinedColor;
                    break;

                default:
                    backgroundImage.color = normalColor;
                    break;
            }
        }

        Debug.Log("Info Panel: " + message);

        // Automatically hide the message after the duration.
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(messageDuration);

        hideCoroutine = null;

        // Clear only the message text.
        if (messageText != null)
        {
            messageText.text = "";
        }

        // Restore the original panel colour.
        RestoreNormalColour();

        // Do NOT deactivate the background.
    }
    public void ShowBuildingSelected(string buildingName)
    {
        ShowMessage(buildingName);
    }

    public void ShowBuildingCanBePlaced()
    {
        ShowMessage("Building can be placed");
    }

    public void ShowBuildingCannotBePlaced()
    {
        ShowMessage(
            "Building cannot be placed",
            MessageType.PlacementDeclined
        );
    }

    public void ShowNoValidPlacement(string buildingName)
    {
        ShowMessage(
            buildingName + " cannot be placed anywhere",
            MessageType.PlacementDeclined
        );
    }

    public void ShowBuildingPlaced(string buildingName)
    {
        ShowMessage(buildingName + " placed");
    }

    public void ShowBuildingCancelled()
    {
        ShowMessage("Building demolished");
    }

    public void ShowStarAchieved(string starName)
    {
        ShowMessage(
            starName + " star unlocked!",
            MessageType.ImportantReward
        );
    }

    public void ShowCheckpointReached(string message)
    {
        ShowMessage(message, MessageType.ImportantReward);
    }

    public void ShowNextMapUnlocked(string mapName)
    {
        ShowMessage(
            mapName + " unlocked!",
            MessageType.ImportantReward
        );
    }

    public void ShowMapReset()
    {
        ShowMessage("Map Reset");
    }

    public void ShowMapSaved()
    {
        ShowMessage("Map Saved");
    }

    public void ShowBuildingDestroyed(string buildingName)
    {
        ShowMessage(
            buildingName + " destroyed",
            MessageType.PlacementDeclined
        );
    }

    public void ShowBuildingUnlocked(string buildingName)
    {
        ShowMessage(
            buildingName + " unlocked!",
            MessageType.ImportantReward
        );
    }

    public void ShowRewardUnlocked(
        PopulationProgressionManager.RewardType reward)
    {
        switch (reward)
        {
            case PopulationProgressionManager.RewardType.BlueBuilding:
                ShowBuildingUnlocked("Blue Building");
                break;

            case PopulationProgressionManager.RewardType.RedBuilding:
                ShowBuildingUnlocked("Red Building");
                break;

            case PopulationProgressionManager.RewardType.GreenBuilding:
                ShowBuildingUnlocked("Green Building");
                break;

            case PopulationProgressionManager.RewardType.YellowBuilding:
                ShowBuildingUnlocked("Yellow Building");
                break;

            case PopulationProgressionManager.RewardType.BlueStar:
                ShowStarAchieved("Blue");
                break;

            case PopulationProgressionManager.RewardType.RedStar:
                ShowStarAchieved("Red");
                break;

            case PopulationProgressionManager.RewardType.GreenStar:
                ShowStarAchieved("Green");
                break;

            case PopulationProgressionManager.RewardType.YellowStar:
                ShowStarAchieved("Yellow");
                break;
        }
    }

    public void Hide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        // Clear only the message text.
        if (messageText != null)
        {
            messageText.text = "";
        }

        // Keep the panel background visible.
        RestoreNormalColour();
    }

    public void RestoreNormalColour()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
}