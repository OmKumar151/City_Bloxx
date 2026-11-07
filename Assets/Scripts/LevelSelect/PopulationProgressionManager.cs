
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

    // One entry per reward, in RewardType order. The hidden name is what
    // the Inspector shows as each element's label.
    [Serializable]
    public class Checkpoint
    {
        [HideInInspector] public string name;

        [Min(0)]
        [Tooltip("City population needed to unlock this reward.")]
        public int population;

        [Tooltip("Runtime state. Tick in Play mode to test an unlock.")]
        public bool unlocked;
    }

    [Header("Population Checkpoints")]
    [Tooltip("Population needed for each reward. Keep them in ascending " +
             "order. Blue Building is always unlocked.")]
    [SerializeField]
    private Checkpoint[] checkpoints = CreateDefaultCheckpoints();

    [Header("Building Population")]
    [Tooltip("Not used for placed buildings any more: a placed building " +
             "adds the score earned building it. Kept only as a fallback.")]
    [Min(0)] [SerializeField] private int bluePopulation = 100;
    [Min(0)] [SerializeField] private int redPopulation = 250;
    [Min(0)] [SerializeField] private int greenPopulation = 500;
    [Min(0)] [SerializeField] private int yellowPopulation = 750;

    private static readonly int[] DefaultPopulations =
    {
        0, 810, 2370, 3450, 4740, 6270, 7980, 9600
    };

    private bool suppressZeroPopulationCheckpoints;

    public event Action<RewardType> OnRewardUnlocked;
    public event Action OnProgressionReset;

    private static int RewardCount => Enum.GetValues(typeof(RewardType)).Length;

    private static Checkpoint[] CreateDefaultCheckpoints()
    {
        Checkpoint[] defaults = new Checkpoint[RewardCount];

        for (int i = 0; i < defaults.Length; i++)
        {
            defaults[i] = new Checkpoint
            {
                name = ((RewardType)i).ToString(),
                population = i < DefaultPopulations.Length
                    ? DefaultPopulations[i]
                    : 0,
                unlocked = i == (int)RewardType.BlueBuilding
            };
        }

        return defaults;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureCheckpoints();
    }

    private void OnValidate()
    {
        EnsureCheckpoints();

        for (int i = 1; i < checkpoints.Length; i++)
        {
            if (checkpoints[i].population < checkpoints[i - 1].population)
            {
                Debug.LogWarning(
                    "PopulationProgressionManager: " + checkpoints[i].name +
                    " (" + checkpoints[i].population + ") is lower than " +
                    checkpoints[i - 1].name + " (" +
                    checkpoints[i - 1].population +
                    "). Checkpoints should be in ascending order.",
                    this
                );
            }
        }
    }

    private void Update()
    {
        CheckProgression();
    }

    // Keeps exactly one entry per reward, labelled, with Blue unlocked.
    private void EnsureCheckpoints()
    {
        int rewardCount = RewardCount;

        if (checkpoints == null || checkpoints.Length != rewardCount)
        {
            Checkpoint[] defaults = CreateDefaultCheckpoints();

            if (checkpoints != null)
            {
                for (int i = 0; i < Mathf.Min(checkpoints.Length, rewardCount); i++)
                {
                    if (checkpoints[i] != null)
                        defaults[i] = checkpoints[i];
                }
            }

            checkpoints = defaults;
        }

        for (int i = 0; i < rewardCount; i++)
        {
            if (checkpoints[i] == null)
                checkpoints[i] = new Checkpoint();

            checkpoints[i].name = ((RewardType)i).ToString();
        }

        Checkpoint blue = checkpoints[(int)RewardType.BlueBuilding];
        blue.unlocked = true;
        blue.population = 0;
    }

    public int GetCurrentPopulation()
    {
        if (PopulationManager.Instance == null)
            return 0;

        return PopulationManager.Instance.TotalPopulation;
    }

    /// <summary>Population a building adds to the city when placed.</summary>
    public int GetBuildingPopulation(GridCell.BuildingType buildingType)
    {
        switch (buildingType)
        {
            case GridCell.BuildingType.Blue: return bluePopulation;
            case GridCell.BuildingType.Red: return redPopulation;
            case GridCell.BuildingType.Green: return greenPopulation;
            case GridCell.BuildingType.Yellow: return yellowPopulation;
            default: return 0;
        }
    }

    /// <summary>
    /// Population needed to unlock the given reward in the active city.
    /// Uses the city's own thresholds (CityManager) when it has them,
    /// otherwise the checkpoints on this component.
    /// </summary>
    public int GetCheckpointPopulation(RewardType reward)
    {
        if (CityManager.Instance != null &&
            CityManager.Instance.TryGetRewardPopulation(reward, out int cityPopulation))
        {
            return reward == RewardType.BlueBuilding ? 0 : cityPopulation;
        }

        int index = (int)reward;

        return checkpoints != null && index >= 0 && index < checkpoints.Length
            ? checkpoints[index].population
            : 0;
    }

    private int GetRequiredPopulation(int index)
    {
        return GetCheckpointPopulation((RewardType)index);
    }

    public bool IsRewardUnlocked(RewardType reward)
    {
        if (reward == RewardType.BlueBuilding)
            return true;

        int index = (int)reward;

        if (checkpoints == null ||
            index < 0 ||
            index >= checkpoints.Length)
        {
            return false;
        }

        return checkpoints[index].unlocked;
    }

    public void CheckProgression()
    {
        int population = GetCurrentPopulation();

        if (suppressZeroPopulationCheckpoints && population <= 0)
            return;

        if (population > 0)
            suppressZeroPopulationCheckpoints = false;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            RewardType reward = (RewardType)i;
            Checkpoint checkpoint = checkpoints[i];

            if (reward == RewardType.BlueBuilding)
            {
                checkpoint.unlocked = true;
                continue;
            }

            if (checkpoint.unlocked)
                continue;

            if (population >= GetRequiredPopulation(i))
            {
                checkpoint.unlocked = true;
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
        EnsureCheckpoints();

        foreach (Checkpoint checkpoint in checkpoints)
            checkpoint.unlocked = false;

        checkpoints[(int)RewardType.BlueBuilding].unlocked = true;

        suppressZeroPopulationCheckpoints = true;

        OnProgressionReset?.Invoke();

        Debug.Log(
            "PopulationProgressionManager: Progression reset. " +
            "Only the Blue building remains unlocked."
        );
    }

    public int GetNextCheckpointPopulation()
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            RewardType reward = (RewardType)i;

            if (reward == RewardType.BlueBuilding)
                continue;

            if (!IsRewardUnlocked(reward))
                return GetRequiredPopulation(i);
        }

        return checkpoints.Length > 0
            ? GetRequiredPopulation(checkpoints.Length - 1)
            : 0;
    }

    public int GetPreviousCheckpointPopulation()
    {
        int population = GetCurrentPopulation();
        int previousCheckpoint = 0;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            RewardType reward = (RewardType)i;

            if (reward == RewardType.BlueBuilding)
                continue;

            if (IsRewardUnlocked(reward) &&
                GetRequiredPopulation(i) <= population)
            {
                previousCheckpoint = GetRequiredPopulation(i);
            }
            else if (GetRequiredPopulation(i) > population)
            {
                break;
            }
        }

        return previousCheckpoint;
    }
}
