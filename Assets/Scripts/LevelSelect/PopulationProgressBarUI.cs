using UnityEngine;
using UnityEngine.UI;

public class PopulationProgressBarUI : MonoBehaviour
{
    [Header("Progress Bar")]
    [SerializeField] private Image fillImage;

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 1.5f;

    private float targetFillAmount = 0f;

    private void Start()
    {
        UpdateTargetFill();

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        UpdateTargetFill();
        SmoothFill();
    }

    private void UpdateTargetFill()
    {
        if (PopulationProgressionManager.Instance == null)
        {
            targetFillAmount = 0f;
            return;
        }

        int currentPopulation =
            PopulationProgressionManager.Instance
                .GetCurrentPopulation();

        int nextCheckpoint =
            PopulationProgressionManager.Instance
                .GetNextCheckpointPopulation();

        if (nextCheckpoint < 0)
        {
            targetFillAmount = 1f;
            return;
        }

        int previousCheckpoint =
            PopulationProgressionManager.Instance
                .GetPreviousCheckpointPopulation();

        int checkpointRange =
            nextCheckpoint -
            previousCheckpoint;

        if (checkpointRange <= 0)
        {
            targetFillAmount = 0f;
            return;
        }

        int populationIntoCheckpoint =
            currentPopulation -
            previousCheckpoint;

        targetFillAmount =
            (float)populationIntoCheckpoint /
            checkpointRange;

        targetFillAmount =
            Mathf.Clamp01(targetFillAmount);
    }

    private void SmoothFill()
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount =
            Mathf.MoveTowards(
                fillImage.fillAmount,
                targetFillAmount,
                fillSpeed * Time.deltaTime
            );
    }

    public float GetProgress()
    {
        return targetFillAmount;
    }
}