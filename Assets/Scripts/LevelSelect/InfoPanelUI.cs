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

    // Shown whenever no timed message is up: what the player should do next.
    private System.Func<string> idleHintProvider;

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
            messageText.richText = true;

            // Messages vary in length: shrink long ones to fit the panel
            // instead of spilling out of it.
            float designedSize = messageText.fontSize;
            messageText.enableAutoSizing = true;
            messageText.fontSizeMax = designedSize;
            messageText.fontSizeMin = Mathf.Min(designedSize, 22f);
        }
    }

    // =========================================================
    // NEXT-STEP HINT
    // =========================================================

    /// <summary>
    /// Sets where the standing "what to do next" hint comes from. It is
    /// shown between messages instead of an empty panel.
    /// </summary>
    public void SetIdleHint(System.Func<string> provider)
    {
        idleHintProvider = provider;
        RefreshIdleHint();
    }

    /// <summary>Re-reads the hint, unless a message is currently showing.</summary>
    public void RefreshIdleHint()
    {
        if (hideCoroutine != null)
            return;

        ShowIdleHint();
    }

    private void ShowIdleHint()
    {
        RestoreNormalColour();

        if (messageText == null)
            return;

        string hint = null;

        if (idleHintProvider != null)
        {
            try
            {
                hint = idleHintProvider();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("InfoPanelUI: Hint failed: " + e.Message);
            }
        }

        messageText.text = hint ?? "";
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

        // Back to the next-step hint (or empty without one). The
        // background stays visible.
        ShowIdleHint();
    }
    // =========================================================
    // MESSAGES
    // Building indexes: 0 Blue, 1 Red, 2 Green, 3 Yellow.
    // =========================================================

    public void ShowBuildingSelected(int building)
    {
        ShowMessage(
            GuideContent.Coloured(building) + " Building: " +
            GuideContent.Floors(building) + " floors, " +
            GuideContent.PlacementRule(building) + ". Press OK to build it."
        );
    }

    public void ShowPlacingBuilding(int building, int residents)
    {
        ShowMessage(
            "Your " + GuideContent.Coloured(building) + " building holds " +
            residents.ToString("N0") + " residents. Use the arrows to pick a square, then OK.",
            MessageType.ImportantReward
        );
    }

    public void ShowBuildingCanBePlaced(int building)
    {
        ShowMessage(
            "Good spot! Press OK to place your " +
            GuideContent.Coloured(building) + " building here."
        );
    }

    public void ShowBuildingCanReplace(int building, int oldBuilding, int oldResidents)
    {
        ShowMessage(
            "OK replaces this " + GuideContent.Coloured(oldBuilding) + " (" +
            oldResidents.ToString("N0") + " residents) with your " +
            GuideContent.Coloured(building) + "."
        );
    }

    /// <summary>missing: the buildings this square still needs next to it.</summary>
    public void ShowBuildingCannotBePlaced(int building, System.Collections.Generic.IList<int> missing)
    {
        string reason = missing != null && missing.Count > 0
            ? "it must touch " + GuideContent.JoinBuildings(missing) + " (corners count)."
            : "this square isn't allowed.";

        ShowMessage(
            "Can't place the " + GuideContent.Coloured(building) + " here: " + reason,
            MessageType.PlacementDeclined
        );
    }

    /// <summary>missingFromCity: required buildings the city has none of yet.</summary>
    public void ShowNoValidPlacement(int building, System.Collections.Generic.IList<int> missingFromCity)
    {
        string advice = missingFromCity != null && missingFromCity.Count > 0
            ? "Build " + GuideContent.JoinBuildings(missingFromCity) + " first."
            : "No free square touches " + GuideContent.JoinBuildings(RequiredBy(building)) +
              ". Build more around them.";

        if (building == 0)
            advice = "Your city is full. Use Reset City to start over.";

        ShowMessage(
            "Nowhere for a " + GuideContent.Coloured(building) + " yet. " + advice,
            MessageType.PlacementDeclined
        );
    }

    // Each building needs every colour before it: Red needs Blue, Green
    // needs Blue and Red, Yellow needs all three.
    private static int[] RequiredBy(int building)
    {
        int[] required = new int[Mathf.Max(0, building)];

        for (int i = 0; i < required.Length; i++)
            required[i] = i;

        return required;
    }

    public void ShowBuildingPlaced(int building, int residents)
    {
        ShowMessage(
            GuideContent.Coloured(building) + " building placed! +" +
            residents.ToString("N0") + " residents. Pick your next building.",
            MessageType.ImportantReward
        );
    }

    public void ShowBuildingReplaced(int building, int oldBuilding, int residentChange)
    {
        string change = residentChange >= 0
            ? "+" + residentChange.ToString("N0")
            : residentChange.ToString("N0");

        ShowMessage(
            GuideContent.Coloured(building) + " replaced the " +
            GuideContent.Coloured(oldBuilding) + ": " + change + " residents."
        );
    }

    public void ShowBuildingDiscarded(int building, int residents)
    {
        ShowMessage(
            GuideContent.Coloured(building) + " building thrown away. Its " +
            residents.ToString("N0") + " residents were not added.",
            MessageType.PlacementDeclined
        );
    }

    public void ShowMustPlaceFirst(int building)
    {
        ShowMessage(
            "Place your new " + GuideContent.Coloured(building) +
            " building first, or use Cancel Build to throw it away.",
            MessageType.PlacementDeclined
        );
    }

    public void ShowCheckpointReached(string message)
    {
        ShowMessage(message, MessageType.ImportantReward);
    }

    public void ShowNextMapUnlocked(string mapName)
    {
        ShowMessage(
            mapName + " unlocked! Use the arrows at the top to visit it.",
            MessageType.ImportantReward
        );
    }

    public void ShowMapReset()
    {
        ShowMessage("City reset. Press OK to build a " + GuideContent.Coloured(0) +
                    " building and start again.");
    }

    public void ShowMapSaved()
    {
        ShowMessage("Map Saved");
    }

    public void ShowRewardUnlocked(
        PopulationProgressionManager.RewardType reward)
    {
        int building = GuideContent.RewardBuildingIndex(reward);

        if (GuideContent.IsStar(reward))
        {
            ShowMessage(
                GuideContent.Coloured(building) + " Star earned! Score " +
                GuideContent.SpecialRoofScore(building).ToString("N0") + "+ on a " +
                GuideContent.Coloured(building) + " building for a special roof.",
                MessageType.ImportantReward
            );
            return;
        }

        ShowMessage(
            GuideContent.Coloured(building) + " Building unlocked! It " +
            GuideContent.PlacementRule(building) + ". Pick it with the arrows.",
            MessageType.ImportantReward
        );
    }

    public void Hide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        // Keep the panel background visible.
        ShowIdleHint();
    }

    public void RestoreNormalColour()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
}