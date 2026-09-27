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
        Debug.Log("========== AUDIO MANAGER AWAKE ==========");
        Debug.Log("AudioManager GameObject: " + gameObject.name);
        Debug.Log("AudioManager Scene: " + gameObject.scene.name);
        Debug.Log("AudioManager Is Root: " + (transform.root == transform));

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

        Debug.Log("AudioManager.Instance assigned.");
        Debug.Log("DontDestroyOnLoad called.");

        SetupAudioSources();
        LoadAudioSettings();
        ApplyAudioSettings();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        Debug.Log("========== AUDIO MANAGER START ==========");
        Debug.Log("Instance exists: " + (Instance != null));
        Debug.Log("Current Scene: " + SceneManager.GetActiveScene().name);

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
        Debug.Log("========== AUDIO MANAGER SCENE LOADED ==========");
        Debug.Log("Scene: " + scene.name);
        Debug.Log("AudioManager.Instance exists: " + (Instance != null));

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

        Debug.Log(
            "Audio Settings Loaded | " +
            "Music: " + musicVolume +
            " | SFX: " + sfxVolume +
            " | Muted: " + isMuted
        );
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
        Debug.Log(
            "AudioManager: Selecting music for scene: " +
            sceneName
        );

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
        else
        {
            Debug.Log(
                "AudioManager: No music assigned for scene: " +
                sceneName
            );
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
            Debug.Log(
                "AudioManager: Music already playing: " +
                newClip.name
            );

            return;
        }

        Debug.Log(
            "AudioManager: Switching music to: " +
            newClip.name
        );

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

        Debug.Log(
            "AudioManager: Music Playing: " +
            newClip.name
        );
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

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError(
                "AudioManager: SFX Source missing."
            );

            return;
        }

        if (clip == null)
        {
            Debug.LogWarning(
                "AudioManager: SFX clip is NULL."
            );

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

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        PlayerPrefs.Save();

        ApplyMusicSettings();

        Debug.Log(
            "AudioManager: Music Volume = " +
            musicVolume
        );
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.Save();

        ApplySFXSettings();

        Debug.Log(
            "AudioManager: SFX Volume = " +
            sfxVolume
        );
    }

    public void SetMuted(bool muted)
    {
        isMuted = muted;

        PlayerPrefs.SetInt(
            MuteKey,
            isMuted ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplyAudioSettings();

        Debug.Log(
            "AudioManager: Muted = " +
            isMuted
        );
    }

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
}