using UnityEngine;

public class ResetConfirmationUI : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private GameObject confirmationPanel;

    [Header("Save Manager")]
    [SerializeField] private MapSaveManager saveManager;

    private void Awake()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    // =========================================================
    // OPEN POPUP
    // =========================================================

    public void OpenResetConfirmation()
    {
        if (confirmationPanel == null)
        {
            Debug.LogWarning(
                "ResetConfirmationUI: " +
                "Confirmation Panel is not assigned."
            );

            return;
        }

        confirmationPanel.SetActive(true);
    }

    // =========================================================
    // CANCEL
    // =========================================================

    public void CancelReset()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    // =========================================================
    // CONFIRM
    // =========================================================

    public void ConfirmReset()
    {
        if (saveManager == null)
        {
            saveManager =
                MapSaveManager.Instance;
        }

        if (saveManager == null)
        {
            Debug.LogWarning(
                "ResetConfirmationUI: " +
                "MapSaveManager is missing."
            );

            return;
        }

        saveManager.ResetMap();

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
}