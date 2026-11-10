
using UnityEngine;

public class SwingPivotController : MonoBehaviour
{
    [Header("Pendulum Settings")]
    [Min(0f)] public float maxAngle = 35f;

    [Tooltip("Starting swing speed, before any perfect landings.")]
    [Min(0.01f)]
    public float swingSpeed = 2f;

    [Header("Perfect Speed-Up")]
    [Tooltip("How many perfect landings it takes to speed up once.")]
    [Min(1)] public int perfectsPerSpeedUp = 2;

    [Tooltip("Swing speed added each time Perfects Per Speed Up is reached.")]
    [Min(0f)] public float speedIncreasePerStep = 0.05f;

    [Header("Height Speed-Up")]
    [Tooltip("The swing speeds up once every this many floors built.")]
    [Min(1)] public int floorsPerSpeedUp = 5;

    [Tooltip("Swing speed added every Floors Per Speed Up floors.")]
    [Min(0f)] public float speedIncreasePerFloorStep = 0.1f;

    [Header("Limit")]
    [Tooltip("The swing never gets faster than this.")]
    [Min(0.01f)] public float maxSwingSpeed = 3f;

    /// <summary>
    /// Swing speed after this game's perfect landings and floors built.
    /// </summary>
    public float CurrentSwingSpeed
    {
        get
        {
            GameManager manager = GameManager.Instance;

            int perfects = manager != null ? manager.totalPerfectLandings : 0;
            int floors = manager != null ? manager.floorsBuilt : 0;

            // Whole steps only: perfects 0-1 = no change, 2-3 = one step...
            int perfectSteps = perfects / Mathf.Max(1, perfectsPerSpeedUp);

            // Floors 0-4 = no change, 5-9 = one step, 10-14 = two...
            int floorSteps = floors / Mathf.Max(1, floorsPerSpeedUp);

            float speed =
                swingSpeed +
                speedIncreasePerStep * perfectSteps +
                speedIncreasePerFloorStep * floorSteps;

            return Mathf.Min(speed, Mathf.Max(swingSpeed, maxSwingSpeed));
        }
    }

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

        // Speed only scales how fast the phase advances, so a speed change
        // never makes the angle jump.
        swingTime += Time.deltaTime * CurrentSwingSpeed;

        // Continue the same oscillation rather than restarting at zero.
        currentAngle = Mathf.Sin(swingTime) * maxAngle;

        // Direction of travel is the slope of the sine wave. (Multiplying
        // by the old direction flipped it every frame on the way back, so
        // a resumed swing could set off the wrong way.)
        float velocity = Mathf.Cos(swingTime);

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
