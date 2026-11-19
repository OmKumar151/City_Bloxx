using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gameplay behaviour for the settings panel copied from LevelSelect:
/// pauses the game while the panel is open, keeps taps on the settings
/// button/panel from dropping a block, hides the button when there is no
/// run to pause, and handles the panel's Exit button.
///
/// Sits next to SettingsPanelUI on the SettingsController object. The
/// panel opens/closes through SettingsPanelUI as in LevelSelect; this
/// only watches whether it is open.
/// </summary>
public class GameplaySettingsUI : MonoBehaviour
{
    public static GameplaySettingsUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private SettingsPanelUI settings;
    [SerializeField] private GameObject settingsPanel;
    [Tooltip("The button that opens the panel. Hidden when there's no run to pause.")]
    [SerializeField] private RectTransform settingsButton;

    [Header("Exit Button")]
    [SerializeField] private TMP_Text exitLabel;
    [SerializeField] private string exitText = "Exit";
    [Tooltip("Shown after the first tap; a second tap leaves.")]
    [SerializeField] private string confirmText = "Sure?";
    [SerializeField, Min(0.5f)] private float confirmSeconds = 3f;

    // Taps just after closing are ignored, so the Close tap doesn't also
    // drop a block.
    private const float InputGraceSeconds = 0.25f;

    private bool paused;
    private float timeScaleBeforePause = 1f;
    private float lastClosedTime = -10f;
    private float exitArmedUntil = -1f;

    private bool IsOpen => settingsPanel != null && settingsPanel.activeInHierarchy;

    private void Awake()
    {
        Instance = this;

        if (settings == null)
            settings = GetComponent<SettingsPanelUI>();
    }

    private void Start()
    {
        DisarmExit();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        // Never leave the next scene frozen.
        if (paused)
            Time.timeScale = 1f;
    }

    private void Update()
    {
        // Android's back button arrives as Escape.
        if (settings != null && Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen)
                settings.CloseSettings();
            else if (CanPause())
                settings.OpenSettings();
        }

        bool open = IsOpen;

        if (open && !paused)
            Pause();
        else if (!open && paused)
            Resume();

        if (settingsButton != null)
            settingsButton.gameObject.SetActive(open || CanPause());

        if (exitArmedUntil > 0f && Time.unscaledTime > exitArmedUntil)
            DisarmExit();
    }

    // Only mid-run: not behind the Quick Play picker, the win/lose
    // panels, or an open tutorial (all of which already pause the game).
    private bool CanPause()
    {
        GameManager game = GameManager.Instance;

        if (game == null || game.isGameOver || game.isBuildingComplete)
            return false;

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialOpen)
            return false;

        return Time.timeScale > 0f;
    }

    private void Pause()
    {
        paused = true;
        timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        DisarmExit();
    }

    private void Resume()
    {
        paused = false;
        lastClosedTime = Time.unscaledTime;

        // Leaving the scene resets time itself; don't undo that.
        GameManager game = GameManager.Instance;
        if (game == null || (!game.isGameOver && !game.isBuildingComplete))
            Time.timeScale = timeScaleBeforePause;
    }

    /// <summary>
    /// True when a tap at this screen point belongs to the settings UI and
    /// must not drop a block. (Canvas is Screen Space - Overlay.)
    /// </summary>
    public bool BlocksGameplayInput(Vector2 screenPoint)
    {
        if (IsOpen || paused || Time.unscaledTime - lastClosedTime < InputGraceSeconds)
            return true;

        return settingsButton != null &&
               settingsButton.gameObject.activeInHierarchy &&
               RectTransformUtility.RectangleContainsScreenPoint(settingsButton, screenPoint, null);
    }

    /// <summary>
    /// Exit button (OnClick). First tap asks "Sure?", the second leaves:
    /// a building goes back to the city (and is lost), Quick Play goes to
    /// the main menu and keeps its best score.
    /// </summary>
    public void ExitGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (Time.unscaledTime > exitArmedUntil)
        {
            exitArmedUntil = Time.unscaledTime + confirmSeconds;

            if (exitLabel != null)
                exitLabel.text = confirmText;

            return;
        }

        GameManager game = GameManager.Instance;
        if (game == null)
            return;

        if (game.IsEndless && game.score > 0)
            QuickPlayScores.Record(game.score, out _);

        paused = false;
        game.ReturnToLevelSelect();
    }

    private void DisarmExit()
    {
        exitArmedUntil = -1f;

        if (exitLabel != null)
            exitLabel.text = exitText;
    }
}
