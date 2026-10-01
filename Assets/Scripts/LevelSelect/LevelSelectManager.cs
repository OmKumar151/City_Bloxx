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
        int oldIndex = currentIndex;

        currentIndex++;

        if (currentIndex >= levels.Length)
            currentIndex = levels.Length - 1;

        // Only play the sound if the selection actually changed.
        if (currentIndex != oldIndex)
        {
            PlayNavigationSound();
            UpdateSelection();
        }
    }

    public void PreviousLevel()
    {
        int oldIndex = currentIndex;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = 0;

        // Only play the sound if the selection actually changed.
        if (currentIndex != oldIndex)
        {
            PlayNavigationSound();
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        foreach (var level in levels)
            level.Select(false);

        levels[currentIndex].Select(true);

        topBar.UpdateTopBar(
            levels[currentIndex].levelData,
            currentPopulation,
            totalPopulation);

        infoPanel.ShowMessage(
            levels[currentIndex].levelData.levelName);
    }

    public void PlaySelectedLevel()
    {
        PlayMajorButtonSound();

        Debug.Log(
            "Play " +
            levels[currentIndex].levelData.levelName);
    }

    public void BackToMenu()
    {
        PlayWindowSwitchSound();

        Debug.Log("Return to Main Menu");
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayNavigationSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: AudioManager is not available.");
            return;
        }

        AudioManager.Instance.PlayNavigationButton();
    }

    private void PlayMajorButtonSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: AudioManager is not available.");
            return;
        }

        AudioManager.Instance.PlayMajorButton();
    }

    private void PlayWindowSwitchSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: AudioManager is not available.");
            return;
        }

        AudioManager.Instance.PlayWindowSwitch();
    }
}