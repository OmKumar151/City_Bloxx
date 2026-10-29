
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Level Selection")]
    public LevelNode[] levels;
    public TopBarUI topBar;
    public InfoPanelUI infoPanel;
    public int currentIndex;

    [Header("Population Display")]
    public int currentPopulation = 17;
    public int totalPopulation = 32443;

    [Header("Scene Settings")]
    public string fallbackGameplayScene = "Gameplay1";
    public string mainMenuScene = "Main Menu";

    private bool isLoadingScene;

    private void Start()
    {
        UpdateSelection();
    }

    public void NextLevel()
    {
        if (!HasLevels())
            return;

        currentIndex = Mathf.Clamp(currentIndex, 0, levels.Length - 1);

        int oldIndex = currentIndex;
        currentIndex = Mathf.Min(currentIndex + 1, levels.Length - 1);

        if (currentIndex != oldIndex)
        {
            PlayNavigationSound();
            UpdateSelection();
        }
    }

    public void PreviousLevel()
    {
        if (!HasLevels())
            return;

        currentIndex = Mathf.Clamp(currentIndex, 0, levels.Length - 1);

        int oldIndex = currentIndex;
        currentIndex = Mathf.Max(currentIndex - 1, 0);

        if (currentIndex != oldIndex)
        {
            PlayNavigationSound();
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        if (!HasLevels())
            return;

        currentIndex = Mathf.Clamp(currentIndex, 0, levels.Length - 1);

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
                levels[i].Select(false);
        }

        LevelNode selectedLevel = levels[currentIndex];

        if (selectedLevel == null || selectedLevel.levelData == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: Selected level or LevelData is missing.",
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
            infoPanel.ShowMessage(selectedLevel.levelData.levelName);
    }

    public void PlaySelectedLevel()
    {
        if (isLoadingScene || !HasValidSelection())
            return;

        LevelData data = levels[currentIndex].levelData;

        if (!data.unlocked)
        {
            Debug.LogWarning(
                "LevelSelectManager: This level is locked.",
                this
            );

            PlayNavigationSound();
            return;
        }

        BuildingGameplayConfig configuration = data.buildingConfiguration;

        if (configuration == null)
        {
            Debug.LogError(
                "LevelSelectManager: No BuildingGameplayConfig is assigned " +
                "to level '" + data.levelName + "'.",
                this
            );

            return;
        }

        if (!configuration.HasValidGameplaySetup())
            return;

        string sceneToLoad = string.IsNullOrWhiteSpace(data.gameplayScene)
            ? fallbackGameplayScene
            : data.gameplayScene;

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError(
                "LevelSelectManager: No gameplay scene is configured.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError(
                "LevelSelectManager: Scene '" + sceneToLoad +
                "' cannot be loaded. Check its name and Build Settings.",
                this
            );

            return;
        }

        PlayMajorButtonSound();

        GameplaySelection.Select(configuration);
        isLoadingScene = true;

        SceneManager.LoadScene(sceneToLoad);
    }

    public void BackToMenu()
    {
        if (isLoadingScene)
            return;

        if (string.IsNullOrWhiteSpace(mainMenuScene) ||
            !Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            Debug.LogError(
                "LevelSelectManager: Main menu scene '" + mainMenuScene +
                "' cannot be loaded. Check the scene name and Build Settings.",
                this
            );

            return;
        }

        PlayWindowSwitchSound();
        isLoadingScene = true;
        SceneManager.LoadScene(mainMenuScene);
    }

    private bool HasLevels()
    {
        if (levels != null && levels.Length > 0)
            return true;

        Debug.LogWarning(
            "LevelSelectManager: No levels have been assigned.",
            this
        );

        return false;
    }

    private bool HasValidSelection()
    {
        if (!HasLevels())
            return false;

        currentIndex = Mathf.Clamp(currentIndex, 0, levels.Length - 1);

        if (levels[currentIndex] == null ||
            levels[currentIndex].levelData == null)
        {
            Debug.LogWarning(
                "LevelSelectManager: Selected level or LevelData is missing.",
                this
            );

            return false;
        }

        return true;
    }

    private void PlayNavigationSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayNavigationButton();
    }

    private void PlayMajorButtonSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMajorButton();
    }

    private void PlayWindowSwitchSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWindowSwitch();
    }
}
