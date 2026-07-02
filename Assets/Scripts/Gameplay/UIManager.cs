using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text floorText;
    public TMP_Text gameOverText;
    public TMP_Text finalScoreText;

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