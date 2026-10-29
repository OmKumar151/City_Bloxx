using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main UI References")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text floorText;
    public TMP_Text gameOverText;
    public TMP_Text finalScoreText;

    [Header("Life Indicators")]
    public Image[] lifeIcons;
    public Color lifeActiveColor = new Color(1f, 0.2f, 0.25f, 1f);
    public Color lifeLostColor = new Color(0.3f, 0.3f, 0.35f, 0.65f);

    [Header("Score Settings")]
    public int displayedScore;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate UIManager found.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (completePanel != null)
            completePanel.SetActive(false);

        if (gameOverText != null)
            gameOverText.enabled = false;

        GameManager manager = GameManager.Instance;

        if (manager != null)
        {
            UpdateScore(manager.score);
            UpdateLives(manager.lives);
            UpdateFloors(manager.floorsBuilt);
            UpdateCombo(manager.comboCount, manager.maxCombo, false);
        }
        else
        {
            UpdateScore(0);
            UpdateLives(3);
            UpdateFloors(0);
            UpdateCombo(0, 5, false);
        }
    }

    public void UpdateScore(int score)
    {
        displayedScore = Mathf.Max(0, score);

        if (scoreText != null)
        {
            scoreText.text =
                "Score\n" + displayedScore.ToString("N0");
        }

        if (finalScoreText != null &&
            completePanel != null &&
            completePanel.activeSelf)
        {
            finalScoreText.text =
                "Final Score\n" + displayedScore.ToString("N0");
        }
    }

    public void UpdateLives(int lives)
    {
        int safeLives = Mathf.Max(0, lives);

        if (lifeIcons != null)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] == null)
                    continue;

                lifeIcons[i].color =
                    i < safeLives ? lifeActiveColor : lifeLostColor;

                lifeIcons[i].enabled = true;
            }
        }

        if (livesText != null)
            livesText.text = "LIVES: " + safeLives;
    }

    public void UpdateFloors(int floors)
    {
        if (floorText != null)
        {
            floorText.text =
                "Floor\n" + Mathf.Max(0, floors);
        }
    }

    public void UpdateCombo(
        int comboCount,
        int maxCombo,
        bool wasPerfect)
    {
        int safeCombo = Mathf.Max(0, comboCount);
        int safeMaxCombo = Mathf.Max(1, maxCombo);
        int multiplier = safeCombo + 1;
        bool maxed = safeCombo >= safeMaxCombo;

        if (comboBarFill != null)
        {
            comboBarFill.fillAmount =
                Mathf.Clamp01((float)safeCombo / safeMaxCombo);

            comboBarFill.color =
                maxed ? comboMaxColor : comboNormalColor;
        }

        if (comboText != null)
        {
            comboText.text = wasPerfect && safeCombo > 0
                ? "PERFECT! x" + multiplier
                : "x" + multiplier;
        }

        if (comboBarContainer != null)
            comboBarContainer.SetActive(safeCombo > 0);

        if (comboStarIcon != null)
        {
            comboStarIcon.color =
                maxed ? comboMaxColor : comboNormalColor;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.enabled = true;
    }

    public void ShowBuildingComplete(int score)
    {
        UpdateScore(score);

        if (completePanel != null)
            completePanel.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "Final Score\n" + Mathf.Max(0, score).ToString("N0");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}