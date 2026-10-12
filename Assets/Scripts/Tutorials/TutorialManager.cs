
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        [Tooltip("Leave empty if this step should not highlight anything.")]
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

    [Header("Tutorial Content")]
    [Tooltip("Add only WELCOME and BUILDING_SELECTION for now.")]
    [SerializeField]
    private List<TutorialDefinition> tutorials =
        new List<TutorialDefinition>();

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string levelSelectSceneName = "LevelSelect";

    [Header("References")]
    [SerializeField] private TutorialPanelUI tutorialPanel;

    [Header("Behaviour")]
    [SerializeField] private bool pauseGameWhileTutorialIsOpen = true;

    private TutorialDefinition currentTutorial;
    private int currentStepIndex;
    private bool tutorialOpen;
    private float previousTimeScale = 1f;
    private string pendingTutorialId;

    private int lastRequestedSceneHandle = -1;

    private const string CompletedPrefix =
        "CityBloxx_Tutorial_Completed_";

    public bool IsTutorialOpen => tutorialOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Handles the scene in which this manager first appears.
        ProcessScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ProcessScene(scene);
    }

    private void ProcessScene(Scene scene)
    {
        if (!scene.IsValid())
            return;

        // Prevent the initial Start and sceneLoaded event from
        // processing the same scene twice.
        if (lastRequestedSceneHandle == scene.handle)
            return;

        lastRequestedSceneHandle = scene.handle;
        StartCoroutine(HandleSceneLoaded(scene));
    }

    private IEnumerator HandleSceneLoaded(Scene scene)
    {
        // Allow the scene's UI objects to finish initializing.
        yield return null;

        // Do not retain a reference to a panel from the old scene.
        tutorialPanel = FindTutorialPanel(scene);

        if (tutorialPanel == null)
        {
            Debug.LogWarning(
                "TutorialManager: No TutorialPanelUI was found in scene '" +
                scene.name + "'."
            );
            yield break;
        }

        string tutorialId = GetTutorialIdForScene(scene.name);

        if (string.IsNullOrEmpty(tutorialId))
            yield break;

        if (IsTutorialCompleted(tutorialId))
        {
            Debug.Log(
                "TutorialManager: Tutorial '" + tutorialId +
                "' has already been completed."
            );
            yield break;
        }

        // If another tutorial is still open, queue this one.
        if (tutorialOpen)
        {
            pendingTutorialId = tutorialId;
            yield break;
        }

        TryShowTutorial(tutorialId);
    }

    private string GetTutorialIdForScene(string sceneName)
    {
        if (string.Equals(
                sceneName,
                mainMenuSceneName,
                StringComparison.OrdinalIgnoreCase))
        {
            return "WELCOME";
        }

        if (string.Equals(
                sceneName,
                levelSelectSceneName,
                StringComparison.OrdinalIgnoreCase))
        {
            return "BUILDING_SELECTION";
        }

        // No tutorial is automatically triggered in other scenes.
        return null;
    }

    private TutorialPanelUI FindTutorialPanel(Scene scene)
    {
        TutorialPanelUI[] panels =
            FindObjectsByType<TutorialPanelUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (TutorialPanelUI panel in panels)
        {
            if (panel != null && panel.gameObject.scene == scene)
                return panel;
        }

        return null;
    }

    public bool TryShowTutorial(string tutorialId)
    {
        if (string.IsNullOrWhiteSpace(tutorialId))
            return false;

        if (IsTutorialCompleted(tutorialId))
            return false;

        if (tutorialOpen)
        {
            pendingTutorialId = tutorialId;
            return false;
        }

        ShowTutorial(tutorialId);
        return tutorialOpen;
    }

    public void ShowTutorial(string tutorialId)
    {
        if (string.IsNullOrWhiteSpace(tutorialId))
        {
            Debug.LogWarning(
                "TutorialManager: Cannot show a tutorial with an empty ID."
            );
            return;
        }

        if (IsTutorialCompleted(tutorialId))
            return;

        TutorialDefinition tutorial = FindTutorial(tutorialId);

        if (tutorial == null)
        {
            Debug.LogWarning(
                "TutorialManager: Tutorial '" + tutorialId +
                "' is missing from the Tutorial Content list."
            );
            return;
        }

        if (tutorial.steps == null || tutorial.steps.Count == 0)
        {
            Debug.LogWarning(
                "TutorialManager: Tutorial '" + tutorialId +
                "' has no steps. It has not been marked completed."
            );
            return;
        }

        if (tutorialPanel == null)
        {
            tutorialPanel = FindTutorialPanel(SceneManager.GetActiveScene());
        }

        if (tutorialPanel == null)
        {
            Debug.LogWarning(
                "TutorialManager: No tutorial panel is available in the active scene."
            );
            return;
        }

        OpenTutorial(tutorial);
    }

    private void OpenTutorial(TutorialDefinition tutorial)
    {
        currentTutorial = tutorial;
        currentStepIndex = 0;
        tutorialOpen = true;

        if (pauseGameWhileTutorialIsOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        tutorialPanel.ShowTutorial(currentTutorial, currentStepIndex);

        Debug.Log(
            "TutorialManager: Opened tutorial '" +
            tutorial.tutorialId + "'."
        );
    }

    public void NextStep()
    {
        if (!tutorialOpen || currentTutorial == null)
            return;

        if (currentStepIndex < currentTutorial.steps.Count - 1)
        {
            currentStepIndex++;
            tutorialPanel.ShowTutorial(currentTutorial, currentStepIndex);
        }
        else
        {
            FinishCurrentTutorial();
        }
    }

    public void PreviousStep()
    {
        if (!tutorialOpen || currentTutorial == null)
            return;

        if (currentStepIndex <= 0)
            return;

        currentStepIndex--;
        tutorialPanel.ShowTutorial(currentTutorial, currentStepIndex);
    }

    public void SkipCurrentTutorial()
    {
        if (!tutorialOpen || currentTutorial == null)
            return;

        // Skipping counts as completing this tutorial.
        FinishCurrentTutorial();
    }

    private void FinishCurrentTutorial()
    {
        if (currentTutorial == null)
        {
            CloseTutorial();
            return;
        }

        MarkTutorialCompleted(currentTutorial.tutorialId);
        CloseTutorial();
    }

    private void CloseTutorial()
    {
        tutorialOpen = false;

        if (tutorialPanel != null)
            tutorialPanel.HideTutorial();

        if (pauseGameWhileTutorialIsOpen)
            Time.timeScale = previousTimeScale;

        currentTutorial = null;
        currentStepIndex = 0;

        if (!string.IsNullOrEmpty(pendingTutorialId))
        {
            string nextTutorialId = pendingTutorialId;
            pendingTutorialId = null;
            StartCoroutine(ShowPendingTutorial(nextTutorialId));
        }
    }

    private IEnumerator ShowPendingTutorial(string tutorialId)
    {
        yield return null;

        if (tutorialOpen || IsTutorialCompleted(tutorialId))
            yield break;

        tutorialPanel = FindTutorialPanel(SceneManager.GetActiveScene());

        if (tutorialPanel != null)
            ShowTutorial(tutorialId);
    }

    public bool IsTutorialCompleted(string tutorialId)
    {
        if (string.IsNullOrWhiteSpace(tutorialId))
            return false;

        return PlayerPrefs.GetInt(
            CompletedPrefix + tutorialId,
            0
        ) == 1;
    }

    private void MarkTutorialCompleted(string tutorialId)
    {
        PlayerPrefs.SetInt(CompletedPrefix + tutorialId, 1);
        PlayerPrefs.Save();

        Debug.Log(
            "TutorialManager: Tutorial '" + tutorialId +
            "' marked as completed."
        );
    }

    private TutorialDefinition FindTutorial(string tutorialId)
    {
        foreach (TutorialDefinition tutorial in tutorials)
        {
            if (tutorial == null)
                continue;

            if (string.Equals(
                    tutorial.tutorialId,
                    tutorialId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return tutorial;
            }
        }

        return null;
    }

    [ContextMenu("Reset Main Menu and Level Select Tutorials")]
    private void ResetCurrentTutorials()
    {
        PlayerPrefs.DeleteKey(CompletedPrefix + "WELCOME");
        PlayerPrefs.DeleteKey(CompletedPrefix + "BUILDING_SELECTION");
        PlayerPrefs.Save();

        Debug.Log(
            "TutorialManager: Main Menu and Level Select tutorial progress reset."
        );
    }
}
