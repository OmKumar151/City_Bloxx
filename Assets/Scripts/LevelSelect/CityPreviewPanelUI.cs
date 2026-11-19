using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Card shown while a locked city is being previewed: how close the
/// player is to unlocking it with population, and a button to buy it now.
///
/// The card's objects live in the LevelSelect scene (Canvas/CityPreview):
/// move or restyle them there. This sits on the always-active CityPreview
/// object and shows/hides Panel Root. The buttons call BuyNow / Back
/// through their OnClick lists.
/// </summary>
public class CityPreviewPanelUI : MonoBehaviour
{
    [Header("Card")]
    [Tooltip("Shown only while previewing. Must not be this object.")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text")]
    [Tooltip("e.g. \"Preview: Harbour City\"")]
    [SerializeField] private TMP_Text titleText;
    [Tooltip("e.g. \"Reach 12,000 residents in Downtown (4,200 / 12,000)\"")]
    [SerializeField] private TMP_Text requirementText;
    [Tooltip("Purchase errors and \"payment pending\" messages.")]
    [SerializeField] private TMP_Text statusText;

    [Header("Progress")]
    [Tooltip("Image with Image Type = Filled.")]
    [SerializeField] private Image progressFill;

    [Header("Buttons")]
    [SerializeField] private Button unlockNowButton;
    [Tooltip("Shows the store price, e.g. \"UNLOCK NOW  ₹89\".")]
    [SerializeField] private TMP_Text unlockNowLabel;
    [SerializeField] private string unlockNowFormat = "UNLOCK NOW  {0}";

    private void OnEnable()
    {
        PurchaseService.OnStoreUpdated += Refresh;
    }

    private void Start()
    {
        if (CityManager.Instance != null)
            CityManager.Instance.OnPreviewChanged += HandlePreviewChanged;

        SetStatus(null);
        Refresh();
    }

    private void OnDisable()
    {
        PurchaseService.OnStoreUpdated -= Refresh;
    }

    private void OnDestroy()
    {
        if (CityManager.Instance != null)
            CityManager.Instance.OnPreviewChanged -= HandlePreviewChanged;
    }

    private void HandlePreviewChanged(bool previewing)
    {
        SetStatus(null);
        Refresh();
    }

    private void Refresh()
    {
        CityManager city = CityManager.Instance;
        string cityName = null, fromCity = null;
        int needed = 0, have = 0;

        bool previewing = city != null && city.TryGetPreviewInfo(
            out cityName, out fromCity, out needed, out have);

        if (panelRoot != null)
            panelRoot.SetActive(previewing);

        if (!previewing)
            return;

        if (titleText != null)
            titleText.text = "Preview: " + cityName;

        if (requirementText != null)
        {
            requirementText.text =
                "Reach <b>" + needed.ToString("N0") + "</b> residents in " + fromCity +
                "\n(" + Mathf.Min(have, needed).ToString("N0") + " / " + needed.ToString("N0") + ")";
        }

        if (progressFill != null)
            progressFill.fillAmount = needed <= 0 ? 1f : Mathf.Clamp01((float)have / needed);

        bool canBuy = city.CanBuyPreviewedCity;

        if (unlockNowButton != null)
        {
            unlockNowButton.gameObject.SetActive(canBuy);
            unlockNowButton.interactable = canBuy && !PurchaseService.IsBusy;
        }

        if (unlockNowLabel != null && canBuy)
            unlockNowLabel.text = string.Format(unlockNowFormat, city.GetPreviewedCityPriceText());
    }

    /// <summary>Unlock Now button (OnClick).</summary>
    public void BuyNow()
    {
        CityManager city = CityManager.Instance;

        if (city == null || !city.CanBuyPreviewedCity || PurchaseService.IsBusy)
            return;

        SetStatus("Opening Google Play…");

        if (unlockNowButton != null)
            unlockNowButton.interactable = false;

        city.BuyPreviewedCity(
            onSucceeded: () =>
            {
                // CityManager has turned the preview into the real city,
                // which hides this card through OnPreviewChanged.
                SetStatus(null);
                Refresh();
            },
            onFailed: message =>
            {
                // null message: the player cancelled.
                SetStatus(message);
                Refresh();
            });
    }

    /// <summary>Back button (OnClick).</summary>
    public void Back()
    {
        if (CityManager.Instance != null)
            CityManager.Instance.ExitPreview();
    }

    private void SetStatus(string message)
    {
        if (statusText == null)
            return;

        statusText.text = message ?? string.Empty;
        statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }
}
