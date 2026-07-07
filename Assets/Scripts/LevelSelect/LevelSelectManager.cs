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
        currentIndex++;

        if (currentIndex >= levels.Length)
            currentIndex = levels.Length - 1;

        UpdateSelection();
    }

    public void PreviousLevel()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = 0;

        UpdateSelection();
    }

    void UpdateSelection()
    {
        foreach (var level in levels)
            level.Select(false);

        levels[currentIndex].Select(true);

        topBar.UpdateTopBar(
            levels[currentIndex].levelData,
            currentPopulation,
            totalPopulation);

        infoPanel.Show(
            levels[currentIndex].levelData);
    }

    public void PlaySelectedLevel()
    {
        Debug.Log("Play " +
            levels[currentIndex].levelData.levelName);
    }

    public void BackToMenu()
    {
        Debug.Log("Return to Main Menu");
    }
}