using System;
using UnityEngine;

public class BuildingScoreManager : MonoBehaviour
{
    public static BuildingScoreManager Instance { get; private set; }

    // =========================================================
    // SCORES
    // =========================================================

    // Score of the building the player currently has selected.
    public int CurrentBuildingScore { get; private set; }

    // Score of the building already occupying the selected
    // grid cell.
    public int ExistingBuildingScore { get; private set; }

    public event Action OnScoreChanged;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        CurrentBuildingScore = 0;
        ExistingBuildingScore = 0;
    }

    // =========================================================
    // CURRENT BUILDING
    // =========================================================

    public void SetCurrentBuildingScore(int score)
    {
        if (score < 0)
        {
            score = 0;
        }

        CurrentBuildingScore = score;

        Debug.Log(
            "Current Building Score set to: " +
            CurrentBuildingScore
        );

        NotifyScoreChanged();
    }

    public void ClearCurrentBuildingScore()
    {
        CurrentBuildingScore = 0;

        Debug.Log(
            "Current Building Score cleared."
        );

        NotifyScoreChanged();
    }

    // =========================================================
    // EXISTING BUILDING
    // =========================================================

    public void SetExistingBuildingScore(int score)
    {
        if (score < 0)
        {
            score = 0;
        }

        ExistingBuildingScore = score;

        Debug.Log(
            "Existing Building Score set to: " +
            ExistingBuildingScore
        );

        NotifyScoreChanged();
    }

    public void ClearExistingBuildingScore()
    {
        ExistingBuildingScore = 0;

        Debug.Log(
            "Existing Building Score cleared."
        );

        NotifyScoreChanged();
    }

    // =========================================================
    // SET BOTH
    // =========================================================

    public void SetScores(
        int currentScore,
        int existingScore)
    {
        if (currentScore < 0)
        {
            currentScore = 0;
        }

        if (existingScore < 0)
        {
            existingScore = 0;
        }

        CurrentBuildingScore = currentScore;
        ExistingBuildingScore = existingScore;

        Debug.Log(
            "Building Scores updated. " +
            "Current: " +
            CurrentBuildingScore +
            " | Existing: " +
            ExistingBuildingScore
        );

        NotifyScoreChanged();
    }

    public void ClearScores()
    {
        CurrentBuildingScore = 0;
        ExistingBuildingScore = 0;

        Debug.Log(
            "Building Scores cleared."
        );

        NotifyScoreChanged();
    }

    // =========================================================
    // BACKWARD COMPATIBILITY
    // =========================================================
    //
    // Your existing BoardManager already calls these methods.
    // Keep them so BoardManager does not need to be changed.
    //
    // SetBuildingScore() now means:
    //     Set the EXISTING/selected grid-cell score.
    //
    // ClearBuildingScore() now means:
    //     Clear the EXISTING/selected grid-cell score.
    //
    // =========================================================

    public void SetBuildingScore(int score)
    {
        SetExistingBuildingScore(score);
    }

    public void ClearBuildingScore()
    {
        ClearExistingBuildingScore();
    }

    // =========================================================
    // EVENT
    // =========================================================

    private void NotifyScoreChanged()
    {
        if (OnScoreChanged != null)
        {
            OnScoreChanged.Invoke();
        }
    }

    // =========================================================
    // TESTS
    // =========================================================

    [ContextMenu("Test Current Score 250")]
    private void TestCurrentScore250()
    {
        SetCurrentBuildingScore(250);
    }

    [ContextMenu("Test Existing Score 100")]
    private void TestExistingScore100()
    {
        SetExistingBuildingScore(100);
    }

    [ContextMenu("Test 250 / 100")]
    private void Test250Over100()
    {
        SetScores(250, 100);
    }

    [ContextMenu("Clear Scores")]
    private void TestClearScores()
    {
        ClearScores();
    }
}