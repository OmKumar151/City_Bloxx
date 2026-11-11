using UnityEngine;

/// <summary>
/// Short vibrations for gameplay events.
///
/// Android: uses the Vibrator service directly so each event can have its
/// own length and strength (Handheld.Vibrate is a fixed ~0.5 s buzz).
/// iOS: falls back to Handheld.Vibrate for the stronger events only.
/// Editor / other platforms: does nothing.
///
/// Referencing Handheld.Vibrate makes Unity add the Android VIBRATE
/// permission automatically.
/// </summary>
public static class Haptics
{
    private const string EnabledKey = "HapticsEnabled";

    // Lengths in milliseconds, amplitudes 1-255 (Android 8+).
    private const long ReleaseMs = 15;
    private const int ReleaseAmplitude = 60;

    private const long PerfectLandingMs = 25;
    private const int PerfectLandingAmplitude = 120;

    private const long OffCentreLandingMs = 45;
    private const int OffCentreLandingAmplitude = 255;

    private const long MissMs = 120;
    private const int MissAmplitude = 255;

    public static bool Enabled
    {
        get => PlayerPrefs.GetInt(EnabledKey, 1) == 1;
        set
        {
            SaveStore.SetInt(EnabledKey, value ? 1 : 0);
            SaveStore.Save();
        }
    }

    /// <summary>Light tick the moment the block leaves the hook.</summary>
    public static void BlockReleased()
    {
        Vibrate(ReleaseMs, ReleaseAmplitude, allowIosFallback: false);
    }

    /// <summary>Landing thump: stronger the further off-centre (accuracy 0-1).</summary>
    public static void BlockLanded(float accuracy, bool isPerfect)
    {
        float t = isPerfect ? 1f : Mathf.Clamp01(accuracy);

        long ms = (long)Mathf.Lerp(OffCentreLandingMs, PerfectLandingMs, t);
        int amplitude = Mathf.RoundToInt(
            Mathf.Lerp(OffCentreLandingAmplitude, PerfectLandingAmplitude, t));

        Vibrate(ms, amplitude, allowIosFallback: true);
    }

    /// <summary>Long buzz when a block misses and a life is lost.</summary>
    public static void BlockMissed()
    {
        Vibrate(MissMs, MissAmplitude, allowIosFallback: true);
    }

    private static void Vibrate(long milliseconds, int amplitude, bool allowIosFallback)
    {
        if (!Enabled)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrate(milliseconds, amplitude);
#elif UNITY_IOS && !UNITY_EDITOR
        if (allowIosFallback)
            Handheld.Vibrate();
#endif
    }

    // Never called. Its reference to Handheld.Vibrate is what makes Unity
    // add the VIBRATE permission to the Android manifest.
    private static void KeepVibratePermission()
    {
        Handheld.Vibrate();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject vibrator;
    private static int sdkInt = -1;

    private static void AndroidVibrate(long milliseconds, int amplitude)
    {
        try
        {
            if (vibrator == null)
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    sdkInt = version.GetStatic<int>("SDK_INT");
                }
            }

            if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
                return;

            if (sdkInt >= 26)
            {
                using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                using (var effect = effectClass.CallStatic<AndroidJavaObject>(
                           "createOneShot", milliseconds, Mathf.Clamp(amplitude, 1, 255)))
                {
                    vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Haptics: vibration failed: " + e.Message);
        }
    }
#endif
}
