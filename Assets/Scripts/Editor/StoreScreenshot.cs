using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Saves App Store screenshots from Play mode:
/// Tools > Stackhaven > Capture Screenshot (Ctrl/Cmd + Shift + K).
///
/// Set the Game view (or Device Simulator) to an App Store size first:
///   iPhone 6.9": 1320 x 2868        iPad 13": 2064 x 2752
/// Files go to the Screenshots folder next to Assets (not committed),
/// named with their size so each set is easy to upload.
/// </summary>
public static class StoreScreenshot
{
    private const string Folder = "Screenshots";

    [MenuItem("Tools/Stackhaven/Capture Screenshot %#k")]
    private static void Capture()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Capture Screenshot",
                "Enter Play mode first, then capture.",
                "OK");
            return;
        }

        Directory.CreateDirectory(Folder);

        string file = Path.Combine(
            Folder,
            $"{Screen.width}x{Screen.height}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        ScreenCapture.CaptureScreenshot(file);
        Debug.Log("Screenshot saved: " + Path.GetFullPath(file));
    }

    [MenuItem("Tools/Stackhaven/Open Screenshots Folder")]
    private static void OpenFolder()
    {
        Directory.CreateDirectory(Folder);
        EditorUtility.RevealInFinder(Folder);
    }
}
