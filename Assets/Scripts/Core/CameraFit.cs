using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps the game world from being cropped on narrow or tall phones.
///
/// The scenes were laid out for a 1080x1920 (9:16) screen at the camera's
/// orthographic size. With a fixed size, a taller phone (e.g. 20:9) sees
/// less width, cutting off the crane swing and the edges of the tower.
/// This zooms out just enough that the designed WIDTH always fits; taller
/// screens see extra sky, wider screens (tablets) see extra sides.
///
/// Added to the main camera of every scene automatically.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [Tooltip("Screen shape the scene was designed for (width / height).")]
    public float designAspect = 1080f / 1920f;

    [Tooltip("On screens wider than designed (tablets), how much to zoom " +
             "out. 0 = keep the designed height (the world looks big on " +
             "a tablet). 1 = keep the designed screen area. 0.5 sits " +
             "between, like a Canvas Scaler match of 0.5.")]
    [Range(0f, 1f)] public float wideScreenZoomOut = 0f;

    [Tooltip("Widest view allowed in world units, so the camera never " +
             "zooms out past the edges of the background. 0 = no limit.")]
    [Min(0f)] public float maxVisibleWidth = 0f;

    private Camera cam;
    private float designSize;
    private int lastWidth;
    private int lastHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded += (_, _) => AttachToMainCamera();
    }

    private static void AttachToMainCamera()
    {
        Camera main = Camera.main;

        if (main != null && main.orthographic && main.GetComponent<CameraFit>() == null)
            main.gameObject.AddComponent<CameraFit>();
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        designSize = cam.orthographicSize;
        Apply();
    }

    // Screen size can change at runtime (rotation, Device Simulator).
    private void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
            Apply();
    }

    private void Apply()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        if (lastHeight <= 0)
            return;

        float aspect = (float)lastWidth / lastHeight;

        // Narrower than designed: grow the size so the width still fits.
        if (aspect < designAspect)
        {
            cam.orthographicSize = designSize * designAspect / aspect;
            return;
        }

        // Wider than designed: zoom out part of the way.
        float size = designSize *
            Mathf.Pow(aspect / designAspect, wideScreenZoomOut);

        if (maxVisibleWidth > 0f)
            size = Mathf.Min(size, maxVisibleWidth / (2f * aspect));

        cam.orthographicSize = Mathf.Max(size, designSize);
    }
}
