using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeRenderer : MonoBehaviour
{
    [Header("Rope Connections")]
    public Transform ropeStart;
    public Transform hook;

    [Header("Rope Appearance")]
    public float ropeWidth = 0.06f;
    public Color ropeColor = Color.black;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        SetupRope();
    }

    private void LateUpdate()
    {
        if (ropeStart == null || hook == null)
            return;

        line.enabled = true;

        line.SetPosition(0, ropeStart.position);
        line.SetPosition(1, hook.position);
    }

    private void SetupRope()
    {
        line.positionCount = 2;

        line.startWidth = ropeWidth;
        line.endWidth = ropeWidth;

        line.startColor = ropeColor;
        line.endColor = ropeColor;

        line.useWorldSpace = true;

        line.numCapVertices = 2;

        // Make the rope render above the background/building.
        line.sortingLayerName = "Default";
        line.sortingOrder = 50;

        // Create a simple visible material automatically.
        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            line.material = new Material(shader);
            line.material.color = ropeColor;
        }
    }
}