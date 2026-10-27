
using System;
using UnityEngine;

public class PopulationProgressionManager : MonoBehaviour
{
    public static PopulationProgressionManager Instance { get; private set; }

    public enum RewardType
    {
        BlueBuilding,
        RedBuilding,
        GreenBuilding,
        BlueStar,
        YellowBuilding,
        RedStar,
        GreenStar,
        YellowStar
    }

    [Header("Population Checkpoints")]
    [SerializeField]
    private int[] populationCheckpoints =
    {
        0, 500, 1000, 1500, 2000, 2500, 3000, 3500
    };

    [Header("Reward Unlock States")]
    [SerializeField]
    private bool[] checkpointUnlocked =
    {
        true, false, false, false, false, false, false, false
    };

    private bool suppressZeroPopulationCheckpoints;

    public event Action<RewardType> OnRewardUnlocked;
    public event Action OnProgressionReset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureArraySizes();
        checkpointUnlocked[(int)RewardType.BlueBuilding] = true;
    }

    private void Update()
    {
        CheckProgression();
    }

    private void EnsureArraySizes()
    {
        int rewardCount = Enum.GetValues(typeof(RewardType)).Length;

        if (checkpointUnlocked == null ||
            checkpointUnlocked.Length != rewardCount)
        {
            bool[] updatedStates = new bool[rewardCount];

            if (checkpointUnlocked != null)
            {
                Array.Copy(
                    checkpointUnlocked,
                    updatedStates,
                    Mathf.Min(checkpointUnlocked.Length, rewardCount)
                );
            }

            checkpointUnlocked = updatedStates;
        }

        if (populationCheckpoints == null ||
            populationCheckpoints.Length != rewardCount)
        {
            int[] defaults =
            {
                0, 500, 1000, 1500, 2000, 2500, 3000, 3500
            };

            int[] updatedCheckpoints = new int[rewardCount];

            for (int i = 0; i < rewardCount; i++)
            {
                updatedCheckpoints[i] =
                    populationCheckpoints != null &&
                    i < populationCheckpoints.Length
                        ? populationCheckpoints[i]
                        : defaults[i];
            }

            populationCheckpoints = updatedCheckpoints;
        }
    }

    public int GetCurrentPopulation()
    {
        if (PopulationManager.Instance == null)
            return 0;

        return PopulationManager.Instance.TotalPopulation;
    }

    public bool IsRewardUnlocked(RewardType reward)
    {
        if (reward == RewardType.BlueBuilding)
            return true;

        int index = (int)reward;

        if (checkpointUnlocked == null ||
            index < 0 ||
            index >= checkpointUnlocked.Length)
        {
            return false;
        }

        return checkpointUnlocked[index];
    }

    public void CheckProgression()
    {
        int population = GetCurrentPopulation();

        if (suppressZeroPopulationCheckpoints && population <= 0)
            return;

        if (population > 0)
            suppressZeroPopulationCheckpoints = false;

        for (int i = 0; i < populationCheckpoints.Length; i++)
        {
            RewardType reward = (RewardType)i;

            if (reward == RewardType.BlueBuilding)
            {
                checkpointUnlocked[i] = true;
                continue;
            }

            if (checkpointUnlocked[i])
                continue;

            if (population >= populationCheckpoints[i])
            {
                checkpointUnlocked[i] = true;
                OnRewardUnlocked?.Invoke(reward);

                Debug.Log(
                    "PopulationProgressionManager: Unlocked " +
                    reward + " at population " + population
                );
            }
        }
    }

    public void ResetProgression()
    {
        EnsureArraySizes();

        for (int i = 0; i < checkpointUnlocked.Length; i++)
            checkpointUnlocked[i] = false;

        checkpointUnlocked[(int)RewardType.BlueBuilding] = true;

        suppressZeroPopulationCheckpoints = true;

        OnProgressionReset?.Invoke();

        Debug.Log(
            "PopulationProgressionManager: Progression reset. " +
            "Only the Blue building remains unlocked."
        );
    }

    public int GetNextCheckpointPopulation()
    {
        for (int i = 0; i < populationCheckpoints.Length; i++)
        {
            RewardType reward = (RewardType)i;

            if (reward == RewardType.BlueBuilding)
                continue;

            if (!IsRewardUnlocked(reward))
                return populationCheckpoints[i];
        }

        return populationCheckpoints.Length > 0
            ? populationCheckpoints[populationCheckpoints.Length - 1]
            : 0;
    }

    public int GetPreviousCheckpointPopulation()
    {
        int population = GetCurrentPopulation();
        int previousCheckpoint = 0;

        for (int i = 0; i < populationCheckpoints.Length; i++)
        {
            RewardType reward = (RewardType)i;

            if (reward == RewardType.BlueBuilding)
                continue;

            if (IsRewardUnlocked(reward) &&
                populationCheckpoints[i] <= population)
            {
                previousCheckpoint = populationCheckpoints[i];
            }
            else if (populationCheckpoints[i] > population)
            {
                break;
            }
        }

        return previousCheckpoint;
    }
}
