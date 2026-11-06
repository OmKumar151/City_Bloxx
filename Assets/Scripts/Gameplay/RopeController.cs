
using UnityEngine;

/// <summary>
/// Draws the crane rope from the top point (pulley) to the hook.
/// With a rope sprite it stretches the sprite between the two points
/// (9-slice, so the eyelets at the ends keep their shape). Without one it
/// falls back to a plain LineRenderer.
/// </summary>
public class RopeController : MonoBehaviour
{
    [Header("Rope Connections")]
    public Transform topPoint;
    public Transform hook;

    [Header("Rope Appearance")]
    [Tooltip("On-screen width of the rope, in world units.")]
    [Min(0.001f)] public float ropeWidth = 0.12f;
    public Color ropeColor = Color.black;
    public string sortingLayerName = "Default";
    public int sortingOrder = 100;

    [Header("Rope Sprite")]
    [Tooltip("Optional. Found on this object if left empty. When it has a " +
             "sprite, the sprite is used instead of the line.")]
    [SerializeField] private SpriteRenderer ropeSprite;

    private LineRenderer line;
    private HookController hookController;

    private bool UsesSprite => ropeSprite != null && ropeSprite.sprite != null;

    private void Awake()
    {
        FindComponents();
        ApplyAppearance();
        UpdateRope();
    }

    private void LateUpdate()
    {
        UpdateRope();
    }

    private void FindComponents()
    {
        if (ropeSprite == null)
            ropeSprite = GetComponent<SpriteRenderer>();

        if (line == null)
            line = GetComponent<LineRenderer>();

        if (hookController == null && hook != null)
            hookController = hook.GetComponent<HookController>();
    }

    /// <summary>
    /// Updates the rope endpoints.
    /// Can also be called by CraneController when the crane moves.
    /// </summary>
    public void UpdateRope()
    {
        FindComponents();

        bool hasEnds = topPoint != null && hook != null;

        if (UsesSprite)
        {
            if (line != null)
                line.enabled = false;

            ropeSprite.enabled = hasEnds;

            if (hasEnds)
                StretchSprite(topPoint.position, GetHookPoint());

            return;
        }

        if (line == null)
            return;

        line.enabled = hasEnds;

        if (!hasEnds)
            return;

        line.positionCount = 2;
        line.useWorldSpace = true;

        line.SetPosition(0, topPoint.position);
        line.SetPosition(1, GetHookPoint());
    }

    private Vector3 GetHookPoint()
    {
        return hookController != null
            ? hookController.RopeAttachPoint
            : hook.position;
    }

    // Centres the sprite between the two points, turns it to face along
    // the rope and sets its length. Uniform scale sets the width, so the
    // end eyelets keep their proportions; the sliced height sets the length.
    private void StretchSprite(Vector3 start, Vector3 end)
    {
        Sprite sprite = ropeSprite.sprite;

        Vector3 direction = end - start;
        direction.z = 0f;

        float length = direction.magnitude;

        if (length < 0.0001f)
        {
            ropeSprite.enabled = false;
            return;
        }

        float naturalWidth = sprite.rect.width / sprite.pixelsPerUnit;

        if (naturalWidth <= 0f)
            return;

        float scale = ropeWidth / naturalWidth;

        transform.position = new Vector3(
            (start.x + end.x) * 0.5f,
            (start.y + end.y) * 0.5f,
            transform.position.z
        );

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Cancel any parent scale so the width is exact.
        Vector3 parentScale = transform.parent != null
            ? transform.parent.lossyScale
            : Vector3.one;

        transform.localScale = new Vector3(
            scale / SafeScale(parentScale.x),
            scale / SafeScale(parentScale.y),
            1f
        );

        ropeSprite.drawMode = SpriteDrawMode.Sliced;
        ropeSprite.size = new Vector2(naturalWidth, length / scale);
    }

    private static float SafeScale(float value)
    {
        return Mathf.Abs(value) < 0.0001f ? 1f : value;
    }

    private void ApplyAppearance()
    {
        if (ropeSprite != null)
        {
            ropeSprite.sortingLayerName = sortingLayerName;
            ropeSprite.sortingOrder = sortingOrder;
        }

        if (line == null)
            return;

        line.startWidth = ropeWidth;
        line.endWidth = ropeWidth;

        line.startColor = ropeColor;
        line.endColor = ropeColor;

        line.numCapVertices = 2;
        line.sortingLayerName = sortingLayerName;
        line.sortingOrder = sortingOrder;

        if (line.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader != null)
                line.material = new Material(shader);
        }
    }

    private void OnValidate()
    {
        ropeWidth = Mathf.Max(0.001f, ropeWidth);

        if (Application.isPlaying)
        {
            FindComponents();
            ApplyAppearance();
            UpdateRope();
        }
    }
}
