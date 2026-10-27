
using UnityEngine;

public class ResetPanelUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The confirmation panel shown when the Reset button is pressed.")]
    [SerializeField] private GameObject resetPanel;

    [Header("Initial State")]
    [Tooltip("If enabled, the confirmation panel starts hidden.")]
    [SerializeField] private bool hidePanelOnAwake = true;

    private void Awake()
    {
        // Do not assume this script's own GameObject is the panel
        // unless no reference has been assigned.
        if (resetPanel == null)
        {
            resetPanel = gameObject;
        }

        if (hidePanelOnAwake && resetPanel != null)
        {
            resetPanel.SetActive(false);
        }
    }

    // =========================================================
    // OPEN RESET CONFIRMATION
    // =========================================================

    public void OpenResetPanel()
    {
        if (resetPanel == null)
        {
            Debug.LogWarning(
                "ResetPanelUI: Reset Panel is not assigned."
            );
            return;
        }

        // Avoid toggling the panel: repeated calls should keep it open.
        if (!resetPanel.activeSelf)
        {
            resetPanel.SetActive(true);
        }

        Debug.Log("Reset confirmation panel opened.");
    }

    // =========================================================
    // CANCEL RESET
    // =========================================================

    public void CancelReset()
    {
        if (resetPanel == null)
        {
            Debug.LogWarning(
                "ResetPanelUI: Cannot cancel because the panel is missing."
            );
            return;
        }

        resetPanel.SetActive(false);

        Debug.Log("Reset cancelled.");
    }

    // =========================================================
    // CONFIRM RESET
    // =========================================================

    public void ConfirmReset()
    {
        MapSaveManager saveManager = MapSaveManager.Instance;

        if (saveManager == null)
        {
            Debug.LogWarning(
                "ResetPanelUI: MapSaveManager is not available. " +
                "The map was not reset."
            );
            return;
        }

        saveManager.ResetMap();

        if (resetPanel != null)
        {
            resetPanel.SetActive(false);
        }

        Debug.Log("Reset confirmed. Map returned to state 0.");
    }
}
