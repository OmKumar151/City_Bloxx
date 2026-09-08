using UnityEngine;

public class BuildingScoreManager : MonoBehaviour
{
    public static BuildingScoreManager Instance { get; private set; }

    public int CurrentBuildingScore { get; private set; }

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
    }

    public void SetBuildingScore(int score)
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
    }

    public void ClearBuildingScore()
    {
        CurrentBuildingScore = 0;

        Debug.Log(
            "Current Building Score cleared."
        );
    }
}