using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // =========================================================
    // AUDIO SOURCES
    // =========================================================

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // =========================================================
    // MUSIC
    // =========================================================

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip levelSelectMusic;
    [SerializeField] private AudioClip gameplayMusic;

    // =========================================================
    // UI SOUND EFFECTS
    // =========================================================

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip majorButtonSound;
    [SerializeField] private AudioClip navigationButtonSound;
    [SerializeField] private AudioClip deniedSound;
    [SerializeField] private AudioClip windowSwitchSound;

    // =========================================================
    // CITY / PROGRESSION
    // =========================================================

    [Header("City / Progression Sound Effects")]
    [SerializeField] private AudioClip placementConfirmedSound;
    [SerializeField] private AudioClip unlockingSound;

    // =========================================================
    // GAMEPLAY
    // =========================================================

    [Header("Gameplay Sound Effects")]
    [SerializeField] private AudioClip gameWonSound;
    [SerializeField] private AudioClip gameOverSound;

    // =========================================================
    // MUSIC TRANSITION
    // =========================================================

    [Header("Music Transition")]
    [SerializeField] private float musicFadeDuration = 0.35f;

    // =========================================================
    // DEFAULT SETTINGS
    // =========================================================

    [Header("Default Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSFXVolume = 1f;

    // =========================================================
    // PLAYER PREFS
    // =========================================================

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MuteKey = "AudioMuted";

    // =========================================================
    // CURRENT SETTINGS
    // =========================================================

    private float musicVolume;
    private float sfxVolume;
    private bool isMuted;

    private Coroutine musicTransitionCoroutine;

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public bool IsMuted => isMuted;

    // =========================================================
    // INITIALIZATION
    // =========================================================

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

    public void PlayLevelSelectMusic()
    {
        PlayMusic(levelSelectMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    private void PlayMusic(AudioClip newClip)
    {
        if (musicSource == null)
        {
            Debug.LogError(
                "AudioManager: Music Source is not assigned."
            );

            return;
        }

        if (newClip == null)
        {
            Debug.LogError(
                "AudioManager: Music Clip is not assigned."
            );

            return;
        }

        if (musicSource.clip == newClip &&
            musicSource.isPlaying)
        {
            return;
        }

        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
        }

        musicTransitionCoroutine =
            StartCoroutine(CrossfadeMusic(newClip));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float startingVolume = musicSource.volume;

        // -----------------------------------------------------
        // FADE OLD MUSIC OUT
        // -----------------------------------------------------

        if (musicSource.isPlaying)
        {
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = musicFadeDuration > 0f
                    ? elapsed / musicFadeDuration
                    : 1f;

                musicSource.volume = Mathf.Lerp(
                    startingVolume,
                    0f,
                    t
                );

                yield return null;
            }

            musicSource.Stop();
        }

        // -----------------------------------------------------
        // CHANGE TRACK
        // -----------------------------------------------------

        musicSource.clip = newClip;
        musicSource.loop = true;

        musicSource.Play();

        // -----------------------------------------------------
        // FADE NEW MUSIC IN
        // -----------------------------------------------------

        float targetVolume =
            isMuted ? 0f : musicVolume;

        float fadeElapsed = 0f;

        while (fadeElapsed < musicFadeDuration)
        {
            fadeElapsed += Time.unscaledDeltaTime;

            float t = musicFadeDuration > 0f
                ? fadeElapsed / musicFadeDuration
                : 1f;

            musicSource.volume = Mathf.Lerp(
                0f,
                targetVolume,
                t
            );

            yield return null;
        }

        musicSource.volume = targetVolume;

        musicTransitionCoroutine = null;

        Debug.Log(
            "AudioManager: Playing music: " + newClip.name
        );
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);

            musicTransitionCoroutine = null;
        }

        musicSource.Stop();

        musicSource.volume =
            isMuted ? 0f : musicVolume;
    }

    // =========================================================
    // UI SOUND EFFECTS
    // =========================================================

    public void PlayButtonClick()
    {
        PlaySFX(buttonSound);
    }

    public void PlayMajorButton()
    {
        PlaySFX(majorButtonSound);
    }

    public void PlayNavigationButton()
    {
        PlaySFX(navigationButtonSound);
    }

    public void PlayDenied()
    {
        PlaySFX(deniedSound);
    }

    public void PlayWindowSwitch()
    {
        PlaySFX(windowSwitchSound);
    }

    // =========================================================
    // CITY / PROGRESSION
    // =========================================================

    public void PlayPlacementConfirmed()
    {
        PlaySFX(placementConfirmedSound);
    }

    public void PlayUnlocking()
    {
        PlaySFX(unlockingSound);
    }

    // =========================================================
    // GAMEPLAY
    // =========================================================

    public void PlayGameWon()
    {
        PlaySFX(gameWonSound);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverSound);
    }

    // =========================================================
    // COMPATIBILITY WITH EXISTING SCRIPTS
    // =========================================================

    public void PlaySelectionSound()
    {
        PlayNavigationButton();
    }

    public void PlayConfirmSound()
    {
        PlayMajorButton();
    }

    public void PlayErrorSound()
    {
        PlayDenied();
    }

    public void PlayWhooshSound()
    {
        PlayWindowSwitch();
    }

    public void PlayBuildingPlacedSound()
    {
        PlayPlacementConfirmed();
    }

    public void PlayBuildingUnlockedSound()
    {
        PlayUnlocking();
    }

    public void PlayVictorySound()
    {
        PlayGameWon();
    }

    // =========================================================
    // SFX CORE
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

        // IMPORTANT:
        // SFX volume is controlled ONLY by sfxVolume.
        // Music volume has no effect here.

        sfxSource.PlayOneShot(clip);
    }

    // =========================================================
    // MUSIC VOLUME
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

        Debug.Log(
            "AudioManager: Music Volume = " +
            musicVolume
        );
    }

    // =========================================================
    // SFX VOLUME
    // =========================================================

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        ApplySFXSettings();

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.Save();

        Debug.Log(
            "AudioManager: SFX Volume = " +
            sfxVolume
        );
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
            "AudioManager: Muted = " +
            isMuted
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

        musicSource.volume =
            isMuted ? 0f : musicVolume;

        // We control mute through volume.
        musicSource.mute = false;
    }

    private void ApplySFXSettings()
    {
        if (sfxSource == null)
            return;

        // IMPORTANT:
        // SFX has its own independent volume.

        sfxSource.volume = sfxVolume;

        // Mute is the only thing shared.
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
    // RESET
    // =========================================================

    [ContextMenu("Reset Audio Settings")]
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
    // TESTING
    // =========================================================

    [ContextMenu("Test Main Menu Music")]
    private void TestMainMenuMusic()
    {
        PlayMainMenuMusic();
    }

    [ContextMenu("Test Level Select Music")]
    private void TestLevelSelectMusic()
    {
        PlayLevelSelectMusic();
    }

    [ContextMenu("Test Button Sound")]
    private void TestButtonSound()
    {
        PlayButtonClick();
    }

    [ContextMenu("Test Major Button")]
    private void TestMajorButton()
    {
        PlayMajorButton();
    }

    [ContextMenu("Test Navigation Button")]
    private void TestNavigationButton()
    {
        PlayNavigationButton();
    }

    [ContextMenu("Test Denied")]
    private void TestDenied()
    {
        PlayDenied();
    }

    [ContextMenu("Test Placement Confirmed")]
    private void TestPlacementConfirmed()
    {
        PlayPlacementConfirmed();
    }

    [ContextMenu("Test Unlocking")]
    private void TestUnlocking()
    {
        PlayUnlocking();
    }

    [ContextMenu("Test Game Won")]
    private void TestGameWon()
    {
        PlayGameWon();
    }

    [ContextMenu("Test Game Over")]
    private void TestGameOver()
    {
        PlayGameOver();
    }

    [ContextMenu("Test Window Switch")]
    private void TestWindowSwitch()
    {
        PlayWindowSwitch();
    }
}