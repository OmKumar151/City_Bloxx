using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// The tutorial / game-guide window and the "i" button. The objects live
/// in each scene (GuideCanvas): move or restyle them there. This drives
/// them: fills the text, grows the window's height to fit the page, and
/// spotlights a step's target (everything else is dimmed).
///
/// The buttons call InfoPressed / NextPressed / BackPressed / SkipPressed
/// through their OnClick lists.
/// </summary>
public class GuidePanel : MonoBehaviour
{
    [Header("Info (i) Button")]
    [SerializeField] private RectTransform infoButton;
    [Tooltip("Off hides the i button in this scene (the guide still opens for tutorials).")]
    [SerializeField] private bool showInfoButton = true;

    [Header("Overlay")]
    [Tooltip("Full-screen root shown while the guide is open.")]
    [SerializeField] private GameObject overlay;
    [Tooltip("Four dim pieces placed around the spotlight hole.")]
    [SerializeField] private RectTransform[] dimPieces = new RectTransform[4];
    [SerializeField] private Image highlightFrameImage;

    [Header("Window")]
    [SerializeField] private RectTransform window;
    [SerializeField] private TMP_Text titleText;
    [Tooltip("Anchored to the top of the window; its height is set to fit the page.")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextLabel;
    [SerializeField] private Button backButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text skipLabel;

    [Header("Window Sizing")]
    [Tooltip("Window height = this + the message's height.")]
    [SerializeField] private float heightWithoutMessage = 334f;
    [SerializeField] private float minMessageHeight = 160f;
    [SerializeField] private float maxMessageHeight = 980f;
    [SerializeField] private float messageFontSize = 38f;

    private const float HighlightPadding = 18f;

    private RectTransform canvasRect;
    private RectTransform highlightFrame;
    private Color highlightColour;

    private RectTransform highlightTarget;
    private float openedAt;
    private GameObject previousSelection;

    public bool IsOpen => overlay != null && overlay.activeSelf;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? (RectTransform)canvas.rootCanvas.transform : (RectTransform)transform;

        if (highlightFrameImage != null)
        {
            highlightFrame = highlightFrameImage.rectTransform;
            highlightColour = highlightFrameImage.color;
        }

        if (overlay != null)
            overlay.SetActive(false);
    }

    // =========================================================
    // BUTTONS (OnClick)
    // =========================================================

    public void InfoPressed()
    {
        TutorialManager.Instance?.ShowGuide();
    }

    public void NextPressed()
    {
        TutorialManager.Instance?.NextStep();
    }

    public void BackPressed()
    {
        TutorialManager.Instance?.PreviousStep();
    }

    public void SkipPressed()
    {
        TutorialManager.Instance?.SkipCurrentTutorial();
    }

    // =========================================================
    // SCENE SETUP
    // =========================================================

    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null ||
            FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject(
            "EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        Debug.Log("GuidePanel: Added an EventSystem so the guide can be used.", eventSystem);
    }

    // =========================================================
    // INFO BUTTON
    // =========================================================

    /// <summary>Shows the i button unless this scene hides it or the guide is open.</summary>
    public void SetInfoButtonVisible(bool visible)
    {
        if (infoButton != null)
            infoButton.gameObject.SetActive(visible && showInfoButton && !IsOpen);
    }

    public bool IsOverInfoButton(Vector2 screenPoint)
    {
        return infoButton != null &&
               infoButton.gameObject.activeInHierarchy &&
               RectTransformUtility.RectangleContainsScreenPoint(infoButton, screenPoint, null);
    }

    // =========================================================
    // SHOW / HIDE
    // =========================================================

    public void Show(TutorialManager.TutorialDefinition tutorial, int stepIndex, bool isGuide)
    {
        if (tutorial == null || tutorial.steps == null || tutorial.steps.Count == 0)
            return;

        stepIndex = Mathf.Clamp(stepIndex, 0, tutorial.steps.Count - 1);
        TutorialManager.TutorialStep step = tutorial.steps[stepIndex];

        bool wasOpen = IsOpen;

        overlay.SetActive(true);
        if (infoButton != null)
            infoButton.gameObject.SetActive(false);

        bool isFirst = stepIndex == 0;
        bool isLast = stepIndex == tutorial.steps.Count - 1;
        int count = tutorial.steps.Count;

        titleText.text = step.title;
        messageText.text = step.message;
        pageText.text = count > 1 ? (stepIndex + 1) + " / " + count : string.Empty;

        nextLabel.text = isLast ? (isGuide ? "CLOSE" : "GOT IT") : "NEXT";
        backButton.gameObject.SetActive(!isFirst);
        skipLabel.text = isGuide ? "CLOSE" : "SKIP";
        skipButton.gameObject.SetActive(!isLast || isGuide);

        highlightTarget = step.showHighlight ? FindTarget(step.targetId) : null;

        LayoutWindow();
        UpdateSpotlight();

        if (!wasOpen)
        {
            openedAt = Time.unscaledTime;

            if (EventSystem.current != null)
                previousSelection = EventSystem.current.currentSelectedGameObject;
        }

        // Gamepad / keyboard users land on the main button.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
    }

    public void Hide()
    {
        if (overlay != null)
            overlay.SetActive(false);

        highlightTarget = null;

        // Hand keyboard / gamepad focus back to the scene.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                previousSelection != null && previousSelection.activeInHierarchy
                    ? previousSelection
                    : null);
        }

        previousSelection = null;
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        // Gentle pop-in, then keep the spotlight on its (possibly moving) target.
        float t = Mathf.Clamp01((Time.unscaledTime - openedAt) / 0.18f);
        float scale = Mathf.Lerp(0.92f, 1f, 1f - (1f - t) * (1f - t));
        window.localScale = new Vector3(scale, scale, 1f);

        UpdateSpotlight();

        if (highlightFrameImage.gameObject.activeSelf)
        {
            Color colour = highlightColour;
            colour.a = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 5f);
            highlightFrameImage.color = colour;
        }
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    // Only the heights change: the window grows to fit the page and the
    // message box (anchored to the window's top) matches it. Everything
    // else keeps the position it has in the scene.
    private void LayoutWindow()
    {
        float innerWidth = messageText.rectTransform.rect.width;

        messageText.enableAutoSizing = false;
        messageText.fontSize = messageFontSize;

        float messageHeight =
            messageText.GetPreferredValues(messageText.text, innerWidth, 0f).y + 8f;

        if (messageHeight > maxMessageHeight)
        {
            // Long page: shrink the text to fit rather than overflow.
            messageText.enableAutoSizing = true;
            messageText.fontSizeMin = 24f;
            messageText.fontSizeMax = messageFontSize;
            messageHeight = maxMessageHeight;
        }

        messageHeight = Mathf.Max(minMessageHeight, messageHeight);

        window.sizeDelta = new Vector2(window.sizeDelta.x, heightWithoutMessage + messageHeight);

        RectTransform messageRect = messageText.rectTransform;
        messageRect.sizeDelta = new Vector2(messageRect.sizeDelta.x, messageHeight);
    }

    // Window sits in the middle unless a highlighted target is there; then
    // it moves to whichever half of the screen leaves the target visible.
    private void PositionWindow(Rect targetLocal, bool hasTarget)
    {
        Vector2 canvasSize = canvasRect.rect.size;
        float halfHeight = window.sizeDelta.y * 0.5f;
        float limit = Mathf.Max(0f, canvasSize.y * 0.5f - halfHeight - 40f);

        float y = 0f;

        if (hasTarget)
        {
            bool targetInTopHalf = targetLocal.center.y > 0f;
            float gap = 40f;

            y = targetInTopHalf
                ? targetLocal.yMin - gap - halfHeight
                : targetLocal.yMax + gap + halfHeight;

            // If it can't fit beside the target, keep it on screen anyway.
            y = Mathf.Clamp(y, -limit, limit);
        }

        window.anchoredPosition = new Vector2(0f, y);
    }

    private void UpdateSpotlight()
    {
        Rect canvasBounds = canvasRect.rect;

        if (highlightTarget == null || !highlightTarget.gameObject.activeInHierarchy ||
            !TryGetLocalRect(highlightTarget, out Rect target))
        {
            highlightFrame.gameObject.SetActive(false);
            SetDim(dimPieces[0], canvasBounds);
            for (int i = 1; i < dimPieces.Length; i++)
                SetDim(dimPieces[i], Rect.zero);

            PositionWindow(Rect.zero, false);
            return;
        }

        target = new Rect(
            target.xMin - HighlightPadding, target.yMin - HighlightPadding,
            target.width + HighlightPadding * 2f, target.height + HighlightPadding * 2f);

        // Keep the spotlight on screen.
        target = Rect.MinMaxRect(
            Mathf.Max(canvasBounds.xMin, target.xMin), Mathf.Max(canvasBounds.yMin, target.yMin),
            Mathf.Min(canvasBounds.xMax, target.xMax), Mathf.Min(canvasBounds.yMax, target.yMax));

        // Above, below, left, right of the hole.
        SetDim(dimPieces[0], Rect.MinMaxRect(canvasBounds.xMin, target.yMax, canvasBounds.xMax, canvasBounds.yMax));
        SetDim(dimPieces[1], Rect.MinMaxRect(canvasBounds.xMin, canvasBounds.yMin, canvasBounds.xMax, target.yMin));
        SetDim(dimPieces[2], Rect.MinMaxRect(canvasBounds.xMin, target.yMin, target.xMin, target.yMax));
        SetDim(dimPieces[3], Rect.MinMaxRect(target.xMax, target.yMin, canvasBounds.xMax, target.yMax));

        highlightFrame.gameObject.SetActive(true);
        highlightFrame.anchoredPosition = target.center;
        highlightFrame.sizeDelta = target.size + Vector2.one * 12f;

        PositionWindow(target, true);
    }

    private static void SetDim(RectTransform piece, Rect rect)
    {
        piece.anchorMin = piece.anchorMax = new Vector2(0.5f, 0.5f);
        piece.pivot = new Vector2(0.5f, 0.5f);
        piece.anchoredPosition = rect.center;
        piece.sizeDelta = new Vector2(Mathf.Max(0f, rect.width), Mathf.Max(0f, rect.height));
    }

    // =========================================================
    // TARGETS
    // =========================================================

    /// <summary>
    /// A TutorialTarget with this id, or else an active scene object with
    /// this name. Lets steps point at any button without scene changes.
    /// </summary>
    private RectTransform FindTarget(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            return null;

        TutorialTarget[] targets = FindObjectsByType<TutorialTarget>(FindObjectsSortMode.None);

        foreach (TutorialTarget target in targets)
        {
            if (target != null && target.isActiveAndEnabled &&
                string.Equals(target.TargetId, targetId, System.StringComparison.OrdinalIgnoreCase) &&
                target.RectTransform != null)
            {
                return target.RectTransform;
            }
        }

        GameObject named = GameObject.Find(targetId);

        if (named != null && !named.transform.IsChildOf(transform))
            return named.GetComponent<RectTransform>();

        return null;
    }

    // Screen bounds of everything visible under the target. Containers in
    // this project are often 100x100 with scaled children, so the target's
    // own rect alone can be far smaller than what the player sees.
    private bool TryGetLocalRect(RectTransform target, out Rect local)
    {
        local = Rect.zero;

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        bool found = false;

        Graphic[] graphics = target.GetComponentsInChildren<Graphic>(false);
        Vector3[] corners = new Vector3[4];

        foreach (Graphic graphic in graphics)
        {
            if (!graphic.enabled || graphic.color.a <= 0.01f)
                continue;

            Canvas owner = graphic.canvas;
            Camera camera = owner != null && owner.renderMode != RenderMode.ScreenSpaceOverlay
                ? owner.worldCamera
                : null;

            graphic.rectTransform.GetWorldCorners(corners);

            for (int i = 0; i < 4; i++)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
                min = Vector2.Min(min, screen);
                max = Vector2.Max(max, screen);
            }

            found = true;
        }

        if (!found)
            return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, min, null, out Vector2 localMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, max, null, out Vector2 localMax);

        local = Rect.MinMaxRect(localMin.x, localMin.y, localMax.x, localMax.y);
        return local.width > 1f && local.height > 1f;
    }
}
