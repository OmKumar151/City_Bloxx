
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the landed tower sway from side to side.
///
/// Every landing above Sway Start Floor changes the tower's instability:
/// non-perfect landings raise it (the further off-centre, the more) and
/// perfect landings lower it. Instability sets both how far
/// and how fast the tower sways.
///
/// The tower sways as ONE piece: every landed block is parented to a
/// pivot at the base of the first floor, and the pivot leans left and
/// right. Blocks never slide over each other; the top moves the most
/// because it is furthest from the base.
/// </summary>
public class TowerSway : MonoBehaviour
{
    public static TowerSway Instance { get; private set; }

    [Header("When It Starts")]
    [Tooltip("Landings on this floor or lower never change the sway.")]
    [Min(0)] public int swayStartFloor = 3;

    [Header("Sway Size")]
    [Tooltip("Max sway range: the furthest the TOP of the tower can ever " +
             "move from its resting spot, in world units (blocks are 1 " +
             "unit wide). Lower floors move less, the base not at all.")]
    [Min(0f)] public float maxSwayDistance = 1f;

    [Tooltip("Max lean of the whole tower, in degrees. Keeps short towers " +
             "from tipping over-dramatically: they lean at most this much, " +
             "so a short tower's top moves less than Max Sway Distance.")]
    [Range(0f, 20f)] public float maxLeanAngle = 5f;

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

    [Header("Runtime (read only)")]
    [SerializeField, Range(0f, 1f)] private float instability;
    [SerializeField, Range(0f, 1f)] private float currentSway;

    // Every landed block is a child of this; rotating it leans the tower.
    private Transform pivot;
    private BuildingTier topBlock;

    // Base of the tower to the top of the highest block, tower upright.
    private float towerHeight;
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

        AttachToTower(block);

        if (floor <= swayStartFloor)
            return;

        float change = GetInstabilityChange(accuracy, isPerfect);

        instability = Mathf.Clamp01(instability + change);
    }

    // Makes the landed block part of the tower so it leans with the rest.
    private void AttachToTower(GameObject block)
    {
        Transform blockTransform = block.transform;
        BuildingTier tier = block.GetComponent<BuildingTier>();

        if (pivot == null)
        {
            // Pivot at the bottom centre of the first floor: the base
            // stays put and everything above it leans.
            float baseY = tier != null && tier.HasSprite
                ? tier.SolidBottom
                : blockTransform.position.y;

            pivot = new GameObject("TowerSwayPivot").transform;
            pivot.position = new Vector3(
                blockTransform.position.x,
                baseY,
                blockTransform.position.z
            );
        }

        // Do the stacking with the tower standing straight, so the new
        // block sits squarely on the one below in the tower's own frame.
        Quaternion lean = pivot.rotation;
        pivot.rotation = Quaternion.identity;

        Vector3 fromPivot = blockTransform.position - pivot.position;
        blockTransform.position = pivot.position + Quaternion.Inverse(lean) * fromPivot;
        blockTransform.rotation = Quaternion.identity;

        if (tier != null && tier.HasSprite &&
            topBlock != null && topBlock.HasSprite)
        {
            blockTransform.position +=
                Vector3.up * (topBlock.SolidTop - tier.SolidBottom);
        }

        blockTransform.SetParent(pivot, true);

        if (tier != null)
        {
            topBlock = tier;

            if (tier.HasSprite)
                towerHeight = tier.SolidTop - pivot.position.y;
        }

        // Lean back to where the tower was, now carrying the new block.
        pivot.rotation = lean;
        Physics2D.SyncTransforms();
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

        // Rotating the pivot moves every landed block together. Setting the
        // Transform works with the blocks' kinematic Rigidbody2Ds: Unity
        // copies moved transforms into physics before each physics step.
        if (pivot != null)
            pivot.rotation = Quaternion.Euler(0f, 0f, -GetLeanAngle());
    }

    // Lean of the whole tower right now, in degrees (positive = top to
    // the right). Chosen so the top moves by the sway offset, but never
    // past Max Lean Angle.
    private float GetLeanAngle()
    {
        if (currentSway <= 0f || towerHeight <= 0.01f)
            return 0f;

        float topOffset = Mathf.Sin(phase) * maxSwayDistance *
                          Mathf.Clamp01(currentSway);

        float maxSin = Mathf.Sin(maxLeanAngle * Mathf.Deg2Rad);
        float sin = Mathf.Clamp(topOffset / towerHeight, -maxSin, maxSin);

        return Mathf.Asin(sin) * Mathf.Rad2Deg;
    }
}
