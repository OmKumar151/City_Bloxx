using UnityEngine;
using TMPro;

public class InfoPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject background;
    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        if (messageText != null)
        {
            messageText.text = "Message";
        }
    }

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

        Debug.Log(
            "Info Panel: " +
            message
        );
    }

    public void ShowBuildingSelected(
        string buildingName)
    {
        ShowMessage(buildingName);
    }

    public void ShowBuildingCanBePlaced()
    {
        ShowMessage(
            "Building can be placed"
        );
    }

    public void ShowBuildingCannotBePlaced()
    {
        ShowMessage(
            "Building cannot be placed"
        );
    }

    public void ShowNoValidPlacement(
        string buildingName)
    {
        ShowMessage(
            buildingName +
            " cannot be placed anywhere"
        );
    }

    public void ShowBuildingPlaced(
        string buildingName)
    {
        ShowMessage(
            buildingName +
            " placed"
        );
    }

    public void ShowBuildingCancelled()
    {
        ShowMessage(
            "Building demolished"
        );
    }

    public void Hide()
    {
        if (background != null)
        {
            background.SetActive(false);
        }
    }
}