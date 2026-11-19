using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    [Serializable]
    public class CityData
    {
        [Header("City Identity")]
        public string cityId = "city_1";
        public string displayName = "City 1";

        [Header("Grid")]
        public Transform gridRoot;

        [Min(0)]
        [Tooltip("Population required in the previous city to unlock this city. City 1 is unlocked automatically.")]
        public int unlockPopulationRequirement = 0;

        [Header("Paid Unlock")]
        [Tooltip("Lets players buy this city straight away from its preview. " +
                 "Off: it unlocks by population only.")]
        public bool allowPaidUnlock = true;

        [Tooltip("Google Play product ID (one-time, non-consumable). Empty uses " +
                 "\"unlock_\" + City ID, e.g. unlock_city_2. Must match Play Console.")]
        public string unlockProductId = "";

        [Tooltip("Shown on the Unlock button until the store returns the real, " +
                 "localised price.")]
        public string fallbackPriceText = "Unlock";

        [Tooltip("Population this city needs for each reward. Sized to the " +
                 "city's number of cells.")]
        public RewardThresholds rewardThresholds = new RewardThresholds();
    }

    /// <summary>
    /// Population needed in one city for each reward. When Use City
    /// Thresholds is off, PopulationProgressionManager's checkpoints are used.
    /// </summary>
    [Serializable]
    public class RewardThresholds
    {
        public bool useCityThresholds;

        [Min(0)] public int redBuilding;
        [Min(0)] public int greenBuilding;
        [Min(0)] public int blueStar;
        [Min(0)] public int yellowBuilding;
        [Min(0)] public int redStar;
        [Min(0)] public int greenStar;
        [Min(0)] public int yellowStar;

        public int Get(PopulationProgressionManager.RewardType reward)
        {
            switch (reward)
            {
                case PopulationProgressionManager.RewardType.RedBuilding: return redBuilding;
                case PopulationProgressionManager.RewardType.GreenBuilding: return greenBuilding;
                case PopulationProgressionManager.RewardType.BlueStar: return blueStar;
                case PopulationProgressionManager.RewardType.YellowBuilding: return yellowBuilding;
                case PopulationProgressionManager.RewardType.RedStar: return redStar;
                case PopulationProgressionManager.RewardType.GreenStar: return greenStar;
                case PopulationProgressionManager.RewardType.YellowStar: return yellowStar;
                default: return 0;
            }
        }
    }

    /// <summary>
    /// The active city's population requirement for a reward, when that
    /// city has its own thresholds.
    /// </summary>
    public bool TryGetRewardPopulation(
        PopulationProgressionManager.RewardType reward,
        out int population)
    {
        population = 0;

        if (!IsValidCityIndex(activeCityIndex) ||
            cities[activeCityIndex] == null)
        {
            return false;
        }

        RewardThresholds thresholds = cities[activeCityIndex].rewardThresholds;

        if (thresholds == null || !thresholds.useCityThresholds)
            return false;

        population = Mathf.Max(0, thresholds.Get(reward));
        return true;
    }

    public static CityManager Instance { get; private set; }

    [Header("Cities")]
    [SerializeField] private CityData[] cities = Array.Empty<CityData>();
    [SerializeField, Min(0)] private int startingCityIndex = 0;

    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private MapSaveManager mapSaveManager;

    [Header("City Navigation Buttons")]
    [SerializeField] private Button previousCityButton;
    [SerializeField] private Image previousCityButtonImage;
    [SerializeField] private Sprite previousEnabledSprite;
    [SerializeField] private Sprite previousDisabledSprite;

    [SerializeField] private Button nextCityButton;
    [SerializeField] private Image nextCityButtonImage;
    [SerializeField] private Sprite nextEnabledSprite;
    [SerializeField] private Sprite nextDisabledSprite;
    [Tooltip("Shown on the Next button when the next city is locked and the " +
             "button opens its preview. Falls back to the enabled sprite.")]
    [SerializeField] private Sprite nextPreviewSprite;

    [Header("Grid Transition")]
    [SerializeField] private bool fadeGridOnSwitch = true;
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

    private const string ActiveCityKey = "CityBloxx_ActiveCityIndex";
    private const string CityUnlockedPrefix = "CityBloxx_CityUnlocked_";

    private int activeCityIndex = -1;
    private Coroutine switchRoutine;
    private int queuedCityIndex = -1;
    private bool isCheckingUnlocks;

    // Preview: a locked city's grid is shown but cannot be played. The
    // city before it is where its population requirement is earned.
    private bool isPreviewing;
    private int previewSourcePopulation;

    public event Action<int, int> OnCityChanged;

    /// <summary>Raised when a preview starts (true) or ends (false).</summary>
    public event Action<bool> OnPreviewChanged;

    public bool IsPreviewing => isPreviewing;

    public int CityCount => cities == null ? 0 : cities.Length;
    public int ActiveCityIndex => activeCityIndex;
    public string ActiveCityId => GetCityId(activeCityIndex);
    public string ActiveCityName =>
        IsValidCityIndex(activeCityIndex) && cities[activeCityIndex] != null
            ? cities[activeCityIndex].displayName
            : string.Empty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        FindReferences();
        EnsureStartingCityUnlocked();
        RegisterUnlockProducts();
    }

    private void OnEnable()
    {
        PurchaseService.OnProductOwned += HandleProductOwned;
    }

    private void OnDisable()
    {
        PurchaseService.OnProductOwned -= HandleProductOwned;
    }

    private void Start()
    {
        if (cities == null || cities.Length == 0)
        {
            Debug.LogError("CityManager: No cities are configured.", this);
            UpdateNavigationButtonStates();
            return;
        }

        if (boardManager == null)
        {
            Debug.LogError("CityManager: BoardManager is not assigned or found.", this);
            return;
        }

        int savedIndex = PlayerPrefs.GetInt(ActiveCityKey, startingCityIndex);
        if (!IsValidCityIndex(savedIndex) || !IsCityUnlocked(savedIndex))
            savedIndex = FindFirstUnlockedCity();

        CityData startingCity = cities[savedIndex];
        if (startingCity == null || startingCity.gridRoot == null)
        {
            Debug.LogError("CityManager: The starting/saved city or its Grid Root is missing.", this);
            return;
        }

        // Keep only the selected city's grid active.
        for (int i = 0; i < CityCount; i++)
        {
            if (cities[i] != null && cities[i].gridRoot != null)
                cities[i].gridRoot.gameObject.SetActive(i == savedIndex);
        }

        activeCityIndex = savedIndex;

        if (!boardManager.SetGridRoot(startingCity.gridRoot))
        {
            Debug.LogError(
                "CityManager: BoardManager failed to activate grid '" +
                startingCity.gridRoot.name + "'.", this);
            return;
        }

        if (mapSaveManager != null)
            mapSaveManager.LoadMapForActiveCity();

        SaveStore.SetInt(ActiveCityKey, activeCityIndex);
        SaveStore.Save();

        CheckPopulationUnlocks();
        UpdateNavigationButtonStates();
    }

    private void Update()
    {
        // Population can change after any building placement, so check unlocks
        // while gameplay is active. A previewed city has no population.
        if (activeCityIndex >= 0 && !isCheckingUnlocks && !isPreviewing)
            CheckPopulationUnlocks();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void FindReferences()
    {
        if (boardManager == null)
            boardManager = FindFirstObjectByType<BoardManager>();

        if (mapSaveManager == null)
            mapSaveManager = FindFirstObjectByType<MapSaveManager>();
    }

    private void EnsureStartingCityUnlocked()
    {
        if (cities == null || cities.Length == 0)
            return;

        int firstIndex = Mathf.Clamp(startingCityIndex, 0, cities.Length - 1);
        SaveStore.SetInt(CityUnlockedPrefix + GetCityId(firstIndex), 1);
        SaveStore.Save();
    }

    public bool IsCityUnlocked(int index)
    {
        if (!IsValidCityIndex(index) || cities[index] == null)
            return false;

        // The configured starting city is always unlocked.
        if (index == Mathf.Clamp(startingCityIndex, 0, Mathf.Max(0, CityCount - 1)))
            return true;

        // Bought cities stay unlocked even if the unlock key was lost.
        if (PurchaseService.IsOwned(GetUnlockProductId(index)))
            return true;

        return PlayerPrefs.GetInt(CityUnlockedPrefix + GetCityId(index), 0) == 1;
    }

    public void UnlockCity(int index)
    {
        if (!IsValidCityIndex(index) || cities[index] == null)
            return;

        string key = CityUnlockedPrefix + GetCityId(index);
        if (PlayerPrefs.GetInt(key, 0) == 1)
            return;

        SaveStore.SetInt(key, 1);
        SaveStore.Save();

        Debug.Log("CityManager: Unlocked city '" + cities[index].displayName +
                  "' (ID: " + GetCityId(index) + ").", this);

        // Only new unlocks get here (already-unlocked cities return above).
        InfoPanelUI infoPanel = FindAnyObjectByType<InfoPanelUI>();

        // Unlocked while previewing it: the preview becomes the real city.
        bool unlockedInPreview = isPreviewing && index == activeCityIndex;

        if (unlockedInPreview)
            EndPreviewInPlace();

        if (infoPanel != null)
        {
            if (unlockedInPreview)
                infoPanel.ShowMessage(
                    cities[index].displayName + " unlocked! Start building.",
                    InfoPanelUI.MessageType.ImportantReward);
            else
                infoPanel.ShowNextMapUnlocked(cities[index].displayName);
        }

        UpdateNavigationButtonStates();
    }

    private void CheckPopulationUnlocks()
    {
        if (isCheckingUnlocks || isPreviewing || boardManager == null ||
            !IsValidCityIndex(activeCityIndex))
            return;

        isCheckingUnlocks = true;

        try
        {
            int population = Mathf.Max(0, boardManager.GetTotalPopulation());

            // Unlock each next city when this city's population reaches
            // that next city's configured requirement.
            int nextIndex = activeCityIndex + 1;
            if (IsValidCityIndex(nextIndex) &&
                cities[nextIndex] != null &&
                population >= cities[nextIndex].unlockPopulationRequirement)
            {
                UnlockCity(nextIndex);
            }
        }
        finally
        {
            isCheckingUnlocks = false;
        }

        UpdateNavigationButtonStates();
    }

    /// <summary>
    /// The city after the active one: its name, the population this city
    /// needs to unlock it, and whether it is already unlocked.
    /// </summary>
    public bool TryGetNextCity(out string cityName, out int requiredPopulation, out bool unlocked)
    {
        int nextIndex = activeCityIndex + 1;

        cityName = string.Empty;
        requiredPopulation = 0;
        unlocked = false;

        if (!IsValidCityIndex(nextIndex) || cities[nextIndex] == null)
            return false;

        cityName = cities[nextIndex].displayName;
        requiredPopulation = cities[nextIndex].unlockPopulationRequirement;
        unlocked = IsCityUnlocked(nextIndex);
        return true;
    }

    public void PreviousCity()
    {
        // From a preview, Previous goes back to the city it was opened from.
        RequestCitySwitch(activeCityIndex - 1);
    }

    public void NextCity()
    {
        int nextIndex = activeCityIndex + 1;

        if (IsValidCityIndex(nextIndex) && !IsCityUnlocked(nextIndex))
        {
            PreviewNextCity();
            return;
        }

        RequestCitySwitch(nextIndex);
    }

    // =========================================================
    // PREVIEW
    // =========================================================

    /// <summary>True when the next city is locked and can be previewed.</summary>
    public bool CanPreviewNextCity
    {
        get
        {
            int nextIndex = activeCityIndex + 1;

            return !isPreviewing &&
                   IsValidCityIndex(nextIndex) &&
                   cities[nextIndex] != null &&
                   cities[nextIndex].gridRoot != null &&
                   !IsCityUnlocked(nextIndex);
        }
    }

    /// <summary>
    /// Shows the next (locked) city's grid without letting the player
    /// build in it. Previous, or ExitPreview, goes back.
    /// </summary>
    public void PreviewNextCity()
    {
        if (!CanPreviewNextCity || switchRoutine != null || boardManager == null)
        {
            UpdateNavigationButtonStates();
            return;
        }

        // The requirement is met in the city being left, so remember how
        // far along it is for the preview panel.
        previewSourcePopulation = Mathf.Max(0, boardManager.GetTotalPopulation());

        switchRoutine = StartCoroutine(SwitchCityRoutine(activeCityIndex + 1, preview: true));
    }

    public void ExitPreview()
    {
        if (isPreviewing)
            RequestCitySwitch(activeCityIndex - 1);
    }

    /// <summary>Everything the preview panel shows. Only valid while previewing.</summary>
    public bool TryGetPreviewInfo(
        out string cityName,
        out string previousCityName,
        out int requiredPopulation,
        out int currentPopulation)
    {
        cityName = string.Empty;
        previousCityName = string.Empty;
        requiredPopulation = 0;
        currentPopulation = previewSourcePopulation;

        if (!isPreviewing || !IsValidCityIndex(activeCityIndex) || cities[activeCityIndex] == null)
            return false;

        cityName = cities[activeCityIndex].displayName;
        requiredPopulation = cities[activeCityIndex].unlockPopulationRequirement;

        int previousIndex = activeCityIndex - 1;
        if (IsValidCityIndex(previousIndex) && cities[previousIndex] != null)
            previousCityName = cities[previousIndex].displayName;

        return true;
    }

    /// <summary>True when the previewed city has a product to buy.</summary>
    public bool CanBuyPreviewedCity =>
        isPreviewing &&
        IsValidCityIndex(activeCityIndex) &&
        cities[activeCityIndex] != null &&
        GetUnlockProductId(activeCityIndex) != null;

    public string GetPreviewedCityPriceText()
    {
        if (!CanBuyPreviewedCity)
            return string.Empty;

        return PurchaseService.GetPriceText(
            GetUnlockProductId(activeCityIndex),
            cities[activeCityIndex].fallbackPriceText);
    }

    /// <summary>
    /// The store product that unlocks a city, or null if it can't be bought
    /// (paid unlock off, or the starting city).
    /// </summary>
    public string GetUnlockProductId(int index)
    {
        if (!IsValidCityIndex(index) || cities[index] == null || !cities[index].allowPaidUnlock ||
            index == Mathf.Clamp(startingCityIndex, 0, Mathf.Max(0, CityCount - 1)))
        {
            return null;
        }

        string id = cities[index].unlockProductId;

        return string.IsNullOrWhiteSpace(id)
            ? "unlock_" + GetCityId(index).ToLowerInvariant()
            : id.Trim();
    }

    /// <summary>
    /// Buys the previewed city. On success it unlocks and the preview
    /// becomes the playable city. onFailed gets a message for the player,
    /// or null if they cancelled.
    /// </summary>
    public void BuyPreviewedCity(Action onSucceeded = null, Action<string> onFailed = null)
    {
        if (!CanBuyPreviewedCity)
        {
            onFailed?.Invoke("This city can't be bought.");
            return;
        }

        int index = activeCityIndex;

        PurchaseService.Buy(
            GetUnlockProductId(index),
            () =>
            {
                UnlockCity(index);
                onSucceeded?.Invoke();
            },
            onFailed);
    }

    private void RegisterUnlockProducts()
    {
        if (cities == null)
            return;

        var productIds = new System.Collections.Generic.List<string>();

        for (int i = 0; i < CityCount; i++)
        {
            string id = GetUnlockProductId(i);

            if (id != null)
                productIds.Add(id);
        }

        PurchaseService.RegisterProducts(productIds);
    }

    // A purchase completed or was restored from the store.
    private void HandleProductOwned(string productId)
    {
        for (int i = 0; i < CityCount; i++)
        {
            if (string.Equals(GetUnlockProductId(i), productId, StringComparison.Ordinal))
            {
                UnlockCity(i);
            }
        }

        UpdateNavigationButtonStates();
    }

    // The previewed city was unlocked: keep its map loaded and make it
    // the real active city.
    private void EndPreviewInPlace()
    {
        isPreviewing = false;

        SaveStore.SetInt(ActiveCityKey, activeCityIndex);
        SaveStore.Save();

        OnPreviewChanged?.Invoke(false);
        UpdateNavigationButtonStates();
    }

    public void RequestCitySwitch(int newIndex)
    {
        if (!IsValidCityIndex(newIndex) || newIndex == activeCityIndex ||
            !IsCityUnlocked(newIndex))
        {
            UpdateNavigationButtonStates();
            return;
        }

        if (cities[newIndex] == null || cities[newIndex].gridRoot == null)
        {
            Debug.LogWarning("CityManager: City index " + newIndex +
                             " has no configured Grid Root.", this);
            return;
        }

        if (switchRoutine != null)
        {
            queuedCityIndex = newIndex;
            return;
        }

        switchRoutine = StartCoroutine(SwitchCityRoutine(newIndex));
    }

    private IEnumerator SwitchCityRoutine(int newIndex, bool preview = false)
    {
        if (!IsValidCityIndex(newIndex) || (!preview && !IsCityUnlocked(newIndex)))
        {
            FinishSwitchRoutine();
            yield break;
        }

        CityData destination = cities[newIndex];
        if (destination == null || destination.gridRoot == null || boardManager == null)
        {
            Debug.LogError("CityManager: Destination city, Grid Root, or BoardManager is missing.", this);
            FinishSwitchRoutine();
            yield break;
        }

        int previousIndex = activeCityIndex;
        Transform previousRoot = IsValidCityIndex(previousIndex) && cities[previousIndex] != null
            ? cities[previousIndex].gridRoot
            : null;

        // A previewed city is never saved.
        bool wasPreviewing = isPreviewing;

        if (!wasPreviewing && previousIndex >= 0 && mapSaveManager != null)
            mapSaveManager.SaveCurrentCity();

        if (previousRoot != null && previousRoot != destination.gridRoot)
        {
            if (fadeGridOnSwitch)
                yield return FadeGrid(previousRoot, GetGridAlpha(previousRoot), 0f);

            previousRoot.gameObject.SetActive(false);
        }

        // Ensure exactly one configured city grid is active.
        for (int i = 0; i < CityCount; i++)
        {
            if (cities[i] != null && cities[i].gridRoot != null &&
                cities[i].gridRoot != destination.gridRoot)
            {
                cities[i].gridRoot.gameObject.SetActive(false);
            }
        }

        destination.gridRoot.gameObject.SetActive(true);
        activeCityIndex = newIndex;
        isPreviewing = preview;

        if (!boardManager.SetGridRoot(destination.gridRoot))
        {
            activeCityIndex = previousIndex;
            isPreviewing = wasPreviewing;
            destination.gridRoot.gameObject.SetActive(false);

            if (previousRoot != null)
                previousRoot.gameObject.SetActive(true);

            Debug.LogError("CityManager: BoardManager failed to activate grid '" +
                           destination.gridRoot.name + "'.", this);

            FinishSwitchRoutine();
            yield break;
        }

        // Never reopen the game on a city the player can't play.
        if (!preview)
        {
            SaveStore.SetInt(ActiveCityKey, activeCityIndex);
            SaveStore.Save();
        }

        if (mapSaveManager != null)
            mapSaveManager.LoadMapForActiveCity();

        if (fadeGridOnSwitch)
            yield return FadeGrid(destination.gridRoot, 0f, 1f);

        OnCityChanged?.Invoke(previousIndex, newIndex);

        if (wasPreviewing != preview)
            OnPreviewChanged?.Invoke(preview);

        CheckPopulationUnlocks();
        UpdateNavigationButtonStates();

        Debug.Log("CityManager: Active city is '" + destination.displayName +
                  "' (ID: " + ActiveCityId + ").", this);

        FinishSwitchRoutine();
    }

    private void FinishSwitchRoutine()
    {
        switchRoutine = null;
        UpdateNavigationButtonStates();

        if (queuedCityIndex >= 0)
        {
            int queuedIndex = queuedCityIndex;
            queuedCityIndex = -1;

            if (queuedIndex != activeCityIndex && IsCityUnlocked(queuedIndex))
                switchRoutine = StartCoroutine(SwitchCityRoutine(queuedIndex));
        }
    }

    private void UpdateNavigationButtonStates()
    {
        bool canGoPrevious =
            activeCityIndex > 0 &&
            IsValidCityIndex(activeCityIndex - 1) &&
            IsCityUnlocked(activeCityIndex - 1);

        bool nextUnlocked =
            !isPreviewing &&
            IsValidCityIndex(activeCityIndex + 1) &&
            IsCityUnlocked(activeCityIndex + 1);

        // A locked next city can still be opened as a preview.
        bool canPreviewNext = !nextUnlocked && CanPreviewNextCity;

        SetNavigationButton(
            previousCityButton,
            previousCityButtonImage,
            canGoPrevious,
            previousEnabledSprite,
            previousDisabledSprite);

        SetNavigationButton(
            nextCityButton,
            nextCityButtonImage,
            nextUnlocked || canPreviewNext,
            canPreviewNext && nextPreviewSprite != null ? nextPreviewSprite : nextEnabledSprite,
            nextDisabledSprite);
    }

    private static void SetNavigationButton(
        Button button,
        Image buttonImage,
        bool enabled,
        Sprite enabledSprite,
        Sprite disabledSprite)
    {
        if (buttonImage != null)
        {
            Sprite targetSprite = enabled ? enabledSprite : disabledSprite;
            if (targetSprite != null)
                buttonImage.sprite = targetSprite;

            // Keep the disabled artwork fully visible. The disabled sprite itself
            // communicates that the button cannot be used, so avoid fading it out.
            buttonImage.color = Color.white;
        }

        if (button != null)
        {
            // Unity's default Color Tint transition often darkens disabled buttons.
            // Use a white disabled tint so the assigned disabled sprite remains clear.
            ColorBlock colors = button.colors;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            button.colors = colors;
            button.interactable = enabled;
        }
    }

    private int FindFirstUnlockedCity()
    {
        for (int i = 0; i < CityCount; i++)
        {
            if (IsCityUnlocked(i))
                return i;
        }

        return Mathf.Clamp(startingCityIndex, 0, Mathf.Max(0, CityCount - 1));
    }

    private bool IsValidCityIndex(int index)
    {
        return cities != null && index >= 0 && index < cities.Length;
    }

    private string GetCityId(int index)
    {
        if (!IsValidCityIndex(index) || cities[index] == null)
            return "default";

        string id = cities[index].cityId;
        return string.IsNullOrWhiteSpace(id) ? "city_" + (index + 1) : id.Trim();
    }

    private static float GetGridAlpha(Transform gridRoot)
    {
        if (gridRoot == null)
            return 1f;

        CanvasGroup group = gridRoot.GetComponent<CanvasGroup>();
        return group != null ? group.alpha : 1f;
    }

    private IEnumerator FadeGrid(Transform gridRoot, float from, float to)
    {
        if (gridRoot == null)
            yield break;

        CanvasGroup group = gridRoot.GetComponent<CanvasGroup>();
        if (group == null)
            group = gridRoot.gameObject.AddComponent<CanvasGroup>();

        group.alpha = from;
        group.interactable = true;
        group.blocksRaycasts = true;

        if (fadeDuration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        group.alpha = to;
    }
}
