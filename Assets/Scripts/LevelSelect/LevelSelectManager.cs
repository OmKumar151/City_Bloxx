
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public LevelNode[] levels;

    public TopBarUI topBar;

    public InfoPanelUI infoPanel;

    public int currentIndex;

    public int currentPopulation = 17;

    public int totalPopulation = 32443;

    private void Start()
    {
        UpdateSelection();
    }

    public void NextLevel()
    {
        if (!HasLevels())
        {
            return;
        }

        // Normalise the index before navigating.
        currentIndex = Mathf.Clamp(
            currentIndex,
            0,
            levels.Length - 1
        );

        int oldIndex = currentIndex;

        currentIndex = Mathf.Min(
            currentIndex + 1,
            levels.Length - 1
        );

        // Only play the sound if the selection changed.
        if (currentIndex != oldIndex)
        {
            PlayNavigationSound();
            UpdateSelection();
        }
    }

    public void PreviousLevel()
    {
        if (!HasLevels())
        {
            return;
        }

        // Normalise the index before navigating.
        currentIndex = Mathf.Clamp(
            currentIndex,
            0,
            levels.Length - 1
        );

        int oldIndex = currentIndex;

        currentIndex = Mathf.Max(
            currentIndex - 1,
            0
        );

        // Only play the sound if the selection changed.
        if (currentIndex != oldIndex)
        {
            PlayNavigationSound();
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        if (!HasLevels())
        {
            return;
        }

        // Keep the index valid if it was changed
        // in the Inspector or by another script.
        currentIndex = Mathf.Clamp(
            currentIndex,
            0,
            levels.Length - 1
        );

        // Clear previous selections.
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                levels[i].Select(false);
            }
        }

        LevelNode selectedLevel = levels[currentIndex];

        if (selectedLevel == null ||
            selectedLevel.levelData == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: The selected level " +
                "or its LevelData is not assigned.",
                this
            );

            return;
        }

        selectedLevel.Select(true);

        if (topBar != null)
        {
            topBar.UpdateTopBar(
                selectedLevel.levelData,
                currentPopulation,
                totalPopulation
            );
        }

        if (infoPanel != null)
        {
            infoPanel.ShowMessage(
                selectedLevel.levelData.levelName
            );
        }
    }

    public void PlaySelectedLevel()
    {
        if (!HasValidSelection())
        {
            return;
        }

        PlayMajorButtonSound();

        Debug.Log(
            "Play " + levels[currentIndex].levelData.levelName
        );
    }

    public void BackToMenu()
    {
        PlayWindowSwitchSound();

        Debug.Log("Return to Main Menu");
    }

    private bool HasLevels()
    {
        if (levels != null && levels.Length > 0)
        {
            return true;
        }

        Debug.LogWarning(
            "LevelSelectManager: No levels have been assigned.",
            this
        );

        return false;
    }

    private bool HasValidSelection()
    {
        if (!HasLevels())
        {
            return false;
        }

        currentIndex = Mathf.Clamp(
            currentIndex,
            0,
            levels.Length - 1
        );

        if (levels[currentIndex] == null ||
            levels[currentIndex].levelData == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: The selected level " +
                "or its LevelData is not assigned.",
                this
            );

            return false;
        }

        return true;
    }

    // =====================================================
    // AUDIO
    // =====================================================

    private void PlayNavigationSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: AudioManager is not available.",
                this
            );

            return;
        }

        AudioManager.Instance.PlayNavigationButton();
    }

    private void PlayMajorButtonSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: AudioManager is not available.",
                this
            );

            return;
        }

        AudioManager.Instance.PlayMajorButton();
    }

    private void PlayWindowSwitchSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: AudioManager is not available.",
                this
            );

            return;
        }

        AudioManager.Instance.PlayWindowSwitch();
    }
}
