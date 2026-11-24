using UnityEngine;

/// <summary>
/// Keeps the crane planted on the ground while its cab climbs.
///
/// The crane is drawn in three pieces: the cab + jib (moves with the
/// crane), the feet (fixed in the world) and this mast between them. The
/// mast is one truss segment drawn Tiled, standing on the feet. As the cab
/// rises, whole segments are added so the truss pattern never slides; the
/// top segment ends hidden behind the cab.
///
/// Without this the whole crane sprite rose with the cab, which shows on
/// wide screens (tablets) where the mast is in view.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CraneMast : MonoBehaviour
{
    [Tooltip("The cab + jib renderer that rises with the crane. The mast " +
             "grows until it reaches this sprite's bottom edge.")]
    public SpriteRenderer craneTop;

    [Tooltip("How far the mast reaches past the cab's bottom edge, so no " +
             "gap shows. Must be less than the cab's height minus one " +
             "segment, or the mast pokes out above the cab.")]
    [Min(0f)] public float overlap = 0.03f;

    private SpriteRenderer mast;
    private int segments = -1;

    private void Awake()
    {
        mast = GetComponent<SpriteRenderer>();
        mast.drawMode = SpriteDrawMode.Tiled;
    }

    private void LateUpdate()
    {
        if (craneTop == null || mast.sprite == null)
            return;

        float segmentHeight =
            mast.sprite.rect.height / mast.sprite.pixelsPerUnit;

        float segmentWorldHeight = segmentHeight * transform.lossyScale.y;

        if (segmentWorldHeight <= 0f)
            return;

        float needed =
            craneTop.bounds.min.y + overlap - transform.position.y;

        int wanted = Mathf.Max(1, Mathf.CeilToInt(needed / segmentWorldHeight));

        if (wanted == segments)
            return;

        segments = wanted;
        mast.size = new Vector2(mast.size.x, segments * segmentHeight);
    }
}
