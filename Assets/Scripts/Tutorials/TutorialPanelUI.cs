using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject tutorialRoot;
    [SerializeField] private Canvas tutorialCanvas;

    [Header("Dim Background")]
    [SerializeField] private Image dimBackground;

    [Tooltip("How dark the screen becomes while the tutorial is open.")]
    [Range(0f, 1f)]
    [SerializeField] private float dimAlpha = 0.60f;

    [Header("Tutorial Window")]
    [SerializeField] private RectTransform tutorialWindow;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text pageText;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button skipButton;

    [Header("Highlight")]
    [SerializeField] private Image highlightImage;
    [SerializeField] private RectTransform highlightRect;

    [Header("Highlight Settings")]
    [SerializeField] private float highlightExtraSize = 20f;

    private TMP_Text nextButtonText;

    private void Awake()
    {
        SetupCanvas();
        FindNextButtonText();
        SetupButtons();
        SetupDimBackground();
        SetupRoot();
        HideTutorial();
    }

    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipClicked);
        }
    }

    // =========================================================
    // INITIAL SETUP
    // =========================================================

    private void SetupCanvas()
    {
        if (tutorialCanvas == null)
        {
            tutorialCanvas = GetComponent<Canvas>();
        }

        if (tutorialCanvas == null)
        {
            Debug.LogWarning(
                "TutorialPanelUI: No Canvas component was found."
            );

            return;
        }

        // The tutorial must always render above the normal UI.
        tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        tutorialCanvas.overrideSorting = true;
        tutorialCanvas.sortingOrder = 100;

        CanvasScaler scaler =
            tutorialCanvas.GetComponent<CanvasScaler>();

        if (scaler != null)
        {
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1080f, 1920f);

            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private void SetupRoot()
    {
        if (tutorialRoot == null)
        {
            Debug.LogWarning(
                "TutorialPanelUI: Tutorial Root is not assigned."
            );

            return;
        }

        RectTransform rootRect =
            tutorialRoot.GetComponent<RectTransform>();

        if (rootRect != null)
        {
            // Make TutorialRoot cover the complete tutorial canvas.
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;

            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
        }

        // Make sure the tutorial window is rendered above
        // the dim background and highlight.
        if (tutorialWindow != null)
        {
            tutorialWindow.SetAsLastSibling();
        }
    }

    private void SetupButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextClicked);
            nextButton.onClick.AddListener(OnNextClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipClicked);
            skipButton.onClick.AddListener(OnSkipClicked);
        }
    }

    private void FindNextButtonText()
    {
        if (nextButton == null)
        {
            nextButtonText = null;
            return;
        }

        nextButtonText =
            nextButton.GetComponentInChildren<TMP_Text>(true);

        if (nextButtonText == null)
        {
            Debug.LogWarning(
                "TutorialPanelUI: Could not find TextMeshPro text inside NextButton."
            );
        }
    }

    private void SetupDimBackground()
    {
        if (dimBackground == null)
        {
            return;
        }

        Color color = dimBackground.color;
        color.a = dimAlpha;
        dimBackground.color = color;

        dimBackground.raycastTarget = true;

        // Dim background must be behind everything else.
        dimBackground.transform.SetAsFirstSibling();
    }

    // =========================================================
    // SHOW / HIDE
    // =========================================================

    public void ShowTutorial(
        TutorialManager.TutorialDefinition tutorial,
        int stepIndex)
    {
        if (tutorial == null)
        {
            Debug.LogWarning(
                "TutorialPanelUI: Cannot show tutorial because tutorial is null."
            );

            return;
        }

        if (tutorial.steps == null ||
            tutorial.steps.Count == 0)
        {
            Debug.LogWarning(
                "TutorialPanelUI: Tutorial has no steps."
            );

            return;
        }

        stepIndex = Mathf.Clamp(
            stepIndex,
            0,
            tutorial.steps.Count - 1
        );

        TutorialManager.TutorialStep step =
            tutorial.steps[stepIndex];

        // -----------------------------------------------------
        // MAKE SURE THE CANVAS IS ACTIVE
        // -----------------------------------------------------

        if (tutorialCanvas != null)
        {
            tutorialCanvas.gameObject.SetActive(true);
        }

        // -----------------------------------------------------
        // MAKE SURE ROOT IS ACTIVE
        // -----------------------------------------------------

        if (tutorialRoot != null)
        {
            tutorialRoot.SetActive(true);
        }

        // -----------------------------------------------------
        // FIX RENDER ORDER
        // -----------------------------------------------------

        if (tutorialCanvas != null)
        {
            tutorialCanvas.overrideSorting = true;
            tutorialCanvas.sortingOrder = 100;
        }

        if (dimBackground != null)
        {
            dimBackground.transform.SetAsFirstSibling();
            dimBackground.gameObject.SetActive(true);
        }

        if (highlightImage != null)
        {
            highlightImage.transform.SetAsLastSibling();
        }

        if (tutorialWindow != null)
        {
            tutorialWindow.gameObject.SetActive(true);
            tutorialWindow.SetAsLastSibling();
        }

        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        if (titleText != null)
        {
            titleText.text = step.title;
        }

        if (messageText != null)
        {
            messageText.text = step.message;
        }

        if (pageText != null)
        {
            pageText.text =
                $"{stepIndex + 1} / {tutorial.steps.Count}";
        }

        // -----------------------------------------------------
        // BUTTONS
        // -----------------------------------------------------

        bool isFirstStep =
            stepIndex == 0;

        bool isLastStep =
            stepIndex == tutorial.steps.Count - 1;

        if (backButton != null)
        {
            backButton.gameObject.SetActive(!isFirstStep);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
        }

        if (nextButtonText != null)
        {
            nextButtonText.text =
                isLastStep ? "GOT IT" : "NEXT";
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
        }

        // -----------------------------------------------------
        // HIGHLIGHT
        // -----------------------------------------------------

        UpdateHighlight(step);

        // -----------------------------------------------------
        // WINDOW
        // -----------------------------------------------------

        PositionTutorialWindow();
    }

    public void HideTutorial()
    {
        if (tutorialRoot != null)
        {
            tutorialRoot.SetActive(false);
        }

        HideHighlight();
    }

    // =========================================================
    // HIGHLIGHT
    // =========================================================

    private void UpdateHighlight(
        TutorialManager.TutorialStep step)
    {
        if (step == null)
        {
            HideHighlight();
            return;
        }

        if (!step.showHighlight ||
            string.IsNullOrWhiteSpace(step.targetId))
        {
            HideHighlight();
            return;
        }

        TutorialTarget target =
            FindTutorialTarget(step.targetId);

        if (target == null)
        {
            Debug.LogWarning(
                $"TutorialPanelUI: Could not find tutorial target '{step.targetId}'."
            );

            HideHighlight();
            return;
        }

        Rect screenRect =
            target.GetScreenRect();

        ShowHighlight(screenRect);
    }

    private TutorialTarget FindTutorialTarget(
        string targetId)
    {
        TutorialTarget[] targets =
            FindObjectsByType<TutorialTarget>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                continue;
            }

            if (string.Equals(
                targets[i].TargetId,
                targetId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                return targets[i];
            }
        }

        return null;
    }

    private void ShowHighlight(Rect screenRect)
    {
        if (highlightImage == null ||
            highlightRect == null ||
            tutorialCanvas == null)
        {
            return;
        }

        highlightImage.gameObject.SetActive(true);

        RectTransform canvasRect =
            tutorialCanvas.transform as RectTransform;

        if (canvasRect == null)
        {
            HideHighlight();
            return;
        }

        Vector2 localPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenRect.center,
            null,
            out localPosition
        );

        highlightRect.anchoredPosition =
            localPosition;

        highlightRect.sizeDelta =
            screenRect.size +
            Vector2.one * highlightExtraSize;

        // Highlight must be above dim background.
        // Tutorial window will be moved above it afterward.
        highlightImage.transform.SetAsLastSibling();

        if (tutorialWindow != null)
        {
            tutorialWindow.SetAsLastSibling();
        }
    }

    private void HideHighlight()
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
    }

    // =========================================================
    // WINDOW POSITION
    // =========================================================

    private void PositionTutorialWindow()
    {
        if (tutorialWindow == null)
        {
            return;
        }

        tutorialWindow.anchorMin =
            new Vector2(0.5f, 0.5f);

        tutorialWindow.anchorMax =
            new Vector2(0.5f, 0.5f);

        tutorialWindow.pivot =
            new Vector2(0.5f, 0.5f);

        tutorialWindow.anchoredPosition =
            Vector2.zero;

        // Do NOT change its size.
        // Your existing window size/design is preserved.

        tutorialWindow.SetAsLastSibling();
    }

    // =========================================================
    // BUTTON EVENTS
    // =========================================================

    private void OnNextClicked()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.NextStep();
        }
    }

    private void OnBackClicked()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.PreviousStep();
        }
    }

    private void OnSkipClicked()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.SkipCurrentTutorial();
        }
    }
}