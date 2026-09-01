using UnityEngine;
using TMPro;

public class InfoPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject background;
    [SerializeField] private TMP_Text messageText;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (messageText != null)
        {
            messageText.text = "Message";
        }
    }


    // =========================================================
    // GENERAL MESSAGE
    // =========================================================

    public void ShowMessage(string message)
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

        Debug.Log("Info Panel: " + message);
    }


    // =========================================================
    // BUILDING SELECTION
    // =========================================================

    public void ShowBuildingSelected(string buildingName)
    {
        ShowMessage(buildingName);
    }


    // =========================================================
    // PLACEMENT
    // =========================================================

    public void ShowBuildingCanBePlaced()
    {
        ShowMessage("Building can be placed");
    }


    public void ShowBuildingCannotBePlaced()
    {
        ShowMessage("Building cannot be placed");
    }


    // Accepts the building name because NavigationManager
    // currently passes it when no valid placement exists.
    public void ShowNoValidPlacement(string buildingName)
    {
        ShowMessage(
            buildingName +
            " cannot be placed anywhere"
        );
    }


    // =========================================================
    // BUILDING PLACED
    // =========================================================

    public void ShowBuildingPlaced(string buildingName)
    {
        ShowMessage(
            buildingName +
            " placed"
        );
    }


    // =========================================================
    // BUILDING CANCELLED / DEMOLISHED
    // =========================================================

    public void ShowBuildingCancelled()
    {
        ShowMessage("Building demolished");
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
    }
}