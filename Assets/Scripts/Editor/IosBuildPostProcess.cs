#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

/// <summary>
/// Adjusts the generated Xcode project:
///   - App Tracking Transparency (TrackingPermission): links the framework
///     and adds the prompt text. iOS rejects apps that ask without it.
///   - Sets the short home-screen name.
/// </summary>
public static class IosBuildPostProcess
{
    private const string TrackingText =
        "Your data will be used to show you more relevant ads.";

    private const string HomeScreenName = "Stackhaven";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS)
            return;

        string projectPath = PBXProject.GetPBXProjectPath(path);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);

        // Weak link: the framework only exists on iOS 14+.
        project.AddFrameworkToProject(
            project.GetUnityFrameworkTargetGuid(),
            "AppTrackingTransparency.framework",
            true);

        project.WriteToFile(projectPath);

        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSUserTrackingUsageDescription", TrackingText);

        // Name under the home-screen icon. The full product name
        // ("Stackhaven City") would be cut off; the App Store listing
        // keeps the full name.
        plist.root.SetString("CFBundleDisplayName", HomeScreenName);
        plist.WriteToFile(plistPath);
    }
}
#endif
