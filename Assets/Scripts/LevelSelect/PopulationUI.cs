using TMPro;
using UnityEngine;

public class PopulationUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text populationText;

    [Header("Board")]
    [SerializeField] private BoardManager boardManager;

    private void Start()
    {
        FindBoardManager();
        UpdatePopulation();
    }

    private void Update()
    {
        UpdatePopulation();
    }

    private void FindBoardManager()
    {
        if (boardManager != null)
        {
            return;
        }

        boardManager =
            FindFirstObjectByType<BoardManager>();
    }

    private void UpdatePopulation()
    {
        if (populationText == null)
        {
            return;
        }

        if (boardManager == null)
        {
            populationText.text = "0";
            return;
        }

        populationText.text =
            boardManager.GetTotalPopulation().ToString();
    }
}