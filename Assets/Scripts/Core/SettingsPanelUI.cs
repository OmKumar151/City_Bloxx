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

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Start()
    {
        // The SettingsPanelUI script should NOT be attached
        // to the Settings Panel itself.
        //
        // It should be attached to a separate always-active
        // SettingsController GameObject.

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        LoadCurrentAudioSettings();
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

        // Load the latest saved values.
        LoadCurrentAudioSettings();

        // Open the panel.
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

    public void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        AudioManager.Instance.SetMusicVolume(value);
    }

    // =========================================================
    // SFX SLIDER
    // =========================================================

    public void OnSFXSliderChanged(float value)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "SettingsPanelUI: AudioManager is not available."
            );

            return;
        }

        AudioManager.Instance.SetSFXVolume(value);
    }

    // =========================================================
    // MUTE TOGGLE
    // =========================================================

    public void OnMuteToggleChanged(bool value)
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
            "SettingsPanelUI: Settings reset."
        );
    }
}