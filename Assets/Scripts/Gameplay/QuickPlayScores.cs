
using UnityEngine;

/// <summary>
/// Highest score reached in Quick Play, saved between sessions.
/// </summary>
public static class QuickPlayScores
{
    private const string BestScoreKey = "QuickPlay_BestScore";

    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

    /// <summary>
    /// Saves the score if it beats the best. Returns true for a new best;
    /// best is the best score after this run.
    /// </summary>
    public static bool Record(int score, out int best)
    {
        int previousBest = BestScore;

        if (score > previousBest)
        {
            SaveStore.SetInt(BestScoreKey, score);
            SaveStore.Save();

            best = score;
            return true;
        }

        best = previousBest;
        return false;
    }
}
