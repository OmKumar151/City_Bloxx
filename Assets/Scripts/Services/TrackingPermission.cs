using System;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

/// <summary>
/// iOS App Tracking Transparency: the system "Allow this app to track
/// you?" prompt. Apple requires it before ads may use the device's
/// advertising ID. Whatever the player answers, ads still show; a "no"
/// just means less personalised ones.
///
/// Shown at most once by iOS itself. On Android, in the Editor and on iOS
/// before 14, the callback runs straight away.
///
/// The prompt text is NSUserTrackingUsageDescription, added to Info.plist
/// by IosBuildPostProcess. Native side: Assets/Plugins/iOS/TrackingPermission.mm.
/// </summary>
public static class TrackingPermission
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern int _AttStatus();
    [DllImport("__Internal")] private static extern void _AttRequest();
#endif

    // ATTrackingManagerAuthorizationStatus.notDetermined
    private const int NotDetermined = 0;

    // Give up waiting after this long, so ads never block the game.
    private const float TimeoutSeconds = 60f;

    public static void Request(Action onDone)
    {
#if UNITY_IOS && !UNITY_EDITOR
        RequestAsync(onDone);
#else
        onDone?.Invoke();
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    private static async void RequestAsync(Action onDone)
    {
        float waited = 0f;

        // iOS ignores the request while the app isn't fully active (e.g.
        // right at launch), so ask again until the player has answered.
        while (_AttStatus() == NotDetermined && waited < TimeoutSeconds)
        {
            _AttRequest();
            await Awaitable.WaitForSecondsAsync(1f);
            waited += 1f;
        }

        onDone?.Invoke();
    }
#endif
}
