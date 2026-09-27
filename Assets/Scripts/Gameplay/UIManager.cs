using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text floorText;
    public TMP_Text gameOverText;
    public TMP_Text finalScoreText;

    [Header("Combo Bar (top bar, like the star meter in Tower Bloxx)")]
    [Tooltip("An Image with Image Type = Filled, Fill Method = Horizontal. Its fillAmount is driven by the current combo.")]
    public Image comboBarFill;

    [Tooltip("Shows the current multiplier, e.g. 'x3'.")]
    public TMP_Text comboText;

    [Tooltip("Optional. The whole bar group — hidden until the player has at least one perfect drop, shown again on a miss.")]
    public GameObject comboBarContainer;

    [Tooltip("Optional. Tinted a highlight color when the combo hits max (matches the gold star in the reference screenshot).")]
    public Image comboStarIcon;

    public Color comboNormalColor = Color.white;
    public Color comboMaxColor = new Color(1f, 0.85f, 0.2f); // gold

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject completePanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateScore(0);
        UpdateLives(3);
        UpdateFloors(0);
        UpdateCombo(0, 5, false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (completePanel != null)
            completePanel.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = "Lives: " + lives;
    }

    public void UpdateFloors(int floors)
    {
        if (floorText != null)
            floorText.text = "Floors: " + floors;
    }

    // comboCount: current combo (0 = no combo yet). maxCombo: the cap (e.g. 5).
    // wasPerfect: true if the drop that triggered this was a "perfect" one —
    // reserved in case you want a flash/animation on a perfect hit later.
    public void UpdateCombo(int comboCount, int maxCombo, bool wasPerfect)
    {
        int multiplier = comboCount + 1;

        if (comboBarFill != null)
        {
            comboBarFill.fillAmount =
                maxCombo > 0 ? (float)comboCount / maxCombo : 0f;
        }

        if (comboText != null)
        {
            comboText.text = "x" + multiplier;
        }

        if (comboBarContainer != null)
        {
            // Matches the real game: the bar/multiplier only shows once
            // you've actually built a combo, and disappears again on a miss.
            comboBarContainer.SetActive(comboCount > 0);
        }

        bool isMaxed = comboCount >= maxCombo;

        if (comboBarFill != null)
        {
            comboBarFill.color = isMaxed ? comboMaxColor : comboNormalColor;
        }

        if (comboStarIcon != null)
        {
            comboStarIcon.color = isMaxed ? comboMaxColor : comboNormalColor;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowBuildingComplete(int score)
    {
        if (completePanel != null)
            completePanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score;
    }
}