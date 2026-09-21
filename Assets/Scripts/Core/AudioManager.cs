using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip cityMapMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip whooshSound;

    [Header("City Map Sound Effects")]
    [SerializeField] private AudioClip buildingPlacedSound;
    [SerializeField] private AudioClip buildingUnlockedSound;

    [Header("Game Sound Effects")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Default Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSFXVolume = 1f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MuteKey = "AudioMuted";

    private float musicVolume;
    private float sfxVolume;
    private bool isMuted;

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public bool IsMuted => isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
        LoadAudioSettings();
        ApplyAudioSettings();
    }

    private void Start()
    {
        PlayMainMenuMusic();
    }

    // =========================================================
    // AUDIO SOURCE SETUP
    // =========================================================

    private void SetupAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    // =========================================================
    // MUSIC
    // =========================================================

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayCityMapMusic()
    {
        PlayMusic(cityMapMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null)
        {
            Debug.LogError(
                "AudioManager: Music Source is not assigned."
            );
            return;
        }

        if (clip == null)
        {
            Debug.LogError(
                "AudioManager: Music Clip is not assigned."
            );
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();

        Debug.Log(
            "AudioManager: Playing music: " + clip.name
        );
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
    }

    // =========================================================
    // SOUND EFFECTS
    // =========================================================

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError(
                "AudioManager: SFX Source is not assigned."
            );
            return;
        }

        if (clip == null)
        {
            Debug.LogError(
                "AudioManager: SFX Clip is not assigned."
            );
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlaySelectionSound()
    {
        PlaySFX(selectionSound);
    }

    public void PlayConfirmSound()
    {
        PlaySFX(confirmSound);
    }

    public void PlayErrorSound()
    {
        PlaySFX(errorSound);
    }

    public void PlayWhooshSound()
    {
        PlaySFX(whooshSound);
    }

    public void PlayBuildingPlacedSound()
    {
        PlaySFX(buildingPlacedSound);
    }

    public void PlayBuildingUnlockedSound()
    {
        PlaySFX(buildingUnlockedSound);
    }

    public void PlayVictorySound()
    {
        PlaySFX(victorySound);
    }

    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound);
    }

    // =========================================================
    // VOLUME
    // =========================================================

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        ApplyMusicSettings();

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        ApplySFXSettings();

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.Save();
    }

    // =========================================================
    // MUTE
    // =========================================================

    public void SetMuted(bool muted)
    {
        isMuted = muted;

        ApplyAudioSettings();

        PlayerPrefs.SetInt(
            MuteKey,
            isMuted ? 1 : 0
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Audio Muted: " + isMuted
        );
    }

    public void ToggleMute()
    {
        SetMuted(!isMuted);
    }

    // =========================================================
    // APPLY SETTINGS
    // =========================================================

    private void ApplyAudioSettings()
    {
        ApplyMusicSettings();
        ApplySFXSettings();
    }

    private void ApplyMusicSettings()
    {
        if (musicSource == null)
            return;

        musicSource.volume = musicVolume;
        musicSource.mute = isMuted;
    }

    private void ApplySFXSettings()
    {
        if (sfxSource == null)
            return;

        sfxSource.volume = sfxVolume;
        sfxSource.mute = isMuted;
    }

    // =========================================================
    // LOAD SETTINGS
    // =========================================================

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(
            MusicVolumeKey,
            defaultMusicVolume
        );

        sfxVolume = PlayerPrefs.GetFloat(
            SFXVolumeKey,
            defaultSFXVolume
        );

        isMuted =
            PlayerPrefs.GetInt(
                MuteKey,
                0
            ) == 1;

        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
    }

    // =========================================================
    // RESET SETTINGS
    // =========================================================

    public void ResetAudioSettings()
    {
        musicVolume = defaultMusicVolume;
        sfxVolume = defaultSFXVolume;
        isMuted = false;

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.SetInt(
            MuteKey,
            0
        );

        PlayerPrefs.Save();

        ApplyAudioSettings();

        Debug.Log(
            "AudioManager: Audio settings reset."
        );
    }

    // =========================================================
    // TEST FUNCTIONS
    // =========================================================

    [ContextMenu("Test Main Menu Music")]
    private void TestMainMenuMusic()
    {
        PlayMainMenuMusic();
    }

    [ContextMenu("Test Button Sound")]
    private void TestButtonSound()
    {
        PlayButtonClick();
    }
}