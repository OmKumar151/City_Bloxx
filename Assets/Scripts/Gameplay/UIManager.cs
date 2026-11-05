
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

    [Header("Game Over (Fail) UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [Tooltip("Score shown on the fail panel.")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverLevelSelectButton;

    [Header("Building Complete (Win) UI")]
    [SerializeField] private GameObject completePanel;
    [Tooltip("Score shown on the win panel.")]
    [SerializeField] private TMP_Text completeFinalScoreText;
    [SerializeField] private Button completeRestartButton;
    [Tooltip("Adds the score to the city's population and returns to LevelSelect.")]
    [SerializeField] private Button completeNextButton;

    [Header("Score Format")]
    [SerializeField] private string scorePrefix = "Score: ";

    private int displayedScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        WireEndPanelButtons();
        HideEndPanels();
    }

    private void WireEndPanelButtons()
    {
        AddClick(gameOverRestartButton, OnRestartClicked);
        AddClick(gameOverLevelSelectButton, OnLevelSelectClicked);
        AddClick(completeRestartButton, OnRestartClicked);
        AddClick(completeNextButton, OnNextClicked);
    }

    private static void AddClick(
        Button button,
        UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void OnRestartClicked()
    {
        PlayButtonSound();

        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void OnNextClicked()
    {
        PlayButtonSound();

        if (GameManager.Instance != null)
            GameManager.Instance.BankScoreAndReturnToLevelSelect();
    }

    private void OnLevelSelectClicked()
    {
        PlayButtonSound();

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToLevelSelect();
    }

    private static void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMajorButton();
    }

    private string FormatScore(int score)
    {
        return scorePrefix + Mathf.Max(0, score).ToString("N0");
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
            finalScoreText.text = FormatScore(finalScore);

        SelectFirstButton(gameOverRestartButton);
    }

    public void ShowBuildingComplete(int finalScore)
    {
        if (completePanel != null)
            completePanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (completeFinalScoreText != null)
            completeFinalScoreText.text = FormatScore(finalScore);

        SelectFirstButton(completeNextButton);
    }

    // Lets keyboard / controller players press a button straight away.
    private static void SelectFirstButton(Button button)
    {
        if (button != null &&
            UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current
                .SetSelectedGameObject(button.gameObject);
        }
    }
}
