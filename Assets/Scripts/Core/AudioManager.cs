using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip levelSelectMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string levelSelectSceneName = "LevelSelect";
    [SerializeField] private string gameplaySceneName = "Gameplay1";

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip deniedSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip gameWonSound;
    [SerializeField] private AudioClip levelSelectSound;
    [SerializeField] private AudioClip mainMenuSound;
    [SerializeField] private AudioClip majorButtonSound;
    [SerializeField] private AudioClip navigationButtonSound;
    [SerializeField] private AudioClip placementConfirmedSound;
    [SerializeField] private AudioClip unlockingSound;
    [SerializeField] private AudioClip windowSwitchSound;

    [Header("Gameplay SFX")]
    [Tooltip("Block let go from the crane hook.")]
    [SerializeField] private AudioClip blockDropSound;
    [Tooltip("Block lands on the tower. Louder the further off-centre.")]
    [SerializeField] private AudioClip blockLandSound;
    [Tooltip("Block missed the tower (a life lost).")]
    [SerializeField] private AudioClip blockMissSound;
    [Tooltip("Perfect landing chimes, one per step of the perfect streak: " +
             "element 0 for the first perfect, 1 for two in a row, and so on. " +
             "Longer streaks keep using the last one.")]
    [SerializeField] private AudioClip[] perfectStreakSounds;

    [Header("Settings")]
    [SerializeField] private float musicFadeDuration = 0.35f;
    [SerializeField] private float defaultMusicVolume = 1f;
    [SerializeField] private float defaultSFXVolume = 1f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MuteKey = "AudioMuted";

    private float musicVolume;
    private float sfxVolume;
    private bool isMuted;

    private Coroutine musicFadeCoroutine;

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public bool IsMuted => isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "DUPLICATE AudioManager FOUND. Destroying: " +
                gameObject.name
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
        LoadAudioSettings();
        ApplyAudioSettings();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForCurrentScene();
    }

    private void OnDestroy()
    {
        Debug.LogWarning("==========================================");
        Debug.LogWarning("!!! AUDIO MANAGER WAS DESTROYED !!!");
        Debug.LogWarning("GameObject: " + gameObject.name);
        Debug.LogWarning("Scene: " + gameObject.scene.name);
        Debug.LogWarning("Was Instance: " + (Instance == this));
        Debug.LogWarning("==========================================");

        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;

            Debug.LogWarning("AudioManager.Instance has been set to NULL.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this)
        {
            Debug.LogError(
                "AudioManager sceneLoaded callback fired, " +
                "but Instance is NOT this AudioManager."
            );
        }

        PlayMusicForScene(scene.name);
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: Music Source is NOT assigned!");
        }

        if (sfxSource == null)
        {
            Debug.LogError("AudioManager: SFX Source is NOT assigned!");
        }

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

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

        isMuted = PlayerPrefs.GetInt(
            MuteKey,
            0
        ) == 1;
    }

    private void ApplyAudioSettings()
    {
        ApplyMusicSettings();
        ApplySFXSettings();
    }

    private void ApplyMusicSettings()
    {
        if (musicSource == null)
            return;

        musicSource.volume = isMuted ? 0f : musicVolume;
        musicSource.mute = false;
    }

    private void ApplySFXSettings()
    {
        if (sfxSource == null)
            return;

        sfxSource.volume = sfxVolume;
        sfxSource.mute = isMuted;
    }

    // =========================================================
    // MUSIC
    // =========================================================

    public void PlayMusicForCurrentScene()
    {
        PlayMusicForScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void PlayMusicForScene(string sceneName)
    {
        if (sceneName == mainMenuSceneName)
        {
            PlayMainMenuMusic();
        }
        else if (sceneName == levelSelectSceneName)
        {
            PlayLevelSelectMusic();
        }
        else if (sceneName == gameplaySceneName)
        {
            PlayGameplayMusic();
        }
    }

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
            Debug.LogError("AudioManager: Music Source missing.");
            return;
        }

        if (newClip == null)
        {
            Debug.LogWarning("AudioManager: Music clip is NULL.");
            return;
        }

        if (musicSource.clip == newClip && musicSource.isPlaying)
        {
            return;
        }

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(
            CrossfadeMusic(newClip)
        );
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;

            float timer = 0f;

            while (timer < musicFadeDuration)
            {
                timer += Time.unscaledDeltaTime;

                musicSource.volume = Mathf.Lerp(
                    startVolume,
                    0f,
                    timer / musicFadeDuration
                );

                yield return null;
            }
        }

        musicSource.clip = newClip;
        musicSource.volume = 0f;
        musicSource.Play();

        float fadeTimer = 0f;

        while (fadeTimer < musicFadeDuration)
        {
            fadeTimer += Time.unscaledDeltaTime;

            musicSource.volume = Mathf.Lerp(
                0f,
                isMuted ? 0f : musicVolume,
                fadeTimer / musicFadeDuration
            );

            yield return null;
        }

        musicSource.volume = isMuted ? 0f : musicVolume;
    }

    // =========================================================
    // SFX
    // =========================================================

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayDenied()
    {
        PlaySFX(deniedSound);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverSound);
    }

    public void PlayGameWon()
    {
        PlaySFX(gameWonSound);
    }

    public void PlayLevelSelectSound()
    {
        PlaySFX(levelSelectSound);
    }

    public void PlayMainMenuSound()
    {
        PlaySFX(mainMenuSound);
    }

    public void PlayMajorButton()
    {
        PlaySFX(majorButtonSound);
    }

    public void PlayNavigationButton()
    {
        PlaySFX(navigationButtonSound);
    }

    public void PlayPlacementConfirmed()
    {
        PlaySFX(placementConfirmedSound);
    }

    public void PlayUnlocking()
    {
        PlaySFX(unlockingSound);
    }

    public void PlayWindowSwitch()
    {
        PlaySFX(windowSwitchSound);
    }

    // =========================================================
    // GAMEPLAY SFX
    // =========================================================

    public void PlayBlockDrop()
    {
        PlayGameplaySFX(blockDropSound, 0.8f);
    }

    /// <param name="accuracy">0 = barely on, 1 = dead centre.</param>
    public void PlayBlockLanded(float accuracy)
    {
        PlayGameplaySFX(blockLandSound, Mathf.Lerp(1f, 0.7f, Mathf.Clamp01(accuracy)));
    }

    /// <param name="streak">Perfects in a row, including this one (1+).</param>
    public void PlayPerfect(int streak)
    {
        if (perfectStreakSounds == null || perfectStreakSounds.Length == 0)
            return;

        int index = Mathf.Clamp(streak - 1, 0, perfectStreakSounds.Length - 1);
        PlayGameplaySFX(perfectStreakSounds[index], 1f);
    }

    public void PlayBlockMiss()
    {
        PlayGameplaySFX(blockMissSound, 1f);
    }

    // Played on every block, so no logging (keeps the phone log quiet).
    private void PlayGameplaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volumeScale);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("SFX REQUEST FAILED: AudioClip is NULL!");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("SFX REQUEST FAILED: SFX AudioSource is NULL!");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        SaveStore.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        SaveStore.Save();

        ApplyMusicSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        SaveStore.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        SaveStore.Save();

        ApplySFXSettings();
    }

    public void SetMuted(bool muted)
    {
        isMuted = muted;

        SaveStore.SetInt(
            MuteKey,
            isMuted ? 1 : 0
        );

        SaveStore.Save();

        ApplyAudioSettings();
    }

    public void ResetAudioSettings()
    {
        musicVolume = defaultMusicVolume;
        sfxVolume = defaultSFXVolume;
        isMuted = false;

        SaveStore.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        SaveStore.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        SaveStore.SetInt(
            MuteKey,
            0
        );

        SaveStore.Save();

        ApplyAudioSettings();
    }

    // =========================================================
    // TEST
    // =========================================================

    [ContextMenu("TEST - Play Button")]
    private void TestButton()
    {
        PlayButtonClick();
    }

    [ContextMenu("TEST - Play Navigation")]
    private void TestNavigation()
    {
        PlayNavigationButton();
    }

    [ContextMenu("TEST - Toggle Mute")]
    private void TestMute()
    {
        SetMuted(!isMuted);
    }

    [ContextMenu("TEST - Reset Audio")]
    private void TestReset()
    {
        ResetAudioSettings();
    }

    // =========================================================
    // SFX TESTS
    // =========================================================

    [ContextMenu("TEST - Play Button")]
    private void TestButtonSound()
    {
        PlayButtonClick();
    }

    [ContextMenu("TEST - Play Navigation")]
    private void TestNavigationSound()
    {
        PlayNavigationButton();
    }

    [ContextMenu("TEST - Play Denied")]
    private void TestDeniedSound()
    {
        PlayDenied();
    }

    [ContextMenu("TEST - Play Placement Confirmed")]
    private void TestPlacementConfirmedSound()
    {
        PlayPlacementConfirmed();
    }

}