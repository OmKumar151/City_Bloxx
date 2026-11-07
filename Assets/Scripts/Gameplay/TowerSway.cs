
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the landed tower sway from side to side.
///
/// Every landing above Sway Start Floor changes the tower's instability:
/// non-perfect landings raise it (the further off-centre, the more) and
/// perfect landings lower it. Instability sets both how far
/// and how fast the tower sways. The tower bends from its base, so the
/// lower floors move less than the upper ones.
/// </summary>
public class TowerSway : MonoBehaviour
{
    public static TowerSway Instance { get; private set; }

    [Header("When It Starts")]
    [Tooltip("Landings on this floor or lower never change the sway.")]
    [Min(0)] public int swayStartFloor = 5;

    [Header("Sway Size")]
    [Tooltip("Max sway range: the furthest any floor can ever move from " +
             "its resting spot, in world units (blocks are 1 unit wide). " +
             "This is a hard limit: tower height and the number of " +
             "imperfect landings never push the sway past it.")]
    [Min(0f)] public float maxSwayDistance = 0.45f;

    [Tooltip("Floors above the foundation over which the bend builds up. " +
             "Lower floors move less; floors this high or higher move the " +
             "full distance.")]
    [Min(1)] public int floorsToFullSway = 6;

    [Header("Sway Speed")]
    [Tooltip("Sway speed when the tower has only just started swaying " +
             "(2 = one full swing about every 3 seconds).")]
    [Min(0f)] public float minSwaySpeed = 2f;

    [Tooltip("Sway speed at full instability.")]
    [Min(0f)] public float maxSwaySpeed = 4f;

    [Header("Landing Effect (instability is 0 to 1)")]
    [Tooltip("Instability added by a non-perfect landing just outside " +
             "the perfect zone.")]
    [Range(0f, 1f)] public float minImperfectIncrease = 0.25f;

    [Tooltip("Instability added by the most off-centre landing that " +
             "still stays on the tower.")]
    [Range(0f, 1f)] public float maxImperfectIncrease = 0.5f;

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

    private struct SwayingBlock
    {
        public Transform transform;
        public Rigidbody2D body;
        public float restX;
        public int floor;
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

        // Store where the block would be if the tower stood straight, so
        // it sways with the floors below it from where it landed.
        float offset = GetOffset(floor);

        blocks.Add(new SwayingBlock
        {
            transform = block.transform,
            body = block.GetComponent<Rigidbody2D>(),
            restX = block.transform.position.x - offset,
            floor = floor
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

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

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

        MoveBlocks();
    }

    private void MoveBlocks()
    {
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            SwayingBlock block = blocks[i];

            if (block.transform == null)
            {
                blocks.RemoveAt(i);
                continue;
            }

            float x = block.restX + GetOffset(block.floor);

            // Kinematic bodies are moved through physics so a falling block
            // collides with where the tower actually is.
            if (block.body != null && block.body.bodyType == RigidbodyType2D.Kinematic)
            {
                block.body.MovePosition(new Vector2(x, block.body.position.y));
            }
            else
            {
                Vector3 position = block.transform.position;
                position.x = x;
                block.transform.position = position;
            }
        }
    }

    // Sideways offset of a floor right now. Floor 0 (foundation) never moves.
    private float GetOffset(int floor)
    {
        if (floor <= 0 || currentSway <= 0f)
            return 0f;

        // The bend stops growing at Floors To Full Sway, so a 40-floor tower
        // sways no further than a 10-floor one.
        float bend = Mathf.Clamp01(floor / (float)Mathf.Max(1, floorsToFullSway));

        float offset = Mathf.Sin(phase) * maxSwayDistance *
                       Mathf.Clamp01(currentSway) * bend;

        // Hard limit, whatever the other settings are.
        return Mathf.Clamp(offset, -maxSwayDistance, maxSwayDistance);
    }
}
