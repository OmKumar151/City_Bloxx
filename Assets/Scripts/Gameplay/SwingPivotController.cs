
using UnityEngine;

public class SwingPivotController : MonoBehaviour
{
    [Header("Pendulum Settings")]
    [Min(0f)] public float maxAngle = 35f;
    [Min(0f)] public float swingSpeed = 2f;

    private bool isSwinging;
    private float startAngle;
    private float swingStartTime;

    private void Awake()
    {
        startAngle = NormalizeAngle(transform.localEulerAngles.z);
    }

    private void Update()
    {
        if (!isSwinging)
            return;

        float elapsed = (Time.time - swingStartTime) * swingSpeed;
        float angle = startAngle + Mathf.Sin(elapsed) * maxAngle;

        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void StartSwing()
    {
        if (isSwinging)
            return;

        isSwinging = true;
        swingStartTime = Time.time;
    }

    public void StopSwing()
    {
        isSwinging = false;
        transform.localRotation =
            Quaternion.Euler(0f, 0f, startAngle);
    }

    private float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
