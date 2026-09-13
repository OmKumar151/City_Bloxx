using System;
using UnityEngine;

public class BuildingScoreManager : MonoBehaviour
{
    public static BuildingScoreManager Instance { get; private set; }

    public int CurrentBuildingScore { get; private set; }

    public int ExistingBuildingScore { get; private set; }

    public event Action OnScoreChanged;

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

        NotifyScoreChanged();
    }

    public void ClearCurrentBuildingScore()
    {
        CurrentBuildingScore = 0;

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

        NotifyScoreChanged();
    }

    public void ClearExistingBuildingScore()
    {
        ExistingBuildingScore = 0;

        NotifyScoreChanged();
    }

    // =========================================================
    // BOTH
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

        NotifyScoreChanged();
    }

    public void ClearScores()
    {
        CurrentBuildingScore = 0;
        ExistingBuildingScore = 0;

        NotifyScoreChanged();
    }

    // =========================================================
    // BACKWARD COMPATIBILITY
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
        OnScoreChanged?.Invoke();
    }

    // =========================================================
    // TESTING
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