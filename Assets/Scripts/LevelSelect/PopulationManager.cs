using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance { get; private set; }

    public int TotalPopulation { get; private set; }

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

        TotalPopulation = 0;
    }

    public void AddPopulation(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning(
                "PopulationManager: " +
                "Cannot add negative population."
            );

            return;
        }

        TotalPopulation += amount;

        Debug.Log(
            "Population added: " +
            amount +
            " | Total Population: " +
            TotalPopulation
        );
    }

    public void RemovePopulation(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning(
                "PopulationManager: " +
                "Cannot remove negative population."
            );

            return;
        }

        TotalPopulation -= amount;

        if (TotalPopulation < 0)
        {
            TotalPopulation = 0;
        }

        Debug.Log(
            "Population removed: " +
            amount +
            " | Total Population: " +
            TotalPopulation
        );
    }

    public void SetPopulation(int amount)
    {
        if (amount < 0)
        {
            amount = 0;
        }

        TotalPopulation = amount;

        Debug.Log(
            "Population set to: " +
            TotalPopulation
        );
    }

    public void ResetPopulation()
    {
        TotalPopulation = 0;

        Debug.Log(
            "Population reset."
        );
    }
}