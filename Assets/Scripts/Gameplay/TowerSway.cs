
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the landed tower sway from side to side.
///
/// Every landing above Sway Start Floor changes the tower's instability:
/// non-perfect landings raise it (the further off-centre, the more) and
/// perfect landings lower it. Instability sets both how far
/// and how fast the tower sways. Only the top few landed blocks (just
/// under the hanging block) sway: the highest moves most, the ones under
/// it less, and everything below stays still.
/// </summary>
public class TowerSway : MonoBehaviour
{
    public static TowerSway Instance { get; private set; }

    [Header("When It Starts")]
    [Tooltip("Landings on this floor or lower never change the sway.")]
    [Min(0)] public int swayStartFloor = 3;

    [Header("Sway Size")]
    [Tooltip("Max sway range: the furthest any floor can ever move from " +
             "its resting spot, in world units (blocks are 1 unit wide). " +
             "This is a hard limit: tower height and the number of " +
             "imperfect landings never push the sway past it.")]
    [Min(0f)] public float maxSwayDistance = 1f;

    [Tooltip("How many of the highest landed blocks sway. The top one " +
             "moves the full amount, each one below it a little less; " +
             "blocks lower down stay still.")]
    [Min(1)] public int swayingTopBlocks = 3;

    [Tooltip("How quickly a block eases in or out of the sway when a new " +
             "block lands and the swaying top moves up.")]
    [Min(0.01f)] public float handoverSpeed = 2f;

    [Header("Sway Speed")]
    [Tooltip("Sway speed when the tower has only just started swaying " +
             "(0.8 = one full swing about every 8 seconds).")]
    [Min(0f)] public float minSwaySpeed = 0.8f;

    [Tooltip("Sway speed at full instability.")]
    [Min(0f)] public float maxSwaySpeed = 1.6f;

    [Header("Landing Effect (instability is 0 to 1)")]
    [Tooltip("Instability added by a non-perfect landing just outside " +
             "the perfect zone.")]
    [Range(0f, 1f)] public float minImperfectIncrease = 0.12f;

    [Tooltip("Instability added by the most off-centre landing that " +
             "still stays on the tower.")]
    [Range(0f, 1f)] public float maxImperfectIncrease = 0.25f;

    [Tooltip("Off-centre distance, as a share of the block width, that " +
             "counts as the worst landing (0.7 = only 30% overlap).")]
    [Range(0.01f, 1f)] public float worstOffCentre = 0.7f;

    [Tooltip("Instability removed by a perfect landing.")]
    [Range(0f, 1f)] public float perfectDecrease = 0.35f;

    [Header("Smoothing")]
    [Tooltip("How quickly the sway grows or shrinks to its new size.")]
    [Min(0.01f)] public float responseSpeed = 3f;

    [Header("Debugging")]
    public bool logChanges = true;

    [Header("Runtime (read only)")]
    [SerializeField, Range(0f, 1f)] private float instability;
    [SerializeField, Range(0f, 1f)] private float currentSway;

    private class SwayingBlock
    {
        public Transform transform;
        public float restX;

        // Share of the sway this block shows (1 = top block), eased so a
        // block leaving or joining the swaying top never jumps.
        public float weight;
    }

    private readonly List<SwayingBlock> blocks = new List<SwayingBlock>();
    private float phase;

    public float Instability => instability;

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
    /// Called by GameManager for every successful landing.
    /// </summary>
    public void RegisterLanding(
        GameObject block,
        int floor,
        float accuracy,
        bool isPerfect)
    {
        if (block == null)
            return;

        // The new block becomes the top: it sways fully from where it
        // landed. Store where it would be with the tower standing straight.
        blocks.Add(new SwayingBlock
        {
            transform = block.transform,
            restX = block.transform.position.x - GetOffset(1f),
            weight = 1f
        });

        if (floor <= swayStartFloor)
            return;

        float previous = instability;
        float change = GetInstabilityChange(accuracy, isPerfect);

        instability = Mathf.Clamp01(instability + change);

        if (logChanges)
        {
            Debug.Log(
                $"TowerSway: Floor {floor} landed {(isPerfect ? "PERFECT" : "off-centre")} " +
                $"(accuracy {accuracy:F2}). Instability {previous:F2} -> {instability:F2}.",
                this
            );
        }
    }

    // Perfect landings calm the tower. Every other landing makes it sway
    // more: a little just outside the perfect zone, the most at the edge.
    private float GetInstabilityChange(float accuracy, bool isPerfect)
    {
        if (isPerfect)
            return -perfectDecrease;

        float offCentre = 1f - Mathf.Clamp01(accuracy);
        float severity = Mathf.InverseLerp(0f, worstOffCentre, offCentre);

        return Mathf.Lerp(minImperfectIncrease, maxImperfectIncrease, severity);
    }

    // Every frame (not FixedUpdate) so the motion is smooth; Unity copies
    // the moved transforms into physics before each physics step.
    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Ease toward the target so the sway never jumps.
        currentSway = Mathf.MoveTowards(
            currentSway,
            instability,
            responseSpeed * deltaTime
        );

        float speed = Mathf.Lerp(minSwaySpeed, maxSwaySpeed, currentSway);

        // Only the phase rate changes with speed, so no position jumps.
        phase += speed * deltaTime;

        if (phase > Mathf.PI * 2f)
            phase -= Mathf.PI * 2f;

        MoveBlocks(deltaTime);
    }

    private void MoveBlocks(float deltaTime)
    {
        blocks.RemoveAll(b => b == null || b.transform == null);

        int top = blocks.Count - 1;
        int swaying = Mathf.Max(1, swayingTopBlocks);

        for (int i = top; i >= 0; i--)
        {
            SwayingBlock block = blocks[i];

            // Top block 1, then e.g. 2/3, 1/3, then 0 for the rest.
            int rank = top - i;
            float targetWeight = rank < swaying
                ? 1f - rank / (float)swaying
                : 0f;

            // Blocks below the top that had already settled stay put, and so
            // does everything under them.
            if (rank >= swaying && block.weight <= 0f)
                break;

            // Updated after the check so a block reaching 0 this frame is
            // still placed exactly back on its resting spot.
            block.weight = Mathf.MoveTowards(
                block.weight,
                targetWeight,
                handoverSpeed * deltaTime
            );

            // Set the Transform's X directly. The blocks' Rigidbody2D has
            // Freeze Position X (so dropped blocks fall straight down),
            // which blocks Rigidbody2D.MovePosition sideways; that is why
            // the tower never moved before.
            Vector3 position = block.transform.position;
            position.x = block.restX + GetOffset(block.weight);
            block.transform.position = position;
        }
    }

    // Sideways offset right now for a block with this share of the sway.
    // The foundation is never registered, so it never moves.
    private float GetOffset(float weight)
    {
        if (weight <= 0f || currentSway <= 0f)
            return 0f;

        float offset = Mathf.Sin(phase) * maxSwayDistance *
                       Mathf.Clamp01(currentSway) * Mathf.Clamp01(weight);

        // Hard limit, whatever the other settings are.
        return Mathf.Clamp(offset, -maxSwayDistance, maxSwayDistance);
    }
}
