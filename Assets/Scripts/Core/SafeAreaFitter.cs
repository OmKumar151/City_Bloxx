using UnityEngine;

/// <summary>
/// Shrinks this RectTransform to the screen's safe area so UI never sits
/// under a notch, camera hole, rounded corner or gesture bar.
///
/// Usage: under a Screen Space - Overlay canvas, create an empty child
/// stretched to fill the canvas (anchors 0,0 to 1,1, offsets 0), add this
/// component, and move the HUD elements (score, floors, lives, buttons)
/// inside it. Keep full-screen backgrounds OUTSIDE it so they still reach
/// the screen edges.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rect;
    private Rect lastSafeArea;
    private Vector2Int lastScreen;

    private void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (Screen.safeArea != lastSafeArea ||
            lastScreen.x != Screen.width ||
            lastScreen.y != Screen.height)
        {
            Apply();
        }
    }

    private void Apply()
    {
        lastSafeArea = Screen.safeArea;
        lastScreen = new Vector2Int(Screen.width, Screen.height);

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Vector2 min = lastSafeArea.position;
        Vector2 max = lastSafeArea.position + lastSafeArea.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
