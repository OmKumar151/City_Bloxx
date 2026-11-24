using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle muteToggle;

    [Tooltip("Faded while muted (the Music and SFX rows).")]
    [SerializeField] private CanvasGroup[] dimWhenMuted;

    [Range(0f, 1f)]
    [SerializeField] private float mutedAlpha = 0.45f;

    [Header("Haptics")]
    [Tooltip("Vibration strength: 0 = off, 1 = full.")]
    [SerializeField] private Slider vibrationSlider;

    [Tooltip("Shortest gap between test buzzes while dragging the slider.")]
    [Min(0f)]
    [SerializeField] private float vibrationPreviewInterval = 0.12f;

    private float lastVibrationPreview = -1f;

    private void Awake()
    {
        Debug.Log(
            "SettingsPanelUI Awake | AudioManager.Instance = " +
            (AudioManager.Instance != null)
        );

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(
                OnMusicSliderChanged
            );

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(
                OnSFXSliderChanged
            );

        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(
                OnMuteToggleChanged
            );

        if (vibrationSlider != null)
            vibrationSlider.onValueChanged.AddListener(
                OnVibrationSliderChanged
            );
    }

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Debug.Log(
            "SettingsPanelUI Start | AudioManager.Instance = " +
            (AudioManager.Instance != null)
        );

        LoadCurrentAudioSettings();
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(
                OnMusicSliderChanged
            );

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(
                OnSFXSliderChanged
            );

        if (muteToggle != null)
            muteToggle.onValueChanged.RemoveListener(
                OnMuteToggleChanged
            );

        if (vibrationSlider != null)
            vibrationSlider.onValueChanged.RemoveListener(
                OnVibrationSliderChanged
            );
    }

    public void OpenSettings()
    {
        Debug.Log(
            "SettingsPanelUI: OpenSettings called."
        );

        if (settingsPanel == null)
        {
            Debug.LogError(
                "SettingsPanelUI: Settings Panel is NOT assigned."
            );

            return;
        }

        Debug.Log(
            "AudioManager.Instance exists = " +
            (AudioManager.Instance != null)
        );

        PlayButtonSound();

        LoadCurrentAudioSettings();

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        Debug.Log(
            "SettingsPanelUI: CloseSettings called."
        );

        if (settingsPanel == null)
            return;

        PlayButtonSound();

        settingsPanel.SetActive(false);
    }

    public void OnMusicSliderChanged(float value)
    {
        Debug.Log(
            "SettingsPanelUI: Music slider changed to " +
            value
        );

        if (AudioManager.Instance == null)
        {
            Debug.LogError(
                "SettingsPanelUI: AudioManager.Instance is NULL."
            );

            return;
        }

        UnmuteForVolumeChange();
        AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        Debug.Log(
            "SettingsPanelUI: SFX slider changed to " +
            value
        );

        if (AudioManager.Instance == null)
        {
            Debug.LogError(
                "SettingsPanelUI: AudioManager.Instance is NULL."
            );

            return;
        }

        UnmuteForVolumeChange();
        AudioManager.Instance.SetSFXVolume(value);
    }

    public void OnMuteToggleChanged(bool value)
    {
        Debug.Log(
            "SettingsPanelUI: Mute toggle changed to " +
            value
        );

        if (AudioManager.Instance == null)
        {
            Debug.LogError(
                "SettingsPanelUI: AudioManager.Instance is NULL."
            );

            return;
        }

        AudioManager.Instance.SetMuted(value);
        RefreshMutedLook(value);

        // Let the player hear that sound is back.
        if (!value)
            PlayButtonSound();
    }

    public void OnVibrationSliderChanged(float value)
    {
        Haptics.Intensity = value;

        // A buzz at the new strength, not on every drag step.
        float now = Time.unscaledTime;

        if (lastVibrationPreview >= 0f &&
            now - lastVibrationPreview < vibrationPreviewInterval)
            return;

        lastVibrationPreview = now;
        Haptics.Preview();
    }

    // Moving a volume slider while muted turns sound back on, so the
    // player hears the change.
    private void UnmuteForVolumeChange()
    {
        if (AudioManager.Instance == null || !AudioManager.Instance.IsMuted)
            return;

        AudioManager.Instance.SetMuted(false);

        if (muteToggle != null)
            muteToggle.SetIsOnWithoutNotify(false);

        RefreshMutedLook(false);
    }

    private void RefreshMutedLook(bool muted)
    {
        if (dimWhenMuted == null)
            return;

        foreach (CanvasGroup group in dimWhenMuted)
        {
            if (group != null)
                group.alpha = muted ? mutedAlpha : 1f;
        }
    }

    private void LoadCurrentAudioSettings()
    {
        // Haptics don't depend on AudioManager, so load them first.
        if (vibrationSlider != null)
            vibrationSlider.SetValueWithoutNotify(Haptics.Intensity);

        if (AudioManager.Instance == null)
        {
            Debug.LogError(
                "SettingsPanelUI: Cannot load audio settings because AudioManager.Instance is NULL."
            );

            return;
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(
                AudioManager.Instance.MusicVolume
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(
                AudioManager.Instance.SFXVolume
            );
        }

        if (muteToggle != null)
        {
            muteToggle.SetIsOnWithoutNotify(
                AudioManager.Instance.IsMuted
            );
        }

        RefreshMutedLook(AudioManager.Instance.IsMuted);

        Debug.Log(
            "SettingsPanelUI: Current audio settings loaded."
        );
    }

    public void ResetSettings()
    {
        Debug.Log(
            "SettingsPanelUI: ResetSettings called."
        );

        if (AudioManager.Instance == null)
        {
            Debug.LogError(
                "SettingsPanelUI: AudioManager.Instance is NULL."
            );

            return;
        }

        PlayButtonSound();

        AudioManager.Instance.ResetAudioSettings();

        Haptics.Intensity = Haptics.DefaultIntensity;

        LoadCurrentAudioSettings();
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayButtonSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        AudioManager.Instance.PlayButtonClick();
    }
}