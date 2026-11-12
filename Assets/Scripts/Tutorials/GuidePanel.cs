using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// The tutorial / game-guide window and the "i" button, built in code so
/// they work the same in every scene with no scene setup. Lives on the
/// persistent TutorialManager. Text sizes itself to the page, and a step
/// with a target spotlights that object (everything else is dimmed).
/// </summary>
public class GuidePanel : MonoBehaviour
{
    // Palette matched to the game's blue panels and gold trim.
    private static readonly Color WindowColour = new Color32(0x14, 0x37, 0x72, 0xFF);
    private static readonly Color TrimColour = new Color32(0xF2, 0xB8, 0x3B, 0xFF);
    private static readonly Color TitleColour = new Color32(0xFF, 0xD9, 0x6A, 0xFF);
    private static readonly Color NextColour = new Color32(0xF2, 0xB8, 0x3B, 0xFF);
    private static readonly Color NextTextColour = new Color32(0x3A, 0x22, 0x05, 0xFF);
    private static readonly Color BackColour = new Color32(0x2D, 0x66, 0xC4, 0xFF);
    private static readonly Color DimColour = new Color(0f, 0f, 0f, 0.68f);
    private static readonly Color InfoColour = new Color32(0x1F, 0x6F, 0xD6, 0xFF);

    private const float WindowWidth = 940f;
    private const float Padding = 56f;
    private const float TitleHeight = 76f;
    private const float FooterHeight = 120f;
    private const float MinMessageHeight = 160f;
    private const float MaxMessageHeight = 980f;
    private const float MessageFontSize = 38f;
    private const float HighlightPadding = 18f;

    private Canvas canvas;
    private CanvasScaler scaler;
    private RectTransform canvasRect;

    private GameObject overlay;
    private RectTransform[] dimPieces;
    private RectTransform highlightFrame;
    private Image highlightFrameImage;

    private RectTransform window;
    private TMP_Text titleText;
    private TMP_Text messageText;
    private TMP_Text pageText;
    private Button nextButton;
    private TMP_Text nextLabel;
    private Button backButton;
    private Button skipButton;
    private TMP_Text skipLabel;

    private RectTransform infoButton;

    private RectTransform highlightTarget;
    private float openedAt;
    private GameObject previousSelection;

    public bool IsOpen => overlay != null && overlay.activeSelf;

    // =========================================================
    // CONSTRUCTION
    // =========================================================

    public static GuidePanel Create(Transform parent, TMP_FontAsset font)
    {
        GameObject root = new GameObject("GuidePanel", typeof(RectTransform));
        root.transform.SetParent(parent, false);

        GuidePanel panel = root.AddComponent<GuidePanel>();
        panel.Build(font);
        return panel;
    }

    private void Build(TMP_FontAsset font)
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        gameObject.AddComponent<GraphicRaycaster>();
        canvasRect = (RectTransform)transform;

        if (font == null)
            font = TMP_Settings.defaultFontAsset;

        BuildInfoButton(font);
        BuildOverlay(font);

        overlay.SetActive(false);
        infoButton.gameObject.SetActive(false);
    }

    private void BuildOverlay(TMP_FontAsset font)
    {
        overlay = NewObject("Overlay", transform);
        Stretch(overlay.GetComponent<RectTransform>());

        // Invisible full-screen blocker: nothing behind the guide can be
        // pressed, even inside the spotlight hole.
        Image blocker = overlay.AddComponent<Image>();
        blocker.color = new Color(0f, 0f, 0f, 0f);
        blocker.raycastTarget = true;

        // Four dim pieces around the spotlight (one covers all without a target).
        dimPieces = new RectTransform[4];

        for (int i = 0; i < dimPieces.Length; i++)
        {
            GameObject piece = NewObject("Dim" + i, overlay.transform);
            Image image = piece.AddComponent<Image>();
            image.color = DimColour;
            image.raycastTarget = false;
            dimPieces[i] = piece.GetComponent<RectTransform>();
        }

        GameObject frame = NewObject("Highlight", overlay.transform);
        highlightFrameImage = frame.AddComponent<Image>();
        highlightFrameImage.sprite = GuideSprites.RoundedRing;
        highlightFrameImage.type = Image.Type.Sliced;
        highlightFrameImage.pixelsPerUnitMultiplier = 0.8f;
        highlightFrameImage.color = TrimColour;
        highlightFrameImage.raycastTarget = false;
        highlightFrame = frame.GetComponent<RectTransform>();
        Centre(highlightFrame);

        // Window
        GameObject windowObject = NewObject("Window", overlay.transform);
        window = windowObject.GetComponent<RectTransform>();
        Centre(window);

        Image windowImage = windowObject.AddComponent<Image>();
        windowImage.sprite = GuideSprites.RoundedFill;
        windowImage.type = Image.Type.Sliced;
        windowImage.pixelsPerUnitMultiplier = 0.9f;
        windowImage.color = WindowColour;

        GameObject trim = NewObject("Trim", window);
        Stretch(trim.GetComponent<RectTransform>());
        Image trimImage = trim.AddComponent<Image>();
        trimImage.sprite = GuideSprites.RoundedRing;
        trimImage.type = Image.Type.Sliced;
        trimImage.pixelsPerUnitMultiplier = 0.9f;
        trimImage.color = TrimColour;
        trimImage.raycastTarget = false;

        titleText = NewText("Title", window, font, 54f, TitleColour, FontStyles.Bold);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 34f;
        titleText.fontSizeMax = 54f;

        messageText = NewText("Message", window, font, MessageFontSize, Color.white, FontStyles.Normal);
        messageText.alignment = TextAlignmentOptions.TopLeft;
        messageText.lineSpacing = 6f;

        pageText = NewText("Page", window, font, 32f, new Color(1f, 1f, 1f, 0.7f), FontStyles.Normal);
        pageText.alignment = TextAlignmentOptions.Center;

        backButton = NewButton("Back", window, font, "BACK", BackColour, Color.white, out _);
        nextButton = NewButton("Next", window, font, "NEXT", NextColour, NextTextColour, out nextLabel);

        skipButton = NewTextButton("Skip", window, font, "SKIP", out skipLabel);

        backButton.onClick.AddListener(() => TutorialManager.Instance?.PreviousStep());
        nextButton.onClick.AddListener(() => TutorialManager.Instance?.NextStep());
        skipButton.onClick.AddListener(() => TutorialManager.Instance?.SkipCurrentTutorial());
    }

    private void BuildInfoButton(TMP_FontAsset font)
    {
        GameObject buttonObject = NewObject("InfoButton", transform);
        infoButton = buttonObject.GetComponent<RectTransform>();

        Image circle = buttonObject.AddComponent<Image>();
        circle.sprite = GuideSprites.Circle;
        circle.color = InfoColour;

        GameObject ring = NewObject("Ring", infoButton);
        Stretch(ring.GetComponent<RectTransform>());
        Image ringImage = ring.AddComponent<Image>();
        ringImage.sprite = GuideSprites.CircleRing;
        ringImage.color = TrimColour;
        ringImage.raycastTarget = false;

        TMP_Text label = NewText("Label", infoButton, font, 64f, Color.white, FontStyles.Bold);
        Stretch(label.rectTransform);
        label.alignment = TextAlignmentOptions.Center;
        label.text = "i";
        label.enableAutoSizing = true;
        label.fontSizeMin = 20f;
        label.fontSizeMax = 90f;
        label.margin = new Vector4(0f, 8f, 0f, 8f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = circle;
        button.onClick.AddListener(() => TutorialManager.Instance?.ShowGuide());
    }

    // =========================================================
    // SCENE SETUP
    // =========================================================

    /// <summary>Match the scene's canvas scaling so the "i" button lines up with its UI.</summary>
    public void MatchSceneScaling()
    {
        CanvasScaler[] scalers = FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);

        foreach (CanvasScaler candidate in scalers)
        {
            if (candidate == scaler ||
                candidate.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                continue;
            }

            Canvas candidateCanvas = candidate.GetComponent<Canvas>();

            if (candidateCanvas == null || !candidateCanvas.isRootCanvas)
                continue;

            scaler.referenceResolution = candidate.referenceResolution;
            scaler.screenMatchMode = candidate.screenMatchMode;
            scaler.matchWidthOrHeight = candidate.matchWidthOrHeight;
            return;
        }
    }

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

    public void SetInfoButton(bool visible, Vector2 anchor, Vector2 position, float size)
    {
        if (infoButton == null)
            return;

        infoButton.anchorMin = anchor;
        infoButton.anchorMax = anchor;
        infoButton.pivot = new Vector2(0.5f, 0.5f);
        infoButton.anchoredPosition = position;
        infoButton.sizeDelta = new Vector2(size, size);
        infoButton.gameObject.SetActive(visible && !IsOpen);
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
        infoButton.gameObject.SetActive(false);
        transform.SetAsLastSibling();

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
            Color colour = TrimColour;
            colour.a = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 5f);
            highlightFrameImage.color = colour;
        }
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    private void LayoutWindow()
    {
        float innerWidth = WindowWidth - Padding * 2f;

        messageText.enableAutoSizing = false;
        messageText.fontSize = MessageFontSize;

        float messageHeight =
            messageText.GetPreferredValues(messageText.text, innerWidth, 0f).y + 8f;

        if (messageHeight > MaxMessageHeight)
        {
            // Long page: shrink the text to fit rather than overflow.
            messageText.enableAutoSizing = true;
            messageText.fontSizeMin = 24f;
            messageText.fontSizeMax = MessageFontSize;
            messageHeight = MaxMessageHeight;
        }

        messageHeight = Mathf.Max(MinMessageHeight, messageHeight);

        float height = Padding + TitleHeight + 24f + messageHeight + 24f + FooterHeight + Padding * 0.6f;
        window.sizeDelta = new Vector2(WindowWidth, height);

        float top = height * 0.5f - Padding;

        Place(titleText.rectTransform, 0f, top - TitleHeight * 0.5f, innerWidth - 220f, TitleHeight);
        Place(messageText.rectTransform, 0f, top - TitleHeight - 24f - messageHeight * 0.5f,
            innerWidth, messageHeight);

        float footerY = -height * 0.5f + Padding * 0.6f + FooterHeight * 0.5f;
        float buttonWidth = 250f;

        Place((RectTransform)backButton.transform, -innerWidth * 0.5f + buttonWidth * 0.5f, footerY,
            buttonWidth, 100f);
        Place((RectTransform)nextButton.transform, innerWidth * 0.5f - buttonWidth * 0.5f, footerY,
            buttonWidth, 100f);
        Place(pageText.rectTransform, 0f, footerY, 220f, 60f);

        Place((RectTransform)skipButton.transform, WindowWidth * 0.5f - 105f, height * 0.5f - 52f,
            170f, 70f);
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

    // =========================================================
    // UI HELPERS
    // =========================================================

    private static GameObject NewObject(string name, Transform parent)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.layer = 5; // UI
        child.transform.SetParent(parent, false);
        return child;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Centre(RectTransform rect)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        Centre(rect);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static TMP_Text NewText(
        string name, Transform parent, TMP_FontAsset font, float size, Color colour, FontStyles style)
    {
        GameObject textObject = NewObject(name, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();

        if (font != null)
            text.font = font;

        text.fontSize = size;
        text.color = colour;
        text.fontStyle = style;
        text.richText = true;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static Button NewButton(
        string name, Transform parent, TMP_FontAsset font, string label,
        Color colour, Color textColour, out TMP_Text labelText)
    {
        GameObject buttonObject = NewObject(name, parent);

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GuideSprites.RoundedFill;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 0.65f;
        image.color = colour;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        labelText = NewText("Label", buttonObject.transform, font, 40f, textColour, FontStyles.Bold);
        Stretch(labelText.rectTransform);
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.text = label;

        return button;
    }

    private static Button NewTextButton(
        string name, Transform parent, TMP_FontAsset font, string label, out TMP_Text labelText)
    {
        GameObject buttonObject = NewObject(name, parent);

        // Transparent hit area so the small label is easy to tap.
        Image hitArea = buttonObject.AddComponent<Image>();
        hitArea.color = new Color(0f, 0f, 0f, 0f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = hitArea;

        labelText = NewText("Label", buttonObject.transform, font, 32f,
            new Color(1f, 1f, 1f, 0.75f), FontStyles.Bold);
        Stretch(labelText.rectTransform);
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.text = label;

        return button;
    }

    // =========================================================
    // SPRITES (generated, so no art needs to be assigned)
    // =========================================================

    private static class GuideSprites
    {
        private static Sprite roundedFill;
        private static Sprite roundedRing;
        private static Sprite circle;
        private static Sprite circleRing;

        private const int RoundedSize = 96;
        private const float RoundedRadius = 32f;
        private const float RingThickness = 6f;

        public static Sprite RoundedFill =>
            roundedFill != null ? roundedFill : roundedFill = MakeRounded(false);

        public static Sprite RoundedRing =>
            roundedRing != null ? roundedRing : roundedRing = MakeRounded(true);

        public static Sprite Circle =>
            circle != null ? circle : circle = MakeCircle(false);

        public static Sprite CircleRing =>
            circleRing != null ? circleRing : circleRing = MakeCircle(true);

        private static Sprite MakeRounded(bool ring)
        {
            Texture2D texture = NewTexture(RoundedSize);
            Color32[] pixels = new Color32[RoundedSize * RoundedSize];
            float half = RoundedSize * 0.5f;

            for (int y = 0; y < RoundedSize; y++)
            {
                for (int x = 0; x < RoundedSize; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f - half, y + 0.5f - half);
                    Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) -
                                new Vector2(half - RoundedRadius, half - RoundedRadius);

                    float distance =
                        Vector2.Max(q, Vector2.zero).magnitude +
                        Mathf.Min(Mathf.Max(q.x, q.y), 0f) - RoundedRadius;

                    pixels[y * RoundedSize + x] = Pixel(distance, ring);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            float border = RoundedRadius + 2f;

            return Sprite.Create(
                texture, new Rect(0, 0, RoundedSize, RoundedSize), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite MakeCircle(bool ring)
        {
            const int size = 128;
            Texture2D texture = NewTexture(size);
            Color32[] pixels = new Color32[size * size];
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance =
                        new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude - (half - 1f);

                    pixels[y * size + x] = Pixel(distance, ring, ring ? 8f : RingThickness);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        // distance < 0 is inside the shape. Edges are anti-aliased.
        private static Color32 Pixel(float distance, bool ring, float thickness = RingThickness)
        {
            float alpha = Mathf.Clamp01(0.5f - distance);

            if (ring)
                alpha *= Mathf.Clamp01(distance + thickness + 0.5f);

            return new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
        }

        private static Texture2D NewTexture(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };
        }
    }
}
