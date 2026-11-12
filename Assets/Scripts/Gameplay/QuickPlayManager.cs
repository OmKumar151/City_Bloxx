
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quick Play (main menu): when Gameplay1 opens without a building, the
/// crane is held and a picker with one button per building colour is shown.
/// Choosing a colour reloads the scene with that building on endless floors;
/// the game then runs until the last life is lost, and the fail panel shows
/// the score.
///
/// Assign your own panel and buttons to style the picker, or leave them
/// empty and a simple one is built at runtime.
/// </summary>
public class QuickPlayManager : MonoBehaviour
{
    [Header("Buildings")]
    [Tooltip("The colours offered, in button order.")]
    public List<BuildingGameplayConfig> buildings = new List<BuildingGameplayConfig>();

    [Header("Custom Picker (optional)")]
    [Tooltip("Your own picker panel. Leave empty to build one at runtime.")]
    public GameObject pickerPanel;

    [Tooltip("Buttons on the custom panel, in the same order as Buildings.")]
    public List<Button> colourButtons = new List<Button>();

    [Tooltip("Optional button on the custom panel that returns to the main menu.")]
    public Button backButton;

    [Header("Generated Picker")]
    public string title = "CHOOSE A BUILDING";

    [Tooltip("Whole-button art per building (e.g. the top bar icons), in " +
             "the same order as Buildings. A building without one uses the " +
             "card background with its roof sprite on top.")]
    public List<Sprite> buttonIcons = new List<Sprite>();

    [Tooltip("Drawn behind each building on its button (Level Select's " +
             "'Building background'). Empty = plain coloured cards.")]
    public Sprite cardBackgroundSprite;

    [Tooltip("Art for the generated Back button (already says 'Back'). " +
             "Empty = plain white button with a BACK label.")]
    public Sprite backButtonSprite;
    public Color overlayColor = new Color(0f, 0f, 0f, 0.7f);
    public Vector2 referenceResolution = new Vector2(1080f, 1920f);
    [Tooltip("Draw order of the picker canvas, above the game HUD.")]
    public int canvasSortingOrder = 500;

    private bool isLeaving;

    private void Start()
    {
        bool needsColour =
            GameplaySelection.IsQuickPlay && !GameplaySelection.HasSelection;

        if (!needsColour)
        {
            if (pickerPanel != null)
                pickerPanel.SetActive(false);

            return;
        }

        ShowPicker();
    }

    // =========================================================
    // PICKER
    // =========================================================

    private void ShowPicker()
    {
        // Hold the game behind the picker: no block can be dropped and the
        // camera intro waits.
        if (CraneController.Instance != null)
            CraneController.Instance.StopCrane();

        Time.timeScale = 0f;

        EnsureEventSystem();

        if (pickerPanel != null)
            WireCustomPicker();
        else
            BuildPicker();
    }

    private void WireCustomPicker()
    {
        pickerPanel.SetActive(true);

        for (int i = 0; i < colourButtons.Count; i++)
        {
            Button button = colourButtons[i];

            if (button == null)
                continue;

            BuildingGameplayConfig config =
                i < buildings.Count ? buildings[i] : null;

            if (config == null)
            {
                Debug.LogWarning(
                    "QuickPlayManager: Colour button " + i +
                    " has no building in the Buildings list.",
                    button
                );

                button.interactable = false;
                continue;
            }

            button.onClick.AddListener(() => ChooseBuilding(config));
        }

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
    }

    private void ChooseBuilding(BuildingGameplayConfig config)
    {
        if (isLeaving || config == null)
            return;

        isLeaving = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMajorButton();

        GameplaySelection.SelectQuickPlayBuilding(config);

        Debug.Log(
            "QuickPlayManager: Endless " + config.displayName + ".",
            this
        );

        // Reload so every system starts with the chosen building.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void BackToMainMenu()
    {
        if (isLeaving)
            return;

        isLeaving = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
            return;
        }

        GameplaySelection.ClearSelection();
        SceneManager.LoadScene("Main Menu");
    }

    // =========================================================
    // GENERATED PICKER UI
    // =========================================================

    private void BuildPicker()
    {
        List<BuildingGameplayConfig> valid = new List<BuildingGameplayConfig>();

        foreach (BuildingGameplayConfig config in buildings)
        {
            if (config != null)
                valid.Add(config);
        }

        if (valid.Count == 0)
        {
            Debug.LogError(
                "QuickPlayManager: No buildings assigned. Add the building " +
                "configs to the Buildings list.",
                this
            );
        }

        // Canvas over everything.
        GameObject canvasObject = new GameObject(
            "QuickPlayPicker",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        // Expand: the whole layout always fits, whatever the screen shape.
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        pickerPanel = canvasObject;

        // Dim, click-blocking backdrop.
        RectTransform overlay = CreateUI("Overlay", canvasObject.transform);
        Stretch(overlay);
        overlay.gameObject.AddComponent<Image>().color = overlayColor;

        // Title.
        TextMeshProUGUI titleText = CreateText(
            "Title", overlay, title, 80f, Color.white);

        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(-80f, 160f);
        titleRect.anchoredPosition = new Vector2(0f, -260f);

        // 2 x 2 grid of colour cards.
        RectTransform grid = CreateUI("Buttons", overlay);
        grid.anchorMin = grid.anchorMax = new Vector2(0.5f, 0.5f);
        grid.sizeDelta = new Vector2(880f, 1120f);
        grid.anchoredPosition = new Vector2(0f, -20f);

        GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();

        // Square cards for the square background art, with room under each
        // row for the building's name.
        if (cardBackgroundSprite != null || buttonIcons.Exists(icon => icon != null))
        {
            layout.cellSize = new Vector2(400f, 400f);
            layout.spacing = new Vector2(60f, 150f);
        }
        else
        {
            layout.cellSize = new Vector2(420f, 540f);
            layout.spacing = new Vector2(40f, 40f);
        }

        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2;
        layout.childAlignment = TextAnchor.MiddleCenter;

        Button first = null;

        foreach (BuildingGameplayConfig config in valid)
        {
            Button card = CreateCard(grid, config);

            if (first == null)
                first = card;
        }

        // Back to the main menu.
        Button back;
        RectTransform backRect;

        if (backButtonSprite != null)
        {
            back = CreateButton("Back", overlay, null, Color.white, Color.black, 56f);
            Image backImage = (Image)back.targetGraphic;
            backImage.sprite = backButtonSprite;
            backImage.preserveAspect = true;

            backRect = (RectTransform)back.transform;
            backRect.sizeDelta = new Vector2(420f, 151f);
        }
        else
        {
            back = CreateButton(
                "Back", overlay, "BACK", new Color(1f, 1f, 1f, 0.9f), Color.black, 56f);

            backRect = (RectTransform)back.transform;
            backRect.sizeDelta = new Vector2(360f, 120f);
        }

        backRect.anchorMin = backRect.anchorMax = new Vector2(0.5f, 0f);
        backRect.pivot = new Vector2(0.5f, 0f);
        backRect.anchoredPosition = new Vector2(0f, 140f);
        back.onClick.AddListener(BackToMainMenu);

        if (first != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(first.gameObject);
    }

    // One button per colour with the building's name under it. The face is
    // the building's icon if it has one, else its roof sprite on the card
    // background (or on a block of its colour).
    private Button CreateCard(Transform parent, BuildingGameplayConfig config)
    {
        Sprite icon = GetButtonIcon(config);
        bool hasBackground = icon != null || cardBackgroundSprite != null;

        // With the background art the card keeps its own colours; without
        // it the card is a plain block of the building's colour.
        Color cardColor = hasBackground
            ? Color.white
            : GetAccentColor(config.buildingId);

        Button card = CreateButton(
            config.buildingId + "Button", parent, null, cardColor, Color.white, 0f);

        if (hasBackground)
        {
            Image cardImage = card.GetComponent<Image>();
            cardImage.sprite = icon != null ? icon : cardBackgroundSprite;
            cardImage.preserveAspect = true;
        }

        // The icon already shows the building. Otherwise the roof (top)
        // sprite, falling back to the floor, then the ground.
        Sprite preview =
            icon != null ? null :
            config.standardRoofSprite != null ? config.standardRoofSprite :
            config.floorSprite != null ? config.floorSprite :
            config.groundSprite;

        if (preview != null)
        {
            RectTransform imageRect = CreateUI("Preview", card.transform);
            imageRect.anchorMin = hasBackground
                ? new Vector2(0.18f, 0.18f)
                : new Vector2(0.1f, 0.28f);
            imageRect.anchorMax = hasBackground
                ? new Vector2(0.82f, 0.82f)
                : new Vector2(0.9f, 0.92f);
            imageRect.offsetMin = imageRect.offsetMax = Vector2.zero;

            Image image = imageRect.gameObject.AddComponent<Image>();
            image.sprite = preview;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        string label = string.IsNullOrWhiteSpace(config.displayName)
            ? config.buildingId
            : config.displayName;

        TextMeshProUGUI text = CreateText(
            "Label", card.transform, label.ToUpperInvariant(), 44f, Color.white);

        RectTransform textRect = text.rectTransform;

        if (hasBackground)
        {
            // Name sits just under the card.
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 0f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.sizeDelta = new Vector2(0f, 110f);
            textRect.anchoredPosition = new Vector2(0f, -15f);
        }
        else
        {
            textRect.anchorMin = new Vector2(0.05f, 0.04f);
            textRect.anchorMax = new Vector2(0.95f, 0.26f);
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        }

        text.enableAutoSizing = true;
        text.fontSizeMin = 24f;
        text.fontSizeMax = 44f;
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color32(0, 0, 0, 160);

        card.onClick.AddListener(() => ChooseBuilding(config));

        return card;
    }

    // Button Icons lines up with Buildings (including empty slots).
    private Sprite GetButtonIcon(BuildingGameplayConfig config)
    {
        int index = buildings.IndexOf(config);

        return index >= 0 && index < buttonIcons.Count
            ? buttonIcons[index]
            : null;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Color background,
        Color textColor,
        float fontSize)
    {
        RectTransform rect = CreateUI(name, parent);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = background;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        button.colors = colors;

        if (!string.IsNullOrEmpty(label))
        {
            TextMeshProUGUI text = CreateText("Label", rect, label, fontSize, textColor);
            Stretch(text.rectTransform);
        }

        return button;
    }

    private static TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Color color)
    {
        RectTransform rect = CreateUI(name, parent);

        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;

        return text;
    }

    private static RectTransform CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Color GetAccentColor(string buildingId)
    {
        switch ((buildingId ?? string.Empty).ToLowerInvariant())
        {
            case "blue": return new Color(0.22f, 0.5f, 0.92f);
            case "red": return new Color(0.88f, 0.27f, 0.27f);
            case "green": return new Color(0.27f, 0.7f, 0.33f);
            case "yellow": return new Color(0.96f, 0.76f, 0.16f);
            default: return new Color(0.4f, 0.4f, 0.45f);
        }
    }

    // The picker needs an EventSystem to receive clicks.
    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null || FindObjectOfType<EventSystem>() != null)
            return;

        new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule)
        );
    }
}
