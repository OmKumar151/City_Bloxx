using TMPro;
using UnityEngine;

public class PopulationUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text populationText;

    private void Start()
    {
        UpdatePopulation();
    }

    private void Update()
    {
        UpdatePopulation();
    }

    private void UpdatePopulation()
    {
        if (populationText == null)
        {
            return;
        }

        if (PopulationManager.Instance == null)
        {
            populationText.text = "0";
            return;
        }

        populationText.text =
            PopulationManager.Instance.TotalPopulation.ToString();
    }
}