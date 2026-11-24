using UnityEngine;
using TMPro;

/// <summary>
/// "PERFECT!" text that pops in, rises and fades out after a perfect
/// landing. From the second perfect in a row it also shows the score
/// multiplier (e.g. "x1.25"), and the perfect roof shows its bonus.
///
/// Lives on the PerfectPopup text object in the Gameplay1 Canvas; position
/// and font are set there. UIManager.ShowPerfect() plays it.
/// </summary>
public class PerfectPopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    [Header("Text")]
    public string perfectText = "PERFECT!";
    public string roofText = "PERFECT ROOF!";
    [Tooltip("Size of the second line (multiplier / roof bonus).")]
    [Range(0.3f, 1f)] public float secondLineSize = 0.7f;

    [Header("Colours")]
    [Tooltip("Colour for the first perfect.")]
    public Color firstPerfectColor = new Color(1f, 0.86f, 0.3f, 1f);
    [Tooltip("Colour once the streak reaches the max combo.")]
    public Color maxStreakColor = new Color(1f, 0.45f, 0.25f, 1f);

    [Header("Animation")]
    [Min(0.1f)] public float duration = 1.1f;
    [Tooltip("How far the text rises while it fades (canvas units).")]
    public float rise = 90f;
    [Tooltip("Scale at the peak of the pop-in.")]
    [Min(1f)] public float popScale = 1.3f;

    private RectTransform rect;
    private Vector2 restPosition;
    private float age = -1f;

    private void Awake()
    {
        if (label == null)
            label = GetComponent<TMP_Text>();

        rect = transform as RectTransform;
        restPosition = rect != null ? rect.anchoredPosition : Vector2.zero;

        if (label != null)
            label.alpha = 0f;
    }

    /// <param name="streak">Perfects in a row, including this one.</param>
    /// <param name="maxStreak">Streak at which the colour stops changing.</param>
    /// <param name="multiplier">Score multiplier this landing got.</param>
    /// <param name="roofBonus">Perfect roof bonus, or 0 for a normal floor.</param>
    public void Show(int streak, int maxStreak, float multiplier, int roofBonus)
    {
        if (label == null)
            return;

        string sizeTag = "<size=" + Mathf.RoundToInt(secondLineSize * 100f) + "%>";

        if (roofBonus > 0)
            label.text = roofText + "\n" + sizeTag + "+" + roofBonus.ToString("N0") + "</size>";
        else if (streak >= 2 && multiplier > 1f)
            label.text = perfectText + "\n" + sizeTag + "x" + multiplier.ToString("0.##") + "</size>";
        else
            label.text = perfectText;

        float t = maxStreak > 1 ? Mathf.Clamp01((streak - 1f) / (maxStreak - 1f)) : 1f;
        label.color = Color.Lerp(firstPerfectColor, maxStreakColor, roofBonus > 0 ? 1f : t);

        age = 0f;
        Apply(0f);
    }

    private void Update()
    {
        if (age < 0f)
            return;

        // Unscaled so the roof popup finishes even after the win panel
        // pauses the game.
        age += Time.unscaledDeltaTime;
        float t = age / duration;

        if (t >= 1f)
        {
            age = -1f;
            Apply(1f);
            label.alpha = 0f;
            return;
        }

        Apply(t);
    }

    private void Apply(float t)
    {
        // Pop: overshoot to popScale in the first 15%, settle to 1 by 30%.
        float scale = t < 0.15f
            ? Mathf.Lerp(0.5f, popScale, t / 0.15f)
            : Mathf.Lerp(popScale, 1f, Mathf.Clamp01((t - 0.15f) / 0.15f));

        transform.localScale = Vector3.one * scale;

        if (rect != null)
            rect.anchoredPosition = restPosition + Vector2.up * rise * (1f - (1f - t) * (1f - t));

        // Hold fully visible, fade out over the last 40%.
        label.alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
    }
}
