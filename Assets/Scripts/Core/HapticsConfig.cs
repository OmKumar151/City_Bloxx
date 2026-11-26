using UnityEngine;

/// <summary>Android 10+ built-in taps (VibrationEffect.EFFECT_*).</summary>
public enum AndroidTap
{
    [Tooltip("No built-in tap: always use the buzz.")]
    None = -1,
    Click = 0,
    DoubleClick = 1,
    Tick = 2,
    HeavyClick = 5
}

public enum IosHapticKind
{
    Impact,
    Notification
}

public enum IosImpactStyle
{
    Light = 0,
    Medium = 1,
    Heavy = 2,
    Rigid = 3,
    Soft = 4
}

public enum IosNotificationType
{
    Success = 0,
    Warning = 1,
    Error = 2
}

/// <summary>How the phone vibrates for one gameplay event.</summary>
[System.Serializable]
public class HapticEvent
{
    [Tooltip("Built-in phone tap used on Android 10+ when the Vibration slider is at " +
             "full. None = always use the buzz below (recommended: built-in taps are " +
             "silenced on phones with touch feedback turned off, and can't be scaled).")]
    public AndroidTap androidTap = AndroidTap.None;

    [Tooltip("Buzz length in milliseconds, used when the built-in tap isn't.")]
    [Range(1, 500)] public int buzzMilliseconds = 30;

    [Tooltip("Buzz strength on Android 8+ (1 = faintest, 255 = strongest).")]
    [Range(1, 255)] public int buzzStrength = 128;

    [Tooltip("iOS: Impact = a tap of the style below. Notification = the system pattern below.")]
    public IosHapticKind iosKind = IosHapticKind.Impact;

    public IosImpactStyle iosImpactStyle = IosImpactStyle.Medium;

    [Range(0f, 1f)] public float iosIntensity = 1f;

    public IosNotificationType iosNotification = IosNotificationType.Error;

    public HapticEvent()
    {
    }

    public HapticEvent(
        AndroidTap tap, int milliseconds, int strength,
        IosImpactStyle style, float intensity)
    {
        androidTap = tap;
        buzzMilliseconds = milliseconds;
        buzzStrength = strength;
        iosKind = IosHapticKind.Impact;
        iosImpactStyle = style;
        iosIntensity = intensity;
    }

    public static HapticEvent Notification(
        AndroidTap tap, int milliseconds, int strength,
        IosNotificationType type)
    {
        return new HapticEvent
        {
            androidTap = tap,
            buzzMilliseconds = milliseconds,
            buzzStrength = strength,
            iosKind = IosHapticKind.Notification,
            iosNotification = type
        };
    }
}

/// <summary>
/// Vibration settings for every gameplay event. Haptics loads the asset
/// at Assets/Resources/HapticsConfig.asset.
/// </summary>
[CreateAssetMenu(
    fileName = "HapticsConfig",
    menuName = "Stackhaven/Haptics Configuration"
)]
public class HapticsConfig : ScriptableObject
{
    public const string ResourcePath = "HapticsConfig";

    [Tooltip("Vibration strength for players who have never moved the " +
             "Settings slider (0 = off, 1 = full). Reset Settings also goes back to this.")]
    [Range(0f, 1f)] public float defaultIntensity = 1f;

    [Header("Block released from the hook")]
    public HapticEvent blockReleased =
        new HapticEvent(AndroidTap.None, 25, 140, IosImpactStyle.Light, 0.7f);

    [Header("Perfect landing")]
    public HapticEvent perfectLanding =
        new HapticEvent(AndroidTap.None, 40, 200, IosImpactStyle.Rigid, 1f);

    [Header("Off-centre landing (values at the far edge)")]
    public HapticEvent offCentreLanding =
        new HapticEvent(AndroidTap.None, 70, 255, IosImpactStyle.Heavy, 1f);

    [Tooltip("Weaken the off-centre vibration the closer the block lands to " +
             "centre, down to the near-centre values below. On Android this " +
             "uses the buzz instead of the built-in tap on phones that can " +
             "vary strength.")]
    public bool scaleOffCentreByAccuracy = true;

    [Range(1, 500)] public int nearCentreBuzzMilliseconds = 40;
    [Range(1, 255)] public int nearCentreBuzzStrength = 170;
    [Range(0f, 1f)] public float nearCentreIosIntensity = 0.7f;

    [Header("Block missed (life lost)")]
    public HapticEvent blockMissed =
        HapticEvent.Notification(AndroidTap.None, 300, 255, IosNotificationType.Error);

    [Header("Vibration slider moved in Settings")]
    public HapticEvent togglePreview =
        new HapticEvent(AndroidTap.None, 50, 255, IosImpactStyle.Medium, 1f);
}
