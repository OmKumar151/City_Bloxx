using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Celebrates a perfect landing on the block that just landed:
///  - a quick white flash over the block's own sprite,
///  - a soft golden glow that swells out behind it,
///  - a burst of twinkling stars from its top edge.
/// The effect gets a little bigger with every perfect in a row, and the
/// perfect roof gets the biggest burst.
///
/// GameManager calls Play() on every perfect landing.
/// </summary>
public class PerfectLandingFX : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Soft round glow (Assets/Sprites/FX/PerfectGlow).")]
    public Sprite glowSprite;
    [Tooltip("Twinkle star (Assets/Sprites/FX/PerfectStar).")]
    public Sprite starSprite;

    [Header("Colours")]
    public Color glowColor = new Color(1f, 0.85f, 0.35f, 0.85f);
    public Color starColorA = new Color(1f, 0.92f, 0.45f, 1f);
    public Color starColorB = Color.white;

    [Header("Flash")]
    [Tooltip("Strength of the white flash over the block (0 = off).")]
    [Range(0f, 1f)] public float flashAlpha = 0.6f;
    [Min(0.01f)] public float flashDuration = 0.25f;

    [Header("Glow")]
    [Tooltip("Glow size compared to the block.")]
    [Min(0f)] public float glowSize = 1.9f;
    [Min(0.01f)] public float glowDuration = 0.5f;

    [Header("Stars")]
    [Min(0)] public int starCount = 6;
    [Tooltip("Extra stars for each perfect in a row after the first.")]
    [Min(0)] public int extraStarsPerCombo = 2;
    [Min(0)] public int maxStars = 16;
    [Tooltip("Star size in world units (blocks are about 1 wide).")]
    [Min(0f)] public float starSize = 0.28f;
    [Min(0f)] public float starSpeed = 2.4f;
    [Min(0f)] public float starGravity = 3f;
    [Min(0.01f)] public float starLifetime = 0.7f;

    [Header("Streak / Roof")]
    [Tooltip("Each perfect in a row makes the glow and stars this much bigger.")]
    [Min(0f)] public float sizePerCombo = 0.08f;
    [Tooltip("Size and star multiplier for a perfect roof.")]
    [Min(1f)] public float roofMultiplier = 1.8f;

    [Tooltip("Stars draw this many sorting orders above the block.")]
    public int starSortingOffset = 60;

    private enum Kind { Flash, Glow, Star }

    private class Piece
    {
        public Kind kind;
        public Transform transform;
        public SpriteRenderer renderer;
        public Color color;
        public Vector3 velocity;
        public float spin;
        public float size;
        public float age;
        public float life;
    }

    private readonly List<Piece> pieces = new List<Piece>();

    public void Play(GameObject block, int comboCount, bool isRoof)
    {
        if (block == null || !isActiveAndEnabled)
            return;

        SpriteRenderer blockRenderer = block.GetComponent<SpriteRenderer>();

        Bounds bounds = blockRenderer != null
            ? blockRenderer.bounds
            : new Bounds(block.transform.position, Vector3.one);

        int baseOrder = blockRenderer != null ? blockRenderer.sortingOrder : 100;
        int layerId = blockRenderer != null ? blockRenderer.sortingLayerID : 0;

        float scale = 1f + sizePerCombo * Mathf.Max(0, comboCount - 1);

        if (isRoof)
            scale *= roofMultiplier;

        SpawnFlash(block, blockRenderer);
        SpawnGlow(block, bounds, layerId, baseOrder - 1, scale);

        int stars = starCount + extraStarsPerCombo * Mathf.Max(0, comboCount - 1);

        if (isRoof)
            stars = Mathf.RoundToInt(stars * roofMultiplier);

        stars = Mathf.Min(stars, Mathf.Max(maxStars, isRoof ? maxStars * 2 : 0));

        SpawnStars(bounds, layerId, baseOrder + starSortingOffset, stars, scale);
    }

    // White silhouette of the block's own sprite that fades out.
    private void SpawnFlash(GameObject block, SpriteRenderer blockRenderer)
    {
        if (flashAlpha <= 0f || blockRenderer == null || blockRenderer.sprite == null)
            return;

        SpriteRenderer sr = CreateRenderer("PerfectFlash", block.transform);
        sr.sprite = blockRenderer.sprite;
        sr.flipX = blockRenderer.flipX;
        sr.flipY = blockRenderer.flipY;
        sr.drawMode = blockRenderer.drawMode;
        if (blockRenderer.drawMode != SpriteDrawMode.Simple)
            sr.size = blockRenderer.size;
        sr.sortingLayerID = blockRenderer.sortingLayerID;
        sr.sortingOrder = blockRenderer.sortingOrder + 1;

        // Child at the block's origin with no extra scale: same size as the block.
        sr.transform.localPosition = Vector3.zero;
        sr.transform.localRotation = Quaternion.identity;
        sr.transform.localScale = Vector3.one;

        Add(new Piece
        {
            kind = Kind.Flash,
            transform = sr.transform,
            renderer = sr,
            color = new Color(1f, 1f, 1f, flashAlpha),
            life = flashDuration
        });
    }

    // Golden halo behind the block, parented so it moves with tower sway.
    private void SpawnGlow(GameObject block, Bounds bounds, int layerId, int order, float scale)
    {
        if (glowSprite == null || glowSize <= 0f)
            return;

        SpriteRenderer sr = CreateRenderer("PerfectGlow", null);
        sr.sprite = glowSprite;
        sr.sortingLayerID = layerId;
        sr.sortingOrder = order;
        sr.transform.position = bounds.center;
        sr.transform.SetParent(block.transform, true);

        Vector2 spriteSize = glowSprite.bounds.size;
        float targetWidth = Mathf.Max(bounds.size.x, bounds.size.y) * glowSize * scale;

        Add(new Piece
        {
            kind = Kind.Glow,
            transform = sr.transform,
            renderer = sr,
            color = glowColor,
            size = spriteSize.x > 0f ? targetWidth / spriteSize.x : 1f,
            life = glowDuration
        });
    }

    // Stars burst up and out from along the block's top edge.
    private void SpawnStars(Bounds bounds, int layerId, int order, int count, float scale)
    {
        if (starSprite == null || count <= 0)
            return;

        float spriteWidth = Mathf.Max(0.0001f, starSprite.bounds.size.x);
        float halfWidth = bounds.extents.x;

        for (int i = 0; i < count; i++)
        {
            // Spread evenly across the top, with a little jitter.
            float t = count == 1 ? 0.5f : (i + Random.Range(-0.3f, 0.3f)) / (count - 1);
            float side = Mathf.Lerp(-1f, 1f, Mathf.Clamp01(t));

            Vector3 start = new Vector3(
                bounds.center.x + side * halfWidth,
                bounds.max.y - Random.Range(0f, 0.15f) * bounds.size.y,
                bounds.center.z);

            // Fan outward: edge stars fly sideways, middle ones fly up.
            Vector2 dir = new Vector2(side * 0.9f, 1f).normalized;
            dir = Rotate(dir, Random.Range(-15f, 15f));

            SpriteRenderer sr = CreateRenderer("PerfectStar", transform);
            sr.sprite = starSprite;
            sr.sortingLayerID = layerId;
            sr.sortingOrder = order;
            sr.transform.position = start;
            sr.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 45f));

            Add(new Piece
            {
                kind = Kind.Star,
                transform = sr.transform,
                renderer = sr,
                color = Color.Lerp(starColorA, starColorB, Random.value),
                velocity = dir * starSpeed * Random.Range(0.6f, 1.15f) * Mathf.Sqrt(scale),
                spin = Random.Range(-260f, 260f),
                size = starSize * scale * Random.Range(0.6f, 1.2f) / spriteWidth,
                life = starLifetime * Random.Range(0.75f, 1.15f)
            });
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = pieces.Count - 1; i >= 0; i--)
        {
            Piece p = pieces[i];

            // The block (and its children) may have been destroyed.
            if (p.transform == null)
            {
                pieces.RemoveAt(i);
                continue;
            }

            p.age += dt;
            float t = Mathf.Clamp01(p.age / p.life);

            switch (p.kind)
            {
                case Kind.Flash:
                    SetAlpha(p, 1f - EaseOut(t));
                    break;

                case Kind.Glow:
                    // Swell out quickly, then fade.
                    p.transform.localScale = WorldToLocalScale(
                        p.transform, p.size * Mathf.Lerp(0.7f, 1.15f, EaseOut(t)));
                    SetAlpha(p, t < 0.15f ? t / 0.15f : 1f - (t - 0.15f) / 0.85f);
                    break;

                case Kind.Star:
                    p.velocity += Vector3.down * starGravity * dt;
                    p.velocity *= 1f - Mathf.Clamp01(1.5f * dt);
                    p.transform.position += p.velocity * dt;
                    p.transform.Rotate(0f, 0f, p.spin * dt);

                    // Pop in, then twinkle down to nothing.
                    float grow = t < 0.15f ? EaseOut(t / 0.15f) : 1f - (t - 0.15f) / 0.85f;
                    float twinkle = 0.85f + 0.15f * Mathf.Sin(p.age * 40f);
                    p.transform.localScale = Vector3.one * p.size * Mathf.Max(0f, grow) * twinkle;
                    SetAlpha(p, 1f - t * t);
                    break;
            }

            if (p.age >= p.life)
            {
                Destroy(p.transform.gameObject);
                pieces.RemoveAt(i);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Piece p in pieces)
            if (p.transform != null)
                Destroy(p.transform.gameObject);

        pieces.Clear();
    }

    private void Add(Piece piece)
    {
        SetAlpha(piece, piece.kind == Kind.Flash ? 1f : 0f);
        pieces.Add(piece);
    }

    private static SpriteRenderer CreateRenderer(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent, false);
        return go.AddComponent<SpriteRenderer>();
    }

    private static void SetAlpha(Piece p, float alpha)
    {
        Color c = p.color;
        c.a *= Mathf.Clamp01(alpha);
        p.renderer.color = c;
    }

    // Glow is parented to the block, whose scale is stretched to fit its
    // sprite; undo that so the glow keeps a round shape.
    private static Vector3 WorldToLocalScale(Transform t, float worldSize)
    {
        Vector3 parentScale = t.parent != null ? t.parent.lossyScale : Vector3.one;

        return new Vector3(
            worldSize / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
            worldSize / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)),
            1f);
    }

    private static float EaseOut(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float r = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
