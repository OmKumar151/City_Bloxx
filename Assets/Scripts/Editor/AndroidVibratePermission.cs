#if UNITY_ANDROID
using System.IO;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Makes sure Android builds ask for the VIBRATE permission, so Haptics
/// always works. Unity normally adds it when it sees Handheld.Vibrate in the
/// code; this is a safety net in case code stripping removes that.
/// </summary>
public class AndroidVibratePermission : IPostGenerateGradleAndroidProject
{
    private const string Permission =
        "<uses-permission android:name=\"android.permission.VIBRATE\" />";

    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
    {
        string manifestPath = Path.Combine(
            unityLibraryPath, "src", "main", "AndroidManifest.xml");

        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning("AndroidVibratePermission: manifest not found at " + manifestPath);
            return;
        }

        string manifest = File.ReadAllText(manifestPath);

        if (manifest.Contains("android.permission.VIBRATE"))
            return;

        int insertAt = manifest.IndexOf("<application", System.StringComparison.Ordinal);

        if (insertAt < 0)
        {
            Debug.LogWarning("AndroidVibratePermission: no <application> tag in manifest.");
            return;
        }

        manifest = manifest.Insert(insertAt, Permission + "\n  ");
        File.WriteAllText(manifestPath, manifest);
    }
}
#endif
