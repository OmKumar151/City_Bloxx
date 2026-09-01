using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeController : MonoBehaviour
{
    public Transform topPoint;
    public Transform hook;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;

        line.startWidth = 0.05f;
        line.endWidth = 0.05f;

        line.numCapVertices = 2;

        line.sortingOrder = 100;
    }

    private void LateUpdate()
    {
        if (topPoint == null || hook == null)
            return;

        line.SetPosition(0, topPoint.position);
        line.SetPosition(1, hook.position);
    }
}