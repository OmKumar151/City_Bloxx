using TMPro;
using UnityEngine;

public class CurrentBuildingScoreUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Score Manager")]
    [SerializeField] private BuildingScoreManager scoreManager;

    private void Awake()
    {
        TryFindScoreManager();
    }

    private void OnEnable()
    {
        TryFindScoreManager();

        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged +=
                UpdateScore;
        }

        UpdateScore();
    }

    private void OnDisable()
    {
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -=
                UpdateScore;
        }
    }

    private void TryFindScoreManager()
    {
        if (scoreManager == null)
        {
            scoreManager =
                BuildingScoreManager.Instance;
        }
    }

    private void UpdateScore()
    {
        if (scoreText == null)
        {
            Debug.LogWarning(
                "CurrentBuildingScoreUI: " +
                "Score Text is not assigned."
            );

            return;
        }

        TryFindScoreManager();

        if (scoreManager == null)
        {
            scoreText.text = "0";
            return;
        }

        int currentScore =
            scoreManager.CurrentBuildingScore;

        int existingScore =
            scoreManager.ExistingBuildingScore;

        // No current building selected.
        if (currentScore <= 0)
        {
            scoreText.text = "0";
            return;
        }

        // Empty grid cell.
        if (existingScore <= 0)
        {
            scoreText.text =
                currentScore.ToString();

            return;
        }

        // Occupied grid cell.
        scoreText.text =
            currentScore +
            " / " +
            existingScore;
    }
}