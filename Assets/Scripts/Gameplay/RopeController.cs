
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeController : MonoBehaviour
{
    [Header("Rope Connections")]
    public Transform topPoint;
    public Transform hook;

    [Header("Rope Appearance")]
    [Min(0.001f)] public float ropeWidth = 0.05f;
    public Color ropeColor = Color.black;
    public string sortingLayerName = "Default";
    public int sortingOrder = 50;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;

        ApplyAppearance();
        UpdateRope();
    }

    private void LateUpdate()
    {
        UpdateRope();
    }

    /// <summary>
    /// Updates the rope endpoints.
    /// Can also be called by CraneController when the crane moves.
    /// </summary>
    public void UpdateRope()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        if (line == null)
            return;

        if (topPoint == null || hook == null)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;
        line.positionCount = 2;
        line.useWorldSpace = true;

        line.SetPosition(0, topPoint.position);
        line.SetPosition(1, hook.position);
    }

    private void ApplyAppearance()
    {
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

        if (line == null)
            line = GetComponent<LineRenderer>();

        if (line != null && Application.isPlaying)
        {
            ApplyAppearance();
            UpdateRope();
        }
    }
}
