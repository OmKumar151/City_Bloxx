using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarUI : MonoBehaviour
{
    [Header("Building")]
    public Image buildingIcon;

    [Header("Population")]
    public TMP_Text populationText;

    [Header("Counter")]
    public TMP_Text counterText;

    [Header("Total")]
    public TMP_Text totalPopulationText;

    [Header("Progress")]
    public Image progressBar;

    public void UpdateTopBar(LevelData level, int currentPopulation, int totalPopulation)
    {
        buildingIcon.sprite = level.buildingIcon;

        populationText.text =
            currentPopulation + "/" + level.targetPopulation;

        counterText.text =
            currentPopulation.ToString("D6");

        totalPopulationText.text =
            totalPopulation.ToString();

        progressBar.fillAmount =
            (float)currentPopulation / level.targetPopulation;
    }
}