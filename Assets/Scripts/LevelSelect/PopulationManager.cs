using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance { get; private set; }

    public int TotalPopulation { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        TotalPopulation = 0;
    }

    public void AddPopulation(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning(
                "PopulationManager: Cannot add negative population."
            );
            return;
        }

        TotalPopulation += amount;
    }

    public void RemovePopulation(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning(
                "PopulationManager: Cannot remove negative population."
            );
            return;
        }

        TotalPopulation -= amount;

        if (TotalPopulation < 0)
            TotalPopulation = 0;
    }

    public void SetPopulation(int amount)
    {
        if (amount < 0)
            amount = 0;

        TotalPopulation = amount;
    }

    public void ResetPopulation()
    {
        TotalPopulation = 0;
    }
}
