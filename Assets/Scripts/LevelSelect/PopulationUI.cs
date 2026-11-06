using TMPro;
using UnityEngine;

/// <summary>
/// Shows the city's total population. When the total changes, the number
/// counts up or down through the values instead of jumping.
/// </summary>
public class PopulationUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text populationText;

    [Header("Board")]
    [SerializeField] private BoardManager boardManager;

    [Header("Count Animation")]
    [Tooltip("Population counted per second before the duration limits apply.")]
    [SerializeField, Min(1f)] private float countSpeed = 2000f;

    [Tooltip("Shortest count, so small changes are still visible.")]
    [SerializeField, Min(0.05f)] private float minCountDuration = 0.5f;

    [Tooltip("Longest count, so big changes don't drag on.")]
    [SerializeField, Min(0.05f)] private float maxCountDuration = 1.5f;

    [Tooltip("Pause before counting up the score earned in Gameplay.")]
    [SerializeField, Min(0f)] private float earnedScoreDelay = 0.4f;

    private float displayedValue;
    private float startValue;
    private int targetValue;
    private float elapsed;
    private float duration;
    private bool isCounting;

    // While snapping, the number is set directly (scene load, city switch)
    // because the board is still being restored.
    private int snapUntilFrame;
    private int earnedScoreAtLoad;

    private void Start()
    {
        FindBoardManager();
        SubscribeToCityManager();

        // Returning from a won game: the earned score is already in the
        // total. Start below it so it counts up into view.
        earnedScoreAtLoad =
            PendingPlacementStore.IsForCity(PopulationBonusStore.ActiveCityId)
                ? PendingPlacementStore.Score
                : 0;

        // MapSaveManager restores the board during Start; wait a frame.
        snapUntilFrame = Time.frameCount + 1;
        Snap(GetTotalPopulation() - earnedScoreAtLoad);
    }

    private void OnDestroy()
    {
        if (CityManager.Instance != null)
            CityManager.Instance.OnCityChanged -= HandleCityChanged;
    }

    private void SubscribeToCityManager()
    {
        if (CityManager.Instance == null)
            return;

        CityManager.Instance.OnCityChanged -= HandleCityChanged;
        CityManager.Instance.OnCityChanged += HandleCityChanged;
    }

    // A different city's population is a different number, not a change.
    private void HandleCityChanged(int previousCityIndex, int newCityIndex)
    {
        earnedScoreAtLoad =
            PendingPlacementStore.IsForCity(PopulationBonusStore.ActiveCityId)
                ? PendingPlacementStore.Score
                : 0;

        snapUntilFrame = Time.frameCount + 1;
    }

    private void Update()
    {
        if (populationText == null)
            return;

        int total = GetTotalPopulation();

        if (Time.frameCount <= snapUntilFrame)
        {
            Snap(total - earnedScoreAtLoad);
            return;
        }

        if (earnedScoreAtLoad > 0)
        {
            earnedScoreAtLoad = 0;
            BeginCount(total, earnedScoreDelay);
        }
        else if (total != targetValue)
        {
            BeginCount(total, 0f);
        }

        if (!isCounting)
            return;

        // Unscaled so a paused timeScale never freezes the counter.
        elapsed += Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(elapsed / duration);

        // Ease out: fast at first, slowing as it reaches the target.
        float eased = 1f - Mathf.Pow(1f - t, 3f);

        displayedValue = Mathf.Lerp(startValue, targetValue, eased);

        if (t >= 1f)
        {
            displayedValue = targetValue;
            isCounting = false;
        }

        SetText(Mathf.RoundToInt(displayedValue));
    }

    // Starts from the number on screen, so a change mid-count carries on
    // smoothly instead of jumping.
    private void BeginCount(int newTarget, float delay)
    {
        startValue = displayedValue;
        targetValue = newTarget;

        float difference = Mathf.Abs(targetValue - startValue);

        duration = Mathf.Clamp(
            difference / countSpeed,
            minCountDuration,
            Mathf.Max(minCountDuration, maxCountDuration)
        );

        // A negative start leaves the number still during the delay.
        elapsed = -delay;
        isCounting = true;
    }

    private void Snap(int value)
    {
        value = Mathf.Max(0, value);

        displayedValue = value;
        startValue = value;
        targetValue = value;
        isCounting = false;

        SetText(value);
    }

    private void SetText(int value)
    {
        if (populationText != null)
            populationText.text = Mathf.Max(0, value).ToString();
    }

    private int GetTotalPopulation()
    {
        if (boardManager == null)
            FindBoardManager();

        return boardManager != null
            ? Mathf.Max(0, boardManager.GetTotalPopulation())
            : 0;
    }

    private void FindBoardManager()
    {
        if (boardManager == null)
            boardManager = FindAnyObjectByType<BoardManager>();
    }
}
