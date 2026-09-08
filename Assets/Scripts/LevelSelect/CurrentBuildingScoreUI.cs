using TMPro;
using UnityEngine;

public class CurrentBuildingScoreUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        UpdateScore();
    }

    private void Update()
    {
        UpdateScore();
    }

    private void UpdateScore()
    {
        if (scoreText == null)
        {
            return;
        }

        if (BuildingScoreManager.Instance == null)
        {
            scoreText.text = "0";
            return;
        }

        scoreText.text =
            BuildingScoreManager.Instance.CurrentBuildingScore
            .ToString();
    }
}