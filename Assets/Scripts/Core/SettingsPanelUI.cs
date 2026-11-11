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

    [Header("Haptics")]
    [Tooltip("On = vibration enabled.")]
    [SerializeField] private Toggle vibrationToggle;

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

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(
                OnVibrationToggleChanged
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

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.RemoveListener(
                OnVibrationToggleChanged
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
    }

    public void OnVibrationToggleChanged(bool value)
    {
        Haptics.Enabled = value;

        // A short buzz so the player feels it was switched on.
        if (value)
            Haptics.BlockLanded(0f, false);
    }

    private void LoadCurrentAudioSettings()
    {
        // Haptics don't depend on AudioManager, so load them first.
        if (vibrationToggle != null)
            vibrationToggle.SetIsOnWithoutNotify(Haptics.Enabled);

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

        Haptics.Enabled = true;

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