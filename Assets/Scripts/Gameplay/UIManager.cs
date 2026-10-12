using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Main UI References")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text floorText;
    public TMP_Text gameOverText;
    public TMP_Text finalScoreText;

    [Header("Life Indicators")]
    [Tooltip("Assign the three life images in order: Life1, Life2, Life3.")]
    public Image[] lifeIcons;

    public Color lifeActiveColor = new Color(1f, 0.2f, 0.25f, 1f);
    public Color lifeLostColor = new Color(0.3f, 0.3f, 0.35f, 0.65f);

    [Header("Score Settings")]
    public string scorePrefix = "SCORE";
    public int displayedScore = 0;

    [Header("Combo Bar")]
    public Image comboBarFill;
    public TMP_Text comboText;
    public GameObject comboBarContainer;
    public Image comboStarIcon;

    public Color comboNormalColor = Color.white;
    public Color comboMaxColor = new Color(1f, 0.85f, 0.2f);

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject completePanel;

    private int currentLives = 3;
    private int currentFloors = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "More than one UIManager exists in the scene."
            );
        }

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

    // ---------------- SCORE ----------------

    public void UpdateScore(int score)
    {
        displayedScore = Mathf.Max(0, score);

        if (scoreText != null)
        {
            scoreText.text =
                scorePrefix + ": " + displayedScore.ToString("N0");
        }

        if (finalScoreText != null &&
            completePanel != null &&
            completePanel.activeSelf)
        {
            finalScoreText.text =
                "Final Score: " + displayedScore.ToString("N0");
        }
    }

    // ---------------- LIVES ----------------

    public void UpdateLives(int lives)
    {
        currentLives = Mathf.Max(0, lives);

        if (lifeIcons != null && lifeIcons.Length > 0)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                Image iconImage = lifeIcons[i];

                if (iconImage == null)
                    continue;

                bool isActive = i < currentLives;

                iconImage.color = isActive
                    ? lifeActiveColor
                    : lifeLostColor;

                iconImage.enabled = true;
            }
        }

        if (livesText != null)
        {
            livesText.text = "LIVES: " + currentLives;
        }
    }

    // ---------------- FLOORS ----------------

    public void UpdateFloors(int floors)
    {
        currentFloors = Mathf.Max(0, floors);

        if (floorText != null)
        {
            floorText.text =
                "FLOORS: " + currentFloors;
        }
    }

    // ---------------- COMBO ----------------

    public void UpdateCombo(
        int comboCount,
        int maxCombo,
        bool wasPerfect)
    {
        int safeCombo = Mathf.Max(0, comboCount);
        int safeMaxCombo = Mathf.Max(1, maxCombo);

        int multiplier = safeCombo + 1;

        if (comboBarFill != null)
        {
            comboBarFill.fillAmount = Mathf.Clamp01(
                (float)safeCombo / safeMaxCombo
            );
        }

        if (comboText != null)
        {
            comboText.text = "x" + multiplier;
        }

        if (comboBarContainer != null)
        {
            comboBarContainer.SetActive(safeCombo > 0);
        }

        bool isMaxed = safeCombo >= safeMaxCombo;

        if (comboBarFill != null)
        {
            comboBarFill.color = isMaxed
                ? comboMaxColor
                : comboNormalColor;
        }

        if (comboStarIcon != null)
        {
            comboStarIcon.color = isMaxed
                ? comboMaxColor
                : comboNormalColor;
        }
    }

    // ---------------- GAME OVER ----------------

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
            gameOverText.enabled = true;
        }
    }

    // ---------------- BUILDING COMPLETE ----------------

    public void ShowBuildingComplete(int score)
    {
        UpdateScore(score);

        if (completePanel != null)
            completePanel.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "FINAL SCORE: " + displayedScore.ToString("N0");
        }
    }
}
