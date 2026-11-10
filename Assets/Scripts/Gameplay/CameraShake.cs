
using UnityEngine;

/// <summary>
/// Short, decaying camera shake, e.g. when a dropped block hits the tower.
///
/// The shake is an offset added after CameraFollow has moved the camera and
/// removed again at the start of the next frame, so following, the opening
/// pan and "only move up" never see it.
/// </summary>
[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Landing")]
    [Tooltip("Shake for a perfect, centred landing (world units).")]
    [Min(0f)] public float perfectLandingShake = 0.06f;

    [Tooltip("Shake for the most off-centre landing that still stays on.")]
    [Min(0f)] public float offCentreLandingShake = 0.16f;

    [Tooltip("Shake when a block misses and a life is lost.")]
    [Min(0f)] public float missShake = 0.2f;

    [Tooltip("Shake the moment the block is released from the hook " +
             "(0 = none).")]
    [Min(0f)] public float releaseShake = 0f;

    [Header("Feel")]
    [Tooltip("Seconds for the single dip down and back.")]
    [Min(0.01f)] public float duration = 0.12f;

    [Tooltip("Sideways nudge compared to the dip (0 = straight down).")]
    [Range(0f, 1f)] public float horizontalFactor = 0.3f;

    [Tooltip("Hard limit on the offset.")]
    [Min(0f)] public float maxOffset = 0.35f;

    private float strength;
    private float timeLeft;
    private float sideDirection = 1f;
    private Vector3 appliedOffset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// One dip down and back. Ignored while a shake is still playing, so
    /// an event can never shake the camera twice.
    /// </summary>
    public void Shake(float amount)
    {
        if (amount <= 0f || timeLeft > 0f)
            return;

        strength = amount;
        timeLeft = duration;
        sideDirection = Random.value < 0.5f ? -1f : 1f;
    }

    /// <summary>Landing shake: harder the further off-centre (accuracy 0-1).</summary>
    public void ShakeForLanding(float accuracy, bool isPerfect)
    {
        float amount = isPerfect
            ? perfectLandingShake
            : Mathf.Lerp(offCentreLandingShake, perfectLandingShake, Mathf.Clamp01(accuracy));

        Shake(amount);
    }

    public void ShakeForMiss() => Shake(missShake);

    public void ShakeForRelease() => Shake(releaseShake);

    // Before CameraFollow (LateUpdate): take last frame's offset back out.
    private void Update()
    {
        RemoveOffset();
    }

    // After CameraFollow (execution order 1000): add this frame's offset.
    private void LateUpdate()
    {
        RemoveOffset();

        if (timeLeft <= 0f)
            return;

        // Unscaled so a shake still settles when the game pauses on the
        // fail panel.
        timeLeft = Mathf.Max(0f, timeLeft - Time.unscaledDeltaTime);

        // 0 -> 1 over the shake. sin(pi * p) goes out and back exactly once.
        float progress = 1f - timeLeft / duration;
        float dip = Mathf.Sin(progress * Mathf.PI) * strength;

        Vector3 offset = new Vector3(
            dip * horizontalFactor * sideDirection,
            -dip,
            0f
        );

        offset = Vector3.ClampMagnitude(offset, maxOffset);

        transform.position += offset;
        appliedOffset = offset;
    }

    private void RemoveOffset()
    {
        if (appliedOffset == Vector3.zero)
            return;

        transform.position -= appliedOffset;
        appliedOffset = Vector3.zero;
    }

    private void OnDisable()
    {
        RemoveOffset();
    }
}
