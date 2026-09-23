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

    private bool listenersRegistered;

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        RegisterListeners();
    }

    private void Start()
    {
        // The SettingsPanelUI script should be on
        // SettingsController, NOT on SettingPanel.

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        LoadCurrentAudioSettings();
    }

    // =========================================================
    // REGISTER UI EVENTS
    // =========================================================

    private void RegisterListeners()
    {
        if (listenersRegistered)
            return;

        // -----------------------------------------------------
        // SAFETY CHECK
        // -----------------------------------------------------

        if (musicSlider != null &&
            sfxSlider != null &&
            musicSlider == sfxSlider)
        {
            Debug.LogError(
                "SettingsPanelUI: Music Slider and SFX Slider " +
                "are assigned to the SAME Slider. " +
                "They must be two separate Slider objects."
            );
        }

        // -----------------------------------------------------
        // MUSIC SLIDER
        // -----------------------------------------------------

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(
                OnMusicSliderChanged
            );
        }

        // -----------------------------------------------------
        // SFX SLIDER
        // -----------------------------------------------------

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(
                OnSFXSliderChanged
            );
        }

        // -----------------------------------------------------
        // MUTE TOGGLE
        // -----------------------------------------------------

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(
                OnMuteToggleChanged
            );
        }

        listenersRegistered = true;
    }

    // =========================================================
    // REMOVE UI EVENTS
    // =========================================================

    private void OnDestroy()
    {
        if (!listenersRegistered)
            return;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(
                OnMusicSliderChanged
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(
                OnSFXSliderChanged
            );
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.RemoveListener(
                OnMuteToggleChanged
            );
        }

        listenersRegistered = false;
    }

    // =========================================================
    // OPEN SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: Settings Panel is not assigned."
            );

            return;
        }

        LoadCurrentAudioSettings();

        settingsPanel.SetActive(true);

        Debug.Log(
            "SettingsPanelUI: Settings panel opened."
        );
    }

    // =========================================================
    // CLOSE SETTINGS
    // =========================================================

    public void CloseSettings()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(false);

        Debug.Log(
            "SettingsPanelUI: Settings panel closed."
        );
    }

    // =========================================================
    // MUSIC SLIDER
    // =========================================================

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        // ONLY changes MUSIC.
        AudioManager.Instance.SetMusicVolume(value);

        Debug.Log(
            "SettingsPanelUI: Music slider changed to " +
            value
        );
    }

    // =========================================================
    // SFX SLIDER
    // =========================================================

    private void OnSFXSliderChanged(float value)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        // ONLY changes SFX.
        AudioManager.Instance.SetSFXVolume(value);

        Debug.Log(
            "SettingsPanelUI: SFX slider changed to " +
            value
        );
    }

    // =========================================================
    // MUTE TOGGLE
    // =========================================================

    private void OnMuteToggleChanged(bool value)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        AudioManager.Instance.SetMuted(value);
    }

    // =========================================================
    // LOAD CURRENT SETTINGS
    // =========================================================

    private void LoadCurrentAudioSettings()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        // -----------------------------------------------------
        // MUSIC
        // -----------------------------------------------------

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(
                AudioManager.Instance.MusicVolume
            );
        }

        // -----------------------------------------------------
        // SFX
        // -----------------------------------------------------

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(
                AudioManager.Instance.SFXVolume
            );
        }

        // -----------------------------------------------------
        // MUTE
        // -----------------------------------------------------

        if (muteToggle != null)
        {
            muteToggle.SetIsOnWithoutNotify(
                AudioManager.Instance.IsMuted
            );
        }
    }

    // =========================================================
    // RESET SETTINGS
    // =========================================================

    public void ResetSettings()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        AudioManager.Instance.ResetAudioSettings();

        LoadCurrentAudioSettings();

        Debug.Log(
            "SettingsPanelUI: Audio settings reset."
        );
    }
}