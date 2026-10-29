
using UnityEngine;

public class SwingPivotController : MonoBehaviour
{
    [Header("Pendulum Settings")]
    [Min(0f)] public float maxAngle = 35f;

    [Min(0.01f)]
    public float swingSpeed = 2f;

    private bool isSwinging;

    // The current angle relative to the initial rotation.
    private float currentAngle;

    // Tracks elapsed swing time while the rope is moving.
    private float swingTime;

    // Direction of travel is preserved when the swing pauses.
    private float swingDirection = 1f;

    private float initialAngle;

    public bool IsSwinging => isSwinging;
    public float CurrentAngle => initialAngle + currentAngle;

    private void Awake()
    {
        initialAngle = NormalizeAngle(transform.localEulerAngles.z);

        currentAngle = 0f;
        swingTime = 0f;
        swingDirection = 1f;

        ApplyRotation();
    }

    private void Update()
    {
        if (!isSwinging)
            return;

        swingTime += Time.deltaTime * swingSpeed;

        // Continue the same oscillation rather than restarting at zero.
        currentAngle = Mathf.Sin(swingTime) * maxAngle;

        float velocity = Mathf.Cos(swingTime) * swingDirection;

        if (velocity > 0.001f)
            swingDirection = 1f;
        else if (velocity < -0.001f)
            swingDirection = -1f;

        ApplyRotation();
    }

    public void StartSwing()
    {
        if (isSwinging)
            return;

        // Reconstruct a matching phase from the current angle.
        // This prevents snapping back to the center when resuming.
        if (Mathf.Abs(maxAngle) > 0.001f)
        {
            float normalizedAngle = Mathf.Clamp(
                currentAngle / maxAngle,
                -1f,
                1f
            );

            float phase = Mathf.Asin(normalizedAngle);

            // Choose the matching half of the sine wave based on direction.
            if (swingDirection < 0f)
                phase = Mathf.PI - phase;

            swingTime = phase;
        }

        isSwinging = true;
    }

    public void StopSwing()
    {
        isSwinging = false;

        // Keep the current angle and phase so swinging can resume.
        ApplyRotation();
    }

    public void ResetSwing()
    {
        isSwinging = false;
        currentAngle = 0f;
        swingTime = 0f;
        swingDirection = 1f;

        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(
            0f,
            0f,
            initialAngle + currentAngle
        );
    }

    private float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
