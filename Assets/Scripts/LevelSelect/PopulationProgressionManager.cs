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

    [Serializable]
    public class ProgressionCheckpoint
    {
        [Min(0)]
        public int requiredPopulation;

        public RewardType reward;
    }

    [Header("Population Progression")]
    [SerializeField]
    private ProgressionCheckpoint[] checkpoints =
    {
        new ProgressionCheckpoint
        {
            requiredPopulation = 0,
            reward = RewardType.BlueBuilding
        },

        new ProgressionCheckpoint
        {
            requiredPopulation = 500,
            reward = RewardType.RedBuilding
        },

        new ProgressionCheckpoint
        {
            requiredPopulation = 1000,
            reward = RewardType.GreenBuilding
        },

        new ProgressionCheckpoint
        {
            requiredPopulation = 1500,
            reward = RewardType.BlueStar
        },

        new ProgressionCheckpoint
        {
            requiredPopulation = 2000,
            reward = RewardType.YellowBuilding
        },

        new ProgressionCheckpoint
        {
            requiredPopulation = 2500,
            reward = RewardType.RedStar
        },

        new ProgressionCheckpoint
        {
            requiredPopulation = 3000,
            reward = RewardType.GreenStar
        },

        new ProgressionCheckpoint
        {
            requiredPopulation = 3500,
            reward = RewardType.YellowStar
        }
    };

    private bool[] checkpointUnlocked;

    public event Action<RewardType> OnRewardUnlocked;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        checkpointUnlocked =
            new bool[checkpoints.Length];
    }

    private void Start()
    {
        CheckProgression();
    }

    private void Update()
    {
        CheckProgression();
    }

    private void CheckProgression()
    {
        if (checkpoints == null ||
            checkpoints.Length == 0)
        {
            return;
        }

        if (PopulationManager.Instance == null)
        {
            return;
        }

        int currentPopulation =
            PopulationManager.Instance.TotalPopulation;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpointUnlocked[i])
            {
                continue;
            }

            if (currentPopulation >=
                checkpoints[i].requiredPopulation)
            {
                UnlockCheckpoint(i);
            }
        }
    }

    private void UnlockCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex < 0 ||
            checkpointIndex >= checkpoints.Length)
        {
            return;
        }

        if (checkpointUnlocked[checkpointIndex])
        {
            return;
        }

        checkpointUnlocked[checkpointIndex] = true;

        RewardType reward =
            checkpoints[checkpointIndex].reward;

        Debug.Log(
            "Population checkpoint reached: " +
            checkpoints[checkpointIndex].requiredPopulation +
            " | Reward unlocked: " +
            reward
        );

        if (OnRewardUnlocked != null)
        {
            OnRewardUnlocked.Invoke(reward);
        }
    }

    public bool IsRewardUnlocked(RewardType reward)
    {
        if (checkpoints == null)
        {
            return false;
        }

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i].reward == reward &&
                checkpointUnlocked[i])
            {
                return true;
            }
        }

        return false;
    }

    public int GetCurrentPopulation()
    {
        if (PopulationManager.Instance == null)
        {
            return 0;
        }

        return PopulationManager.Instance.TotalPopulation;
    }

    public int GetNextCheckpointPopulation()
    {
        if (checkpoints == null ||
            checkpoints.Length == 0)
        {
            return -1;
        }

        int currentPopulation =
            GetCurrentPopulation();

        int nextCheckpoint = -1;
        int smallestRequiredPopulation =
            int.MaxValue;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpointUnlocked[i])
            {
                continue;
            }

            int required =
                checkpoints[i].requiredPopulation;

            if (required > currentPopulation &&
                required < smallestRequiredPopulation)
            {
                smallestRequiredPopulation =
                    required;

                nextCheckpoint = required;
            }
        }




        return nextCheckpoint;
    }

    public int GetPreviousCheckpointPopulation()
    {
        if (checkpoints == null ||
            checkpoints.Length == 0)
        {
            return 0;
        }

        int currentPopulation =
            GetCurrentPopulation();

        int previousCheckpoint = 0;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            int required =
                checkpoints[i].requiredPopulation;

            if (required <= currentPopulation)
            {
                if (required > previousCheckpoint)
                {
                    previousCheckpoint = required;
                }
            }
        }

        return previousCheckpoint;
    }

    public void ResetProgression()
    {
        if (checkpointUnlocked == null)
        {
            return;
        }

        for (int i = 0; i < checkpointUnlocked.Length; i++)
        {
            checkpointUnlocked[i] = false;
        }

        CheckProgression();

        Debug.Log(
            "Population progression reset."
        );
    }

    public void ForceCheckProgression()
    {
        CheckProgression();
    }

}