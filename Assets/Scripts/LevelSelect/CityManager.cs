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
        [Tooltip("Population required in the previous/current city to unlock this city. City 1 is unlocked automatically.")]
        public int unlockPopulationRequirement = 0;
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

    [Header("Grid Transition")]
    [SerializeField] private bool fadeGridOnSwitch = true;
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

    private const string ActiveCityKey = "CityBloxx_ActiveCityIndex";
    private const string CityUnlockedPrefix = "CityBloxx_CityUnlocked_";

    private int activeCityIndex = -1;
    private Coroutine switchRoutine;
    private int queuedCityIndex = -1;
    private bool isCheckingUnlocks;

    public event Action<int, int> OnCityChanged;

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

        PlayerPrefs.SetInt(ActiveCityKey, activeCityIndex);
        PlayerPrefs.Save();

        CheckPopulationUnlocks();
        UpdateNavigationButtonStates();
    }

    private void Update()
    {
        // Population can change after any building placement, so check unlocks
        // while gameplay is active.
        if (activeCityIndex >= 0 && !isCheckingUnlocks)
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
        PlayerPrefs.SetInt(CityUnlockedPrefix + GetCityId(firstIndex), 1);
        PlayerPrefs.Save();
    }

    public bool IsCityUnlocked(int index)
    {
        if (!IsValidCityIndex(index) || cities[index] == null)
            return false;

        // The configured starting city is always unlocked.
        if (index == Mathf.Clamp(startingCityIndex, 0, Mathf.Max(0, CityCount - 1)))
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

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log("CityManager: Unlocked city '" + cities[index].displayName +
                  "' (ID: " + GetCityId(index) + ").", this);

        UpdateNavigationButtonStates();
    }

    private void CheckPopulationUnlocks()
    {
        if (isCheckingUnlocks || boardManager == null ||
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

    public void PreviousCity()
    {
        RequestCitySwitch(activeCityIndex - 1);
    }

    public void NextCity()
    {
        RequestCitySwitch(activeCityIndex + 1);
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

        switchRoutine = StartCoroutine(SwitchCityRoutine(newIndex, true));
    }

    private IEnumerator SwitchCityRoutine(int newIndex, bool savePrevious)
    {
        if (!IsValidCityIndex(newIndex) || !IsCityUnlocked(newIndex))
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

        if (savePrevious && previousIndex >= 0 && mapSaveManager != null)
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

        if (!boardManager.SetGridRoot(destination.gridRoot))
        {
            activeCityIndex = previousIndex;
            destination.gridRoot.gameObject.SetActive(false);

            if (previousRoot != null)
                previousRoot.gameObject.SetActive(true);

            Debug.LogError("CityManager: BoardManager failed to activate grid '" +
                           destination.gridRoot.name + "'.", this);

            FinishSwitchRoutine();
            yield break;
        }

        PlayerPrefs.SetInt(ActiveCityKey, activeCityIndex);
        PlayerPrefs.Save();

        if (mapSaveManager != null)
            mapSaveManager.LoadMapForActiveCity();

        if (fadeGridOnSwitch)
            yield return FadeGrid(destination.gridRoot, 0f, 1f);

        OnCityChanged?.Invoke(previousIndex, newIndex);
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
                switchRoutine = StartCoroutine(SwitchCityRoutine(queuedIndex, true));
        }
    }

    private void UpdateNavigationButtonStates()
    {
        bool canGoPrevious =
            activeCityIndex > 0 &&
            IsValidCityIndex(activeCityIndex - 1) &&
            IsCityUnlocked(activeCityIndex - 1);

        bool canGoNext =
            IsValidCityIndex(activeCityIndex + 1) &&
            IsCityUnlocked(activeCityIndex + 1);

        SetNavigationButton(
            previousCityButton,
            previousCityButtonImage,
            canGoPrevious,
            previousEnabledSprite,
            previousDisabledSprite);

        SetNavigationButton(
            nextCityButton,
            nextCityButtonImage,
            canGoNext,
            nextEnabledSprite,
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
