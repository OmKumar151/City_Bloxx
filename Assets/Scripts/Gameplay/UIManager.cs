
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Lives UI")]
    [Tooltip("Number of lives remaining (e.g. the Text under Lives).")]
    public TMP_Text livesText;

    [Tooltip("Full hearts, first life first. A lost life hides its heart " +
             "so the empty heart behind it shows. Sprites are never tinted.")]
    [SerializeField] private List<Image> lifeIcons = new List<Image>();

    [Header("Floor UI")]
    [SerializeField] private TMP_Text floorText;

    [Header("Game Over (Fail) UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [Tooltip("Score shown on the fail panel.")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverLevelSelectButton;

    [Header("Building Complete (Win) UI")]
    [SerializeField] private GameObject completePanel;
    [Tooltip("Score shown on the win panel.")]
    [SerializeField] private TMP_Text completeFinalScoreText;
    [SerializeField] private Button completeRestartButton;
    [Tooltip("Adds the score to the city's population and returns to LevelSelect.")]
    [SerializeField] private Button completeNextButton;

    [Header("Score Format")]
    [SerializeField] private string scorePrefix = "Score: ";

    [Header("End Panel Animation")]
    [Tooltip("Seconds for the win / game over panel to fade in.")]
    [SerializeField, Min(0f)] private float panelFadeDuration = 0.4f;

    [Tooltip("Size the panel starts at while fading in (1 = no scale-up).")]
    [SerializeField, Range(0.5f, 1f)] private float panelStartScale = 0.9f;

    private int displayedScore;
    private Coroutine panelFadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        WireEndPanelButtons();
        HideEndPanels();
    }

    private void WireEndPanelButtons()
    {
        AddClick(gameOverRestartButton, OnRestartClicked);
        AddClick(gameOverLevelSelectButton, OnLevelSelectClicked);
        AddClick(completeRestartButton, OnRestartClicked);
        AddClick(completeNextButton, OnNextClicked);
    }

    private static void AddClick(
        Button button,
        UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void OnRestartClicked()
    {
        PlayButtonSound();

        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void OnNextClicked()
    {
        PlayButtonSound();

        if (GameManager.Instance != null)
            GameManager.Instance.BankScoreAndReturnToLevelSelect();
    }

    private void OnLevelSelectClicked()
    {
        PlayButtonSound();

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToLevelSelect();
    }

    private static void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMajorButton();
    }

    private string FormatScore(int score)
    {
        return scorePrefix + Mathf.Max(0, score).ToString("N0");
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateScore(GameManager.Instance.score);
            UpdateLives(GameManager.Instance.lives);
            UpdateFloors(
                GameManager.Instance.floorsBuilt,
                GameManager.Instance.floorsToComplete
            );
        }
    }

    public void UpdateScore(int score)
    {
        displayedScore = Mathf.Max(0, score);

        if (scoreText != null)
            scoreText.text = displayedScore.ToString("N0");
    }

    public void UpdateLives(int lives)
    {
        lives = Mathf.Max(0, lives);

        if (livesText != null)
            livesText.text = lives.ToString();

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] == null)
                continue;

            // Show or hide only; keep the sprite's own colours.
            lifeIcons[i].color = Color.white;
            lifeIcons[i].enabled = i < lives;
        }
    }

    public void UpdateFloors(int floorsBuilt, int floorsToComplete)
    {
        if (floorText != null)
        {
            floorText.text =
                Mathf.Max(0, floorsBuilt) + " / " +
                Mathf.Max(1, floorsToComplete);
        }
    }

    // Kept for compatibility with any older scripts that call UpdateCombo.
    // Combo information is intentionally not displayed to the player.
    public void UpdateCombo(int comboCount, int maxCombo, bool wasPerfect)
    {
        // Intentionally empty.
    }

    public void HideEndPanels()
    {
        StopPanelFade();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (completePanel != null)
            completePanel.SetActive(false);
    }

    public void ShowGameOver(int finalScore)
    {
        if (completePanel != null)
            completePanel.SetActive(false);

        FadeInPanel(gameOverPanel);

        if (gameOverText != null)
            gameOverText.text = "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = FormatScore(finalScore);

        SelectFirstButton(gameOverRestartButton);
    }

    public void ShowBuildingComplete(int finalScore)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        FadeInPanel(completePanel);

        if (completeFinalScoreText != null)
            completeFinalScoreText.text = FormatScore(finalScore);

        SelectFirstButton(completeNextButton);
    }

    // =========================================================
    // END PANEL FADE
    // =========================================================

    private void FadeInPanel(GameObject panel)
    {
        if (panel == null)
            return;

        StopPanelFade();

        panel.SetActive(true);

        if (panelFadeDuration <= 0f || !isActiveAndEnabled)
            return;

        panelFadeRoutine = StartCoroutine(FadeInRoutine(panel));
    }

    // Unscaled time: the game is paused (timeScale 0) while panels show.
    private System.Collections.IEnumerator FadeInRoutine(GameObject panel)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();

        if (group == null)
            group = panel.AddComponent<CanvasGroup>();

        Transform panelTransform = panel.transform;
        Vector3 endScale = Vector3.one;
        Vector3 startScale = endScale * panelStartScale;

        // No clicks until the panel is fully in.
        group.interactable = false;
        group.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < panelFadeDuration)
        {
            float t = Mathf.Clamp01(elapsed / panelFadeDuration);

            // Ease out: quick at first, settling gently.
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            group.alpha = eased;
            panelTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        panelTransform.localScale = endScale;

        panelFadeRoutine = null;
    }

    // Stops a fade part-way and leaves both panels fully visible-ready, so
    // a panel shown again never stays half transparent.
    private void StopPanelFade()
    {
        if (panelFadeRoutine != null)
        {
            StopCoroutine(panelFadeRoutine);
            panelFadeRoutine = null;
        }

        ResetPanelVisuals(gameOverPanel);
        ResetPanelVisuals(completePanel);
    }

    private static void ResetPanelVisuals(GameObject panel)
    {
        if (panel == null)
            return;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
        }

        panel.transform.localScale = Vector3.one;
    }

    // Lets keyboard / controller players press a button straight away.
    private static void SelectFirstButton(Button button)
    {
        if (button != null &&
            UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current
                .SetSelectedGameObject(button.gameObject);
        }
    }
}
