using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

#if CITYBLOXX_GPGS && UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

/// <summary>
/// Google Play Games sign-in and cloud save.
///
/// - Signs in automatically at launch with the Google account already on
///   the phone (Play Games v2 shows no button for this).
/// - Progress (everything written through SaveStore) is stored in Play
///   Games "Saved Games", so it comes back after an uninstall/reinstall
///   or on a new phone.
///
/// Created automatically; nothing needs adding to a scene. Does nothing
/// until the Play Games plugin is installed and the CITYBLOXX_GPGS
/// scripting define is set (see Player Settings > Scripting Define Symbols).
/// </summary>
public class PlayGamesService : MonoBehaviour
{
    public static PlayGamesService Instance { get; private set; }

    public static event Action<bool> SignInChanged;

    public bool IsSignedIn { get; private set; }

    private const string SaveName = "citybloxx_progress";
    private const string MainMenuScene = "Main Menu";

    // Not tracked by SaveStore, so it never goes to the cloud: whether
    // THIS install has synced with the cloud at least once.
    private const string SyncedOnceKey = "CityBloxx_CloudSyncedOnce";

    private bool busy;
    private bool dirty;
    private string pendingImport;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject(nameof(PlayGamesService));
        DontDestroyOnLoad(go);
        go.AddComponent<PlayGamesService>();
    }

    private void Awake()
    {
        Instance = this;
        SaveStore.Changed += OnSaveChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SaveStore.Changed -= OnSaveChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
#if CITYBLOXX_GPGS && UNITY_ANDROID
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
#endif
    }

    /// <summary>
    /// For a "Sign in" button: use when automatic sign-in failed or the
    /// player cancelled it.
    /// </summary>
    public void SignIn()
    {
#if CITYBLOXX_GPGS && UNITY_ANDROID
        if (!IsSignedIn)
            PlayGamesPlatform.Instance.ManuallyAuthenticate(OnAuthenticated);
#endif
    }

    // =========================================================
    // UPLOAD TRIGGERS
    // =========================================================

    private void OnSaveChanged() => dirty = true;

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            UploadIfDirty();
    }

    private void OnApplicationQuit() => UploadIfDirty();

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingImport != null && scene.name == MainMenuScene)
        {
            string json = pendingImport;
            pendingImport = null;
            ApplyCloudSave(json);
            return;
        }

        UploadIfDirty();
    }

    private void UploadIfDirty()
    {
        if (dirty && IsSignedIn && PlayerPrefs.GetInt(SyncedOnceKey, 0) == 1)
            Upload();
    }

    // =========================================================
    // CLOUD SYNC
    // =========================================================

    private void ApplyCloudSave(string json)
    {
        if (!SaveStore.Import(json))
            return;

        dirty = false;
        Debug.Log("PlayGamesService: Progress restored from the cloud.");

        // Managers read their data on load, so reload to pick it up.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void MarkSynced()
    {
        PlayerPrefs.SetInt(SyncedOnceKey, 1);
        PlayerPrefs.Save();
    }

#if CITYBLOXX_GPGS && UNITY_ANDROID
    private void OnAuthenticated(SignInStatus status)
    {
        IsSignedIn = status == SignInStatus.Success;
        SignInChanged?.Invoke(IsSignedIn);

        Debug.Log("PlayGamesService: Sign-in " + status);

        if (IsSignedIn)
            Download();
    }

    private ISavedGameClient Client => PlayGamesPlatform.Instance.SavedGame;

    private void Open(Action<ISavedGameMetadata> onOpened)
    {
        if (busy)
            return;

        busy = true;

        Client.OpenWithAutomaticConflictResolution(
            SaveName,
            DataSource.ReadNetworkOnly,
            ConflictResolutionStrategy.UseMostRecentlySaved,
            (status, metadata) =>
            {
                if (status != SavedGameRequestStatus.Success)
                {
                    busy = false;
                    Debug.LogWarning("PlayGamesService: Open save failed: " + status);
                    return;
                }

                onOpened(metadata);
            });
    }

    private void Download()
    {
        Open(metadata =>
        {
            Client.ReadBinaryData(metadata, (status, data) =>
            {
                busy = false;

                if (status != SavedGameRequestStatus.Success)
                {
                    Debug.LogWarning("PlayGamesService: Read save failed: " + status);
                    return;
                }

                OnCloudDataRead(data);
            });
        });
    }

    private void OnCloudDataRead(byte[] data)
    {
        bool hasCloud = data != null && data.Length > 0;
        bool syncedBefore = PlayerPrefs.GetInt(SyncedOnceKey, 0) == 1;

        if (!hasCloud)
        {
            // New player: this device's progress becomes the cloud save.
            MarkSynced();
            Upload();
            return;
        }

        string json = Encoding.UTF8.GetString(data);

        // Fresh install (or first sign-in here): the cloud always wins, so
        // settings written at launch can't overwrite real progress.
        // Afterwards, whichever side changed last wins.
        bool useCloud = !syncedBefore ||
            SaveStore.GetExportModified(json) > SaveStore.LastModified;

        MarkSynced();

        if (!useCloud)
        {
            if (SaveStore.LastModified > SaveStore.GetExportModified(json))
                Upload();

            return;
        }

        // Don't yank the player out of a level; restore on the main menu.
        if (SceneManager.GetActiveScene().name == MainMenuScene)
            ApplyCloudSave(json);
        else
            pendingImport = json;
    }

    private void Upload()
    {
        // Another save operation is running; stay dirty and retry on the
        // next trigger (pause, scene change).
        if (busy)
            return;

        dirty = false;
        byte[] data = Encoding.UTF8.GetBytes(SaveStore.Export());

        Open(metadata =>
        {
            SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedDescription("City Bloxx progress")
                .Build();

            Client.CommitUpdate(metadata, update, data, (status, _) =>
            {
                busy = false;

                if (status != SavedGameRequestStatus.Success)
                {
                    dirty = true;
                    Debug.LogWarning("PlayGamesService: Upload failed: " + status);
                }
            });
        });
    }
#else
    private void Upload() { dirty = false; }
#endif
}
