
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Lives UI")]
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private List<Image> lifeIcons = new List<Image>();
    [SerializeField] private Color lifeActiveColor = Color.white;
    [SerializeField] private Color lifeLostColor = Color.gray;

    [Header("Floor UI")]
    [SerializeField] private TMP_Text floorText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Building Complete UI")]
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TMP_Text completeFinalScoreText;

    private int displayedScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HideEndPanels();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateScore(GameManager.Instance.score);
            UpdateLives(GameManager.Instance.lives);
            UpdateFloors(
                GameManager.Instance.floorsBuilt,
                GameManager.Instance.floorsToComplete
            );
        }
    }

    public void UpdateScore(int score)
    {
        displayedScore = Mathf.Max(0, score);

        if (scoreText != null)
            scoreText.text = displayedScore.ToString("N0");
    }

    public void UpdateLives(int lives)
    {
        lives = Mathf.Max(0, lives);

        if (livesText != null)
            livesText.text = lives.ToString();

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] == null)
                continue;

            lifeIcons[i].color = i < lives
                ? lifeActiveColor
                : lifeLostColor;
        }
    }

    public void UpdateFloors(int floorsBuilt, int floorsToComplete)
    {
        if (floorText != null)
        {
            floorText.text =
                Mathf.Max(0, floorsBuilt) + " / " +
                Mathf.Max(1, floorsToComplete);
        }
    }

    // Kept for compatibility with any older scripts that call UpdateCombo.
    // Combo information is intentionally not displayed to the player.
    public void UpdateCombo(int comboCount, int maxCombo, bool wasPerfect)
    {
        // Intentionally empty.
    }

    public void HideEndPanels()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (completePanel != null)
            completePanel.SetActive(false);
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (completePanel != null)
            completePanel.SetActive(false);

        if (gameOverText != null)
            gameOverText.text = "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = Mathf.Max(0, finalScore).ToString("N0");
    }

    public void ShowBuildingComplete(int finalScore)
    {
        if (completePanel != null)
            completePanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (completeFinalScoreText != null)
        {
            completeFinalScoreText.text =
                Mathf.Max(0, finalScore).ToString("N0");
        }
        else if (finalScoreText != null)
        {
            finalScoreText.text =
                Mathf.Max(0, finalScore).ToString("N0");
        }
    }
}
