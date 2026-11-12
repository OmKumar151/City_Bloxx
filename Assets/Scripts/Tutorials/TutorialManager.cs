using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows one-time tutorials when the player first meets something (the
/// main menu, the city, placing a building, a new unlock...) and the "i"
/// game guide, which can be opened at any time. Text lives in GuideContent.
/// Survives scene loads and creates itself if no scene has one.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Serializable]
    public class TutorialStep
    {
        [Header("Step Text")]
        public string title;

        [TextArea(3, 8)]
        public string message;

        [Header("Highlight")]
        [Tooltip("TutorialTarget id or scene object name to spotlight. " +
                 "Leave empty for no highlight.")]
        public string targetId;

        public bool showHighlight = true;
    }

    [Serializable]
    public class TutorialDefinition
    {
        [Header("Tutorial")]
        public string tutorialId;
        public string displayName;

        [Header("Steps")]
        public List<TutorialStep> steps = new List<TutorialStep>();
    }

    [Serializable]
    public class InfoButtonPlacement
    {
        public string sceneName;
        public bool show = true;

        [Tooltip("Anchor on the screen (0,0 bottom-left, 1,1 top-right).")]
        public Vector2 anchor = new Vector2(0.5f, 0.5f);

        [Tooltip("Offset from the anchor, in the scene canvas's units.")]
        public Vector2 position;

        [Min(40f)] public float size = 110f;
    }

    // Renamed from "tutorials" on purpose: the old two-page texts saved in
    // the scenes are dropped so the full built-in text in GuideContent is used.
    [Header("Tutorial Content")]
    [Tooltip("Optional overrides. A tutorial listed here (same id) replaces " +
             "the built-in text in GuideContent. Leave empty to use the built-in text.")]
    [SerializeField]
    private List<TutorialDefinition> tutorialOverrides = new List<TutorialDefinition>();

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string levelSelectSceneName = "LevelSelect";
    [SerializeField] private string gameplaySceneName = "Gameplay1";

    [Header("Look")]
    [Tooltip("Font for the guide. Empty uses Roboto Bold, else the TMP default.")]
    [SerializeField] private TMP_FontAsset guideFont;

    [Header("Info (i) Button")]
    [Tooltip("Where the i button sits in each scene. Scenes not listed don't show it.")]
    [SerializeField]
    private List<InfoButtonPlacement> infoButtonPlacements = new List<InfoButtonPlacement>
    {
        // Mirrors the Settings button on the other side of the screen.
        new InfoButtonPlacement
        {
            sceneName = "Main Menu", anchor = new Vector2(0.5f, 0.5f),
            position = new Vector2(416f, 845f), size = 120f
        },
        new InfoButtonPlacement
        {
            sceneName = "LevelSelect", anchor = new Vector2(0.5f, 0.5f),
            position = new Vector2(391f, 644f), size = 100f
        },
        // Under the floor counter, top right.
        new InfoButtonPlacement
        {
            sceneName = "Gameplay1", anchor = new Vector2(0.5f, 1f),
            position = new Vector2(452f, -235f), size = 90f
        },
    };

    [Header("Behaviour")]
    [SerializeField] private bool pauseGameWhileTutorialIsOpen = true;

    private const string CompletedPrefix = "CityBloxx_Tutorial_Completed_";

    // How long after closing the guide taps are still ignored by gameplay,
    // so the tap on "GOT IT" doesn't also drop a block.
    private const float InputGraceSeconds = 0.25f;

    private GuidePanel panel;
    private TutorialDefinition currentTutorial;
    private int currentStepIndex;
    private bool tutorialOpen;
    private bool currentIsGuide;
    private float previousTimeScale = 1f;
    private float lastClosedTime = float.NegativeInfinity;
    private int lastProcessedSceneHandle = -1;

    private readonly Queue<KeyValuePair<string, int>> pendingTutorials =
        new Queue<KeyValuePair<string, int>>();

    public bool IsTutorialOpen => tutorialOpen;

    // =========================================================
    // LIFECYCLE
    // =========================================================

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("TutorialManager").AddComponent<TutorialManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // DontDestroyOnLoad only works on root objects. In the scenes this
        // sits under "TutorialSystem", so it used to be destroyed on every
        // scene change and the LevelSelect tutorial never had a panel.
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);

        if (guideFont == null)
            guideFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");

        panel = GuidePanel.Create(transform, guideFont);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        ProcessScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single && tutorialOpen)
        {
            // Left the scene with a page still up: drop it without marking
            // it completed, so it shows again next time.
            CloseTutorial(markCompleted: false);
            pendingTutorials.Clear();
        }

        ProcessScene(scene);
    }

    private void ProcessScene(Scene scene)
    {
        if (!scene.IsValid() || lastProcessedSceneHandle == scene.handle)
            return;

        lastProcessedSceneHandle = scene.handle;
        StartCoroutine(HandleSceneLoaded(scene));
    }

    private IEnumerator HandleSceneLoaded(Scene scene)
    {
        // Let the scene's UI finish initialising.
        yield return null;

        GuidePanel.EnsureEventSystem();
        panel.MatchSceneScaling();
        RefreshInfoButton();

        int buildingIndex;
        string tutorialId = GetTutorialIdForScene(scene.name, out buildingIndex);

        if (!string.IsNullOrEmpty(tutorialId))
            TryShowTutorial(tutorialId, buildingIndex);
    }

    private string GetTutorialIdForScene(string sceneName, out int buildingIndex)
    {
        buildingIndex = -1;

        if (SceneIs(sceneName, mainMenuSceneName))
            return GuideContent.Welcome;

        if (SceneIs(sceneName, levelSelectSceneName))
            return GuideContent.CityIntro;

        if (SceneIs(sceneName, gameplaySceneName))
        {
            if (GameplaySelection.IsQuickPlay)
                return GuideContent.QuickGame;

            buildingIndex = GuideContent.IndexOfBuildingId(GameplaySelection.BuildingId);
            return GuideContent.BuildTower;
        }

        return null;
    }

    private static bool SceneIs(string sceneName, string expected)
    {
        return string.Equals(sceneName, expected, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================
    // PUBLIC API
    // =========================================================

    /// <summary>
    /// Shows a one-time tutorial unless it was already completed. If another
    /// tutorial is open, this one is queued. buildingIndex (0 Blue .. 3
    /// Yellow) lets the text name the building it is about.
    /// </summary>
    public bool TryShowTutorial(string tutorialId, int buildingIndex = -1)
    {
        if (string.IsNullOrWhiteSpace(tutorialId) || IsTutorialCompleted(tutorialId))
            return false;

        if (tutorialOpen)
        {
            if (!IsQueued(tutorialId) &&
                (currentTutorial == null || currentTutorial.tutorialId != tutorialId))
            {
                pendingTutorials.Enqueue(new KeyValuePair<string, int>(tutorialId, buildingIndex));
            }

            return false;
        }

        TutorialDefinition tutorial = GetTutorial(tutorialId, buildingIndex);

        if (tutorial == null)
        {
            Debug.LogWarning("TutorialManager: No content for tutorial '" + tutorialId + "'.");
            return false;
        }

        Open(tutorial, 0, isGuide: false);
        return true;
    }

    /// <summary>Kept for existing callers: shows the tutorial if not yet completed.</summary>
    public void ShowTutorial(string tutorialId)
    {
        TryShowTutorial(tutorialId);
    }

    /// <summary>
    /// The "i" button: the full game guide, opened on the page that fits
    /// the current scene. Never marked completed.
    /// </summary>
    public void ShowGuide()
    {
        if (tutorialOpen)
            return;

        TutorialDefinition guide = GetTutorial(GuideContent.Guide, -1);

        if (guide == null)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Open(guide, GetGuideStartPage(), isGuide: true);
    }

    /// <summary>
    /// True while gameplay taps must be ignored: the guide is open, was
    /// just closed, or the tap is on the "i" button.
    /// </summary>
    public bool BlocksGameplayInput(Vector2 screenPoint)
    {
        return tutorialOpen ||
               Time.unscaledTime - lastClosedTime < InputGraceSeconds ||
               (panel != null && panel.IsOverInfoButton(screenPoint));
    }

    public void NextStep()
    {
        if (!tutorialOpen || currentTutorial == null)
            return;

        if (currentStepIndex < currentTutorial.steps.Count - 1)
        {
            currentStepIndex++;
            panel.Show(currentTutorial, currentStepIndex, currentIsGuide);
            PlayClick();
        }
        else
        {
            CloseTutorial(markCompleted: true);
        }
    }

    public void PreviousStep()
    {
        if (!tutorialOpen || currentTutorial == null || currentStepIndex <= 0)
            return;

        currentStepIndex--;
        panel.Show(currentTutorial, currentStepIndex, currentIsGuide);
        PlayClick();
    }

    public void SkipCurrentTutorial()
    {
        if (!tutorialOpen)
            return;

        // Skipping counts as completing this tutorial.
        CloseTutorial(markCompleted: true);
    }

    public bool IsTutorialCompleted(string tutorialId)
    {
        if (string.IsNullOrWhiteSpace(tutorialId))
            return false;

        return PlayerPrefs.GetInt(CompletedPrefix + tutorialId, 0) == 1;
    }

    /// <summary>Every tutorial will show again the next time it triggers.</summary>
    [ContextMenu("Reset All Tutorials")]
    public void ResetAllTutorials()
    {
        foreach (string id in GuideContent.AllTutorialIds)
            SaveStore.DeleteKey(CompletedPrefix + id);

        SaveStore.Save();

        Debug.Log("TutorialManager: All tutorials reset.");
    }

    // =========================================================
    // OPEN / CLOSE
    // =========================================================

    private void Open(TutorialDefinition tutorial, int stepIndex, bool isGuide)
    {
        currentTutorial = tutorial;
        currentStepIndex = Mathf.Clamp(stepIndex, 0, tutorial.steps.Count - 1);
        currentIsGuide = isGuide;
        tutorialOpen = true;

        if (pauseGameWhileTutorialIsOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        panel.Show(currentTutorial, currentStepIndex, isGuide);
    }

    private void CloseTutorial(bool markCompleted)
    {
        if (markCompleted && !currentIsGuide && currentTutorial != null)
            MarkTutorialCompleted(currentTutorial.tutorialId);

        tutorialOpen = false;
        lastClosedTime = Time.unscaledTime;

        panel.Hide();

        if (pauseGameWhileTutorialIsOpen)
            Time.timeScale = previousTimeScale;

        currentTutorial = null;
        currentStepIndex = 0;
        currentIsGuide = false;

        RefreshInfoButton();
        PlayClick();

        if (pendingTutorials.Count > 0)
            StartCoroutine(ShowNextPendingTutorial());
    }

    private IEnumerator ShowNextPendingTutorial()
    {
        yield return null;

        while (!tutorialOpen && pendingTutorials.Count > 0)
        {
            KeyValuePair<string, int> next = pendingTutorials.Dequeue();
            TryShowTutorial(next.Key, next.Value);
        }
    }

    private bool IsQueued(string tutorialId)
    {
        foreach (KeyValuePair<string, int> pending in pendingTutorials)
        {
            if (pending.Key == tutorialId)
                return true;
        }

        return false;
    }

    private void MarkTutorialCompleted(string tutorialId)
    {
        SaveStore.SetInt(CompletedPrefix + tutorialId, 1);
        SaveStore.Save();
    }

    // =========================================================
    // CONTENT
    // =========================================================

    private TutorialDefinition GetTutorial(string tutorialId, int buildingIndex)
    {
        if (tutorialOverrides != null)
        {
            foreach (TutorialDefinition tutorial in tutorialOverrides)
            {
                if (tutorial != null &&
                    tutorial.steps != null && tutorial.steps.Count > 0 &&
                    string.Equals(tutorial.tutorialId, tutorialId, StringComparison.OrdinalIgnoreCase))
                {
                    return tutorial;
                }
            }
        }

        TutorialDefinition built = GuideContent.Build(tutorialId, buildingIndex);
        return built != null && built.steps.Count > 0 ? built : null;
    }

    private int GetGuideStartPage()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (SceneIs(sceneName, levelSelectSceneName))
            return GuideContent.GuidePageCity;

        if (SceneIs(sceneName, gameplaySceneName))
        {
            return GameplaySelection.IsQuickPlay
                ? GuideContent.GuidePageQuickGame
                : GuideContent.GuidePageStacking;
        }

        return GuideContent.GuidePageGoal;
    }

    // =========================================================
    // INFO BUTTON
    // =========================================================

    private void RefreshInfoButton()
    {
        if (panel == null)
            return;

        string sceneName = SceneManager.GetActiveScene().name;

        // A scene with its own InfoButton doesn't need the floating one.
        bool sceneHasOwnButton = FindAnyObjectByType<InfoButton>() != null;

        foreach (InfoButtonPlacement placement in infoButtonPlacements)
        {
            if (placement != null && SceneIs(sceneName, placement.sceneName))
            {
                panel.SetInfoButton(
                    placement.show && !sceneHasOwnButton,
                    placement.anchor, placement.position, placement.size);
                return;
            }
        }

        panel.SetInfoButton(false, Vector2.zero, Vector2.zero, 0f);
    }

    private static void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
}
