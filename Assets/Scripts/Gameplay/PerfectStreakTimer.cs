
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerfectStreakTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Min(0.1f)]
    [SerializeField] private float streakDuration = 2.5f;

    [Header("UI References")]
    [SerializeField] private GameObject timerContainer;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TMP_Text timerLabel;

    public bool IsStreakActive { get; private set; }
    public bool IsCountingDown { get; private set; }
    public float TimeRemaining { get; private set; }

    private void Awake()
    {
        streakDuration = Mathf.Max(0.1f, streakDuration);
        TimeRemaining = streakDuration;

        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = 1f;
            timerSlider.value = 1f;
        }

        SetVisible(false);
        UpdateVisuals();
    }

    private void Update()
    {
        if (!IsStreakActive || !IsCountingDown)
            return;

        TimeRemaining -= Time.deltaTime;
        TimeRemaining = Mathf.Max(0f, TimeRemaining);

        UpdateVisuals();

        if (TimeRemaining <= 0f)
        {
            // Stop first so the GameManager can safely reset the streak.
            StopTimer();

            if (GameManager.Instance != null)
                GameManager.Instance.EndPerfectStreak();
        }
    }

    public void StartOrResetTimer()
    {
        streakDuration = Mathf.Max(0.1f, streakDuration);

        IsStreakActive = true;

        // The timer becomes visible immediately, but does not count down
        // until CraneController starts the next swinging block.
        IsCountingDown = false;
        TimeRemaining = streakDuration;

        SetVisible(true);
        UpdateVisuals();
    }

    public void ResumeTimer()
    {
        if (!IsStreakActive)
            return;

        IsCountingDown = true;
        SetVisible(true);
    }

    public void PauseTimer()
    {
        if (IsStreakActive)
            IsCountingDown = false;
    }

    public void StopTimer()
    {
        IsStreakActive = false;
        IsCountingDown = false;
        TimeRemaining = 0f;

        UpdateVisuals();
        SetVisible(false);
    }

    private void UpdateVisuals()
    {
        float normalizedTime = streakDuration > 0f
            ? Mathf.Clamp01(TimeRemaining / streakDuration)
            : 0f;

        if (timerSlider != null)
            timerSlider.value = normalizedTime;

        if (timerSlider != null && timerSlider.fillRect != null)
        {
            Image fillImage =
                timerSlider.fillRect.GetComponent<Image>();

            if (fillImage != null)
            {
                if (normalizedTime > 0.6f)
                    fillImage.color = new Color(0.2f, 0.85f, 0.3f);
                else if (normalizedTime > 0.3f)
                    fillImage.color = new Color(1f, 0.75f, 0.15f);
                else
                    fillImage.color = new Color(1f, 0.25f, 0.2f);
            }
        }

        if (timerLabel != null)
        {
            timerLabel.text = IsStreakActive
                ? TimeRemaining.ToString("F1") + "s"
                : string.Empty;
        }
    }

    private void SetVisible(bool visible)
    {
        if (timerContainer != null)
            timerContainer.SetActive(visible);
    }
}
