using UnityEngine;

public class ResetPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject resetPanel;

    private void Awake()
    {
        if (resetPanel == null)
        {
            resetPanel = gameObject;
        }

        resetPanel.SetActive(false);
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

        resetPanel.SetActive(true);

        Debug.Log(
            "Reset confirmation panel opened."
        );
    }


    // =========================================================
    // CANCEL RESET
    // =========================================================

    public void CancelReset()
    {
        if (resetPanel == null)
        {
            return;
        }

        resetPanel.SetActive(false);

        Debug.Log(
            "Reset cancelled."
        );
    }


    // =========================================================
    // CONFIRM RESET
    // =========================================================

    public void ConfirmReset()
    {
        if (MapSaveManager.Instance == null)
        {
            Debug.LogWarning(
                "ResetPanelUI: MapSaveManager is not available."
            );

            return;
        }

        MapSaveManager.Instance.ResetMap();

        if (resetPanel != null)
        {
            resetPanel.SetActive(false);
        }

        Debug.Log(
            "Reset confirmed. Map returned to state 0."
        );
    }
}