using UnityEngine;
using UnityEngine.UI;

public class PopulationProgressBarUI : MonoBehaviour
{
    [Header("Progress Bar")]
    [SerializeField] private Image fillImage;

    [Header("Population Requirement")]
    [SerializeField] private int requiredPopulation = 5000;

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 2f;

    [Header("Board")]
    [SerializeField] private BoardManager boardManager;

    private float targetFillAmount = 0f;

    private void Start()
    {
        FindBoardManager();
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

    private void FindBoardManager()
    {
        if (boardManager != null)
            return;

        boardManager =
            FindFirstObjectByType<BoardManager>();

        if (boardManager == null)
        {
            Debug.LogWarning(
                "PopulationProgressBarUI: " +
                "Could not find a BoardManager in the scene."
            );
        }
    }

    private void UpdateTargetFill()
    {
        if (boardManager == null)
        {
            targetFillAmount = 0f;
            return;
        }

        if (requiredPopulation <= 0)
        {
            targetFillAmount = 1f;
            return;
        }

        int currentPopulation =
            boardManager.GetTotalPopulation();

        targetFillAmount =
            (float)currentPopulation /
            requiredPopulation;

        targetFillAmount =
            Mathf.Clamp01(targetFillAmount);
    }

    private void SmoothFill()
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount =
            Mathf.MoveTowards(
                fillImage.fillAmount,
                targetFillAmount,
                fillSpeed * Time.deltaTime
            );
    }

    public int GetRequiredPopulation()
    {
        return requiredPopulation;
    }

    public bool HasReachedPopulationRequirement()
    {
        if (boardManager == null)
            return false;

        return boardManager.GetTotalPopulation() >=
               requiredPopulation;
    }

    public float GetProgress()
    {
        if (requiredPopulation <= 0)
            return 1f;

        if (boardManager == null)
            return 0f;

        float progress =
            (float)boardManager.GetTotalPopulation() /
            requiredPopulation;

        return Mathf.Clamp01(progress);
    }
}