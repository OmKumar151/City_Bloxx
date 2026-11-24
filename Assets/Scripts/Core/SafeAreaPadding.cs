using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a canvas's UI clear of the Dynamic Island, notch, camera hole,
/// rounded corners and home bar, with a little extra padding.
///
/// Put it on a Canvas. Every UI element under it that pokes outside the
/// screen's safe area is nudged back inside. Elements are only moved on
/// edges that actually have a cutout, so phones without one are untouched
/// and the layout in the scene stays exactly as designed.
///
/// Full-screen elements (anchored to all four edges, e.g. backgrounds and
/// panels) are never moved; the elements inside them are checked instead.
///
/// Test it with the Device Simulator (Window > General > Device Simulator)
/// and a phone with a Dynamic Island or notch; the normal Game view has no
/// cutouts, so nothing moves there.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class SafeAreaPadding : MonoBehaviour
{
    [Tooltip("Extra space, in canvas units, kept between the UI and a " +
             "Dynamic Island / notch / home bar. Only added on edges that " +
             "have one.")]
    [Min(0f)] public float extraPadding = 24f;

    [Tooltip("UI elements to leave where they are, e.g. art that is meant " +
             "to go under the notch.")]
    public List<RectTransform> ignore = new List<RectTransform>();

    private Canvas canvas;

    // How far each element has been moved, so it can be put back before
    // re-measuring (e.g. after a rotation or resolution change).
    private readonly Dictionary<RectTransform, Vector2> applied =
        new Dictionary<RectTransform, Vector2>();

    private Rect lastSafeArea;
    private Vector2Int lastScreen;
    private float lastScaleFactor;
    private float lastPadding = -1f;

    // Screen-space limits for this pass (infinite where there's no cutout).
    private float minX, maxX, minY, maxY;
    private Camera canvasCamera;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void OnDisable()
    {
        Restore();
        lastPadding = -1f;
    }

    // LateUpdate: the CanvasScaler sets the canvas scale in Update, so the
    // UI has its final size by now.
    private void LateUpdate()
    {
        if (canvas == null)
            return;

        if (Screen.safeArea == lastSafeArea &&
            lastScreen.x == Screen.width &&
            lastScreen.y == Screen.height &&
            Mathf.Approximately(canvas.scaleFactor, lastScaleFactor) &&
            Mathf.Approximately(extraPadding, lastPadding))
        {
            return;
        }

        lastSafeArea = Screen.safeArea;
        lastScreen = new Vector2Int(Screen.width, Screen.height);
        lastScaleFactor = canvas.scaleFactor;
        lastPadding = extraPadding;

        Apply();
    }

    /// <summary>Re-checks every element now (e.g. after showing new UI).</summary>
    public void Apply()
    {
        Restore();

        if (canvas == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safe = Screen.safeArea;
        float padding = extraPadding * canvas.scaleFactor;

        // Only edges with a cutout limit the UI.
        minX = safe.xMin > 0.5f ? safe.xMin + padding : float.NegativeInfinity;
        maxX = safe.xMax < Screen.width - 0.5f ? safe.xMax - padding : float.PositiveInfinity;
        minY = safe.yMin > 0.5f ? safe.yMin + padding : float.NegativeInfinity;
        maxY = safe.yMax < Screen.height - 0.5f ? safe.yMax - padding : float.PositiveInfinity;

        if (float.IsInfinity(minX) && float.IsInfinity(maxX) &&
            float.IsInfinity(minY) && float.IsInfinity(maxY))
        {
            return;
        }

        canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        Visit(transform);
    }

    private void Visit(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            RectTransform child = parent.GetChild(i) as RectTransform;

            if (child == null || ignore.Contains(child))
                continue;

            if (IsFullScreen(child))
                Visit(child);
            else
                Nudge(child);
        }
    }

    private static bool IsFullScreen(RectTransform rect)
    {
        return rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
    }

    private readonly Vector3[] corners = new Vector3[4];

    private void Nudge(RectTransform rect)
    {
        rect.GetWorldCorners(corners);

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < 4; i++)
        {
            Vector2 p = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[i]);
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }

        Vector2 shift = new Vector2(
            GetShift(min.x, max.x, minX, maxX),
            GetShift(min.y, max.y, minY, maxY)
        );

        if (shift == Vector2.zero)
            return;

        // Screen pixels -> the parent's local units (handles canvas and
        // parent scaling).
        RectTransform parent = rect.parent as RectTransform;

        if (parent == null)
            return;

        Vector2 from = (min + max) * 0.5f;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, from, canvasCamera, out Vector2 localFrom) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, from + shift, canvasCamera, out Vector2 localTo))
        {
            return;
        }

        Vector2 delta = localTo - localFrom;

        rect.anchoredPosition += delta;
        applied[rect] = delta;
    }

    // How far to move a span [low, high] to fit inside [limitLow, limitHigh].
    // Elements larger than the safe area are left alone.
    private static float GetShift(float low, float high, float limitLow, float limitHigh)
    {
        if (high - low > limitHigh - limitLow)
            return 0f;

        if (high > limitHigh)
            return limitHigh - high;

        if (low < limitLow)
            return limitLow - low;

        return 0f;
    }

    private void Restore()
    {
        foreach (KeyValuePair<RectTransform, Vector2> entry in applied)
        {
            if (entry.Key != null)
                entry.Key.anchoredPosition -= entry.Value;
        }

        applied.Clear();
    }
}
