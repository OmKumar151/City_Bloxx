using UnityEngine;
using UnityEngine.UI;

public class TutorialTarget : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetId;

    [Header("Optional Custom Bounds")]
    [SerializeField] private RectTransform customRect;

    [Header("Highlight Settings")]
    [SerializeField] private float padding = 20f;

    public string TargetId => targetId;

    public RectTransform RectTransform
    {
        get
        {
            if (customRect != null)
            {
                return customRect;
            }

            return GetComponent<RectTransform>();
        }
    }

    public Rect GetScreenRect()
    {
        RectTransform rectTransform = RectTransform;

        if (rectTransform != null)
        {
            return GetUIObjectScreenRect(rectTransform);
        }

        Renderer targetRenderer = GetComponent<Renderer>();

        if (targetRenderer != null)
        {
            return GetRendererScreenRect(targetRenderer);
        }

        Collider targetCollider = GetComponent<Collider>();

        if (targetCollider != null)
        {
            return GetColliderScreenRect(targetCollider);
        }

        return new Rect(
            Screen.width * 0.5f,
            Screen.height * 0.5f,
            100f,
            100f
        );
    }

    private Rect GetUIObjectScreenRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];

        rectTransform.GetWorldCorners(corners);

        Vector2 min = new Vector2(
            float.MaxValue,
            float.MaxValue
        );

        Vector2 max = new Vector2(
            float.MinValue,
            float.MinValue
        );

        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                null,
                corners[i]
            );

            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        Rect result = Rect.MinMaxRect(
            min.x,
            min.y,
            max.x,
            max.y
        );

        result.xMin -= padding;
        result.xMax += padding;
        result.yMin -= padding;
        result.yMax += padding;

        return result;
    }

    private Rect GetRendererScreenRect(Renderer renderer)
    {
        Bounds bounds = renderer.bounds;

        Vector3[] points =
        {
            bounds.min,
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),

            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
            bounds.max
        };

        return GetWorldPointsScreenRect(points);
    }

    private Rect GetColliderScreenRect(Collider collider)
    {
        Bounds bounds = collider.bounds;

        Vector3[] points =
        {
            bounds.min,
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),

            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
            bounds.max
        };

        return GetWorldPointsScreenRect(points);
    }

    private Rect GetWorldPointsScreenRect(Vector3[] points)
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            return new Rect(
                Screen.width * 0.5f,
                Screen.height * 0.5f,
                100f,
                100f
            );
        }

        Vector2 min = new Vector2(
            float.MaxValue,
            float.MaxValue
        );

        Vector2 max = new Vector2(
            float.MinValue,
            float.MinValue
        );

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(points[i]);

            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        Rect result = Rect.MinMaxRect(
            min.x,
            min.y,
            max.x,
            max.y
        );

        result.xMin -= padding;
        result.xMax += padding;
        result.yMin -= padding;
        result.yMax += padding;

        return result;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            targetId = gameObject.name;
        }
    }
}