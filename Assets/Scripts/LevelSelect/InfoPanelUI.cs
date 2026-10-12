
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelUI : MonoBehaviour
{
    private enum MessageStyle
    {
        Normal,
        Reward,
        Unlock,
        Warning
    }

    [Header("UI References")]
    [SerializeField] private GameObject background;
    [SerializeField] private TMP_Text messageText;

    [Header("Panel Colours")]
    [Tooltip("White preserves the original brown/beige artwork.")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("Gold tint for stars, checkpoints and rewards.")]
    [SerializeField]
    private Color rewardColor =
        new Color(1f, 0.82f, 0.48f, 1f);

    [Tooltip("Green-gold tint for major unlocks.")]
    [SerializeField]
    private Color unlockColor =
        new Color(0.78f, 0.91f, 0.57f, 1f);

    [Tooltip("Red tint for resets and destructive events.")]
    [SerializeField]
    private Color warningColor =
        new Color(1f, 0.62f, 0.58f, 1f);

    private Image backgroundImage;

    private void Awake()
    {
        FindBackgroundImage();

        if (messageText != null)
        {
            messageText.text = "Message";
        }

        ApplyStyle(MessageStyle.Normal);
    }

    private void FindBackgroundImage()
    {
        if (background == null)
        {
            Debug.LogWarning(
                "InfoPanelUI: Background is not assigned."
            );

            return;
        }

        backgroundImage = background.GetComponent<Image>();

        if (backgroundImage == null)
        {
            backgroundImage =
                background.GetComponentInChildren<Image>(true);
        }

        if (backgroundImage == null)
        {
            Debug.LogWarning(
                "InfoPanelUI: No Image component was found " +
                "on the Background or its children."
            );
        }
    }

    // =========================================================
    // GENERAL MESSAGE
    // =========================================================

    public void ShowMessage(string message)
    {
        MessageStyle style = GetStyleForMessage(message);

        ShowMessage(message, style);
    }

    private void ShowMessage(
        string message,
        MessageStyle style)
    {
        if (messageText == null)
        {
            Debug.LogWarning(
                "InfoPanelUI: Message Text is not assigned."
            );

            return;
        }

        messageText.text = message;

        if (background != null)
        {
            background.SetActive(true);
        }

        ApplyStyle(style);

        Debug.Log("Info Panel: " + message);
    }

    private MessageStyle GetStyleForMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return MessageStyle.Normal;
        }

        string text = message.ToLowerInvariant();

        // Major positive events.
        if (text.Contains("map unlocked") ||
            text.Contains("building unlocked") ||
            text.Contains("new map") ||
            text.Contains("new building unlocked"))
        {
            return MessageStyle.Unlock;
        }

        // Rewards and checkpoints.
        if (text.Contains("star achieved") ||
            text.Contains("checkpoint") ||
            text.Contains("milestone") ||
            text.Contains("reward achieved"))
        {
            return MessageStyle.Reward;
        }

        // Destructive or cautionary events.
        if (text.Contains("map reset") ||
            text.Contains("building destroyed") ||
            text.Contains("building demolished"))
        {
            return MessageStyle.Warning;
        }

        return MessageStyle.Normal;
    }

    // =========================================================
    // PANEL COLOUR
    // =========================================================

    private void ApplyStyle(MessageStyle style)
    {
        if (backgroundImage == null)
        {
            return;
        }

        switch (style)
        {
            case MessageStyle.Reward:
                backgroundImage.color = rewardColor;
                break;

            case MessageStyle.Unlock:
                backgroundImage.color = unlockColor;
                break;

            case MessageStyle.Warning:
                backgroundImage.color = warningColor;
                break;

            default:
                backgroundImage.color = normalColor;
                break;
        }
    }

    // =========================================================
    // EXISTING GAMEPLAY MESSAGES
    // =========================================================

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
        ShowMessage("Building cannot be placed");
    }

    public void ShowNoValidPlacement(string buildingName)
    {
        ShowMessage(
            buildingName + " cannot be placed anywhere"
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

    // =========================================================
    // CHECKPOINTS AND REWARDS
    // =========================================================

    public void ShowStarAchieved()
    {
        ShowMessage(
            "Star Achieved!",
            MessageStyle.Reward
        );
    }

    public void ShowCheckpointReached()
    {
        ShowMessage(
            "Checkpoint Reached!",
            MessageStyle.Reward
        );
    }

    public void ShowRewardAchieved(string rewardName)
    {
        ShowMessage(
            rewardName + " Achieved!",
            MessageStyle.Reward
        );
    }

    // =========================================================
    // UNLOCKS
    // =========================================================

    public void ShowNextMapUnlocked()
    {
        ShowMessage(
            "Next Map Unlocked!",
            MessageStyle.Unlock
        );
    }

    public void ShowNewBuildingUnlocked(string buildingName)
    {
        ShowMessage(
            "New Building Unlocked: " + buildingName,
            MessageStyle.Unlock
        );
    }

    // =========================================================
    // DESTRUCTIVE EVENTS
    // =========================================================

    public void ShowBuildingDestroyed()
    {
        ShowMessage(
            "Building Destroyed",
            MessageStyle.Warning
        );
    }

    public void ShowMapReset()
    {
        ShowMessage(
            "Map Reset",
            MessageStyle.Warning
        );
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        if (background != null)
        {
            background.SetActive(false);
        }

        // Restore the default tint for the next message.
        ApplyStyle(MessageStyle.Normal);
    }
}
