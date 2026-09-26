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

        LoadCurrentAudioSettings();

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        Debug.Log(
            "SettingsPanelUI: CloseSettings called."
        );

        if (settingsPanel != null)
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

    private void LoadCurrentAudioSettings()
    {
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

        AudioManager.Instance.ResetAudioSettings();

        LoadCurrentAudioSettings();
    }
}