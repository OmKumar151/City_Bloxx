using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Short vibrations for gameplay events.
///
/// Every value (which tap, how long, how strong, per event) lives in the
/// HapticsConfig asset at Assets/Resources/HapticsConfig.asset; edit it in
/// the Inspector.
///
/// Android: uses the Vibrator service directly. Android 10+ gets the
/// phone's built-in crisp taps; older phones get short buzzes
/// (Handheld.Vibrate is a fixed ~0.5 s buzz, too long for this).
/// iOS: uses the Taptic Engine (Assets/Plugins/iOS/CityBloxxHaptics.mm).
/// Editor / other platforms: does nothing.
///
/// The player sets the strength with the Vibration slider in Settings
/// (0 = off). Android buzzes are played as game vibrations, so they work
/// even when the phone's touch feedback is turned off.
///
/// Referencing Handheld.Vibrate makes Unity add the Android VIBRATE
/// permission automatically.
/// </summary>
public static class Haptics
{
    private const string IntensityKey = "HapticsIntensity";

    // Older builds saved an on/off toggle; off carries over as 0.
    private const string LegacyEnabledKey = "HapticsEnabled";

    private static HapticsConfig config;

    public static HapticsConfig Config
    {
        get
        {
            if (config == null)
            {
                config = Resources.Load<HapticsConfig>(HapticsConfig.ResourcePath);

                if (config == null)
                {
                    Debug.LogWarning(
                        "Haptics: Resources/" + HapticsConfig.ResourcePath +
                        " not found, using default values.");

                    config = ScriptableObject.CreateInstance<HapticsConfig>();
                }
            }

            return config;
        }
    }

    public static float DefaultIntensity => Mathf.Clamp01(Config.defaultIntensity);

    /// <summary>Vibration strength from the Settings slider: 0 = off, 1 = full.</summary>
    public static float Intensity
    {
        get
        {
            if (PlayerPrefs.HasKey(IntensityKey))
                return Mathf.Clamp01(PlayerPrefs.GetFloat(IntensityKey));

            if (PlayerPrefs.HasKey(LegacyEnabledKey) &&
                PlayerPrefs.GetInt(LegacyEnabledKey) == 0)
                return 0f;

            return DefaultIntensity;
        }
        set
        {
            SaveStore.SetFloat(IntensityKey, Mathf.Clamp01(value));
            SaveStore.Save();
        }
    }

    public static bool Enabled => Intensity > 0.001f;

    /// <summary>Light tick the moment the block leaves the hook.</summary>
    public static void BlockReleased()
    {
        if (Enabled)
            Play(Config.blockReleased);
    }

    /// <summary>Landing thump: stronger the further off-centre (accuracy 0-1).</summary>
    public static void BlockLanded(float accuracy, bool isPerfect)
    {
        if (!Enabled)
            return;

        HapticsConfig c = Config;

        if (isPerfect)
        {
            Play(c.perfectLanding);
            return;
        }

        HapticEvent e = c.offCentreLanding;

        if (!c.scaleOffCentreByAccuracy)
        {
            Play(e);
            return;
        }

        float t = Mathf.Clamp01(accuracy);

        Play(
            e,
            Mathf.RoundToInt(Mathf.Lerp(e.buzzMilliseconds, c.nearCentreBuzzMilliseconds, t)),
            Mathf.RoundToInt(Mathf.Lerp(e.buzzStrength, c.nearCentreBuzzStrength, t)),
            Mathf.Lerp(e.iosIntensity, c.nearCentreIosIntensity, t),
            preferGraded: true);
    }

    /// <summary>Long buzz when a block misses and a life is lost.</summary>
    public static void BlockMissed()
    {
        if (Enabled)
            Play(Config.blockMissed);
    }

    /// <summary>Short tap at the current strength, for the Settings slider.</summary>
    public static void Preview()
    {
        if (Enabled)
            Play(Config.togglePreview);
    }

    private static void Play(HapticEvent e)
    {
        Play(e, e.buzzMilliseconds, e.buzzStrength, e.iosIntensity, false);
    }

    /// <param name="preferGraded">Android: use the buzz (so strength can
    /// vary) on phones that support strength control, the tap otherwise.</param>
    private static void Play(
        HapticEvent e, int milliseconds, int strength, float iosIntensity,
        bool preferGraded)
    {
        float scale = Intensity;

        if (e == null || scale <= 0.001f)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        // Notifications are fixed system patterns; below full strength use
        // a heavy impact instead so the slider still has an effect.
        if (e.iosKind == IosHapticKind.Notification && scale >= 0.999f)
            IosNotification((int)e.iosNotification);
        else if (e.iosKind == IosHapticKind.Notification)
            IosImpact((int)IosImpactStyle.Heavy, scale);
        else
            IosImpact((int)e.iosImpactStyle, iosIntensity * scale);
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrate(milliseconds, strength, (int)e.androidTap, preferGraded, scale);
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void CityBloxx_HapticImpact(int style, float intensity);

    [DllImport("__Internal")]
    private static extern void CityBloxx_HapticNotification(int type);

    private static void IosImpact(int style, float intensity)
    {
        try { CityBloxx_HapticImpact(style, intensity); }
        catch (System.Exception ex) { Debug.LogWarning("Haptics: " + ex.Message); }
    }

    private static void IosNotification(int type)
    {
        try { CityBloxx_HapticNotification(type); }
        catch (System.Exception ex) { Debug.LogWarning("Haptics: " + ex.Message); }
    }
#endif

    // Never called. Its reference to Handheld.Vibrate is what makes Unity
    // add the VIBRATE permission to the Android manifest. [Preserve] stops
    // code stripping from removing it (and with it the permission).
    [UnityEngine.Scripting.Preserve]
    private static void KeepVibratePermission()
    {
        Handheld.Vibrate();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject vibrator;
    private static AndroidJavaClass effectClass;
    private static int sdkInt = -1;
    private static bool hasVibrator;
    private static bool hasAmplitudeControl;
    private static bool initialised;
    private static AndroidJavaObject gameAudioAttributes;

    // AudioAttributes.USAGE_GAME: the phone treats these as game/media
    // vibrations, not touch feedback (which many players have turned off).
    private const int UsageGame = 14;

    private static void InitAndroid()
    {
        if (initialised)
            return;

        initialised = true;

        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            sdkInt = version.GetStatic<int>("SDK_INT");

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

        hasVibrator = vibrator != null && vibrator.Call<bool>("hasVibrator");

        if (sdkInt >= 26)
        {
            effectClass = new AndroidJavaClass("android.os.VibrationEffect");
            hasAmplitudeControl = hasVibrator && vibrator.Call<bool>("hasAmplitudeControl");
        }

        try
        {
            using (var builder = new AndroidJavaObject("android.media.AudioAttributes$Builder"))
            using (var withUsage = builder.Call<AndroidJavaObject>("setUsage", UsageGame))
                gameAudioAttributes = withUsage.Call<AndroidJavaObject>("build");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Haptics: no game audio attributes: " + ex.Message);
            gameAudioAttributes = null;
        }
    }

    private static void VibrateEffect(AndroidJavaObject effect)
    {
        if (gameAudioAttributes != null)
            vibrator.Call("vibrate", effect, gameAudioAttributes);
        else
            vibrator.Call("vibrate", effect);
    }

    /// <param name="milliseconds">Buzz length for the fallback one-shot.</param>
    /// <param name="amplitude">Buzz strength 1-255 for the fallback one-shot.</param>
    /// <param name="predefinedEffect">Built-in tap to use on Android 10+, or -1.</param>
    /// <param name="preferGraded">Use the one-shot (so strength can vary)
    /// when the phone supports strength control, the tap otherwise.</param>
    /// <param name="scale">Settings slider strength, 0-1.</param>
    private static void AndroidVibrate(
        long milliseconds, int amplitude, int predefinedEffect, bool preferGraded,
        float scale)
    {
        try
        {
            InitAndroid();

            if (!hasVibrator)
                return;

            milliseconds = System.Math.Max(1L, milliseconds);

            // Built-in taps can't be scaled, so only use them at full strength.
            bool usePredefined =
                sdkInt >= 29 &&
                predefinedEffect >= 0 &&
                scale >= 0.999f &&
                !(preferGraded && hasAmplitudeControl);

            // Phones that can vary strength get a weaker buzz; the rest get
            // a shorter one.
            if (hasAmplitudeControl)
                amplitude = Mathf.RoundToInt(amplitude * scale);
            else
                milliseconds = (long)Mathf.Max(8f, milliseconds * scale);

            if (usePredefined)
            {
                using (var effect = effectClass.CallStatic<AndroidJavaObject>(
                           "createPredefined", predefinedEffect))
                {
                    VibrateEffect(effect);
                }
            }
            else if (sdkInt >= 26)
            {
                using (var effect = effectClass.CallStatic<AndroidJavaObject>(
                           "createOneShot", milliseconds, Mathf.Clamp(amplitude, 1, 255)))
                {
                    VibrateEffect(effect);
                }
            }
            else
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Haptics: vibration failed: " + ex.Message);
        }
    }
#endif
}
