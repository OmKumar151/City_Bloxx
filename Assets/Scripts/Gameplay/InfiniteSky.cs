
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Endless sky for Quick Play. The sky tiles are stacked edge to edge, and
/// whenever the lowest tile drops fully below the camera it is moved on top
/// of the highest one, so the sky never runs out however tall the tower gets.
/// </summary>
public class InfiniteSky : MonoBehaviour
{
    [Header("Tiles")]
    [Tooltip("The duplicated sky sprites (Sky, Sky (1), Sky (3)...). " +
             "Two or more; they need to cover the screen height together.")]
    public List<SpriteRenderer> skyTiles = new List<SpriteRenderer>();

    [Tooltip("Optional: sky sprite used for every tile in Quick Play. " +
             "Leave empty to keep the current sky.")]
    public Sprite quickPlaySkySprite;

    [Header("Behaviour")]
    [Tooltip("Leave the normal (LevelSelect) game's sky exactly as placed.")]
    public bool onlyInQuickPlay = true;

    [Tooltip("How much each tile overlaps the one below, in world units, " +
             "so no thin gap shows between them.")]
    [Min(0f)] public float seamOverlap = 0.02f;

    [Tooltip("Camera to follow. Main Camera if left empty.")]
    public Camera targetCamera;

    // Lowest tile first.
    private readonly List<SpriteRenderer> tiles = new List<SpriteRenderer>();
    private bool isActive;

    private void Start()
    {
        isActive = !onlyInQuickPlay || GameplaySelection.IsQuickPlay;

        if (!isActive)
            return;

        foreach (SpriteRenderer tile in skyTiles)
        {
            if (tile != null && !tiles.Contains(tile))
                tiles.Add(tile);
        }

        if (quickPlaySkySprite != null)
        {
            foreach (SpriteRenderer tile in tiles)
                tile.sprite = quickPlaySkySprite;
        }

        tiles.RemoveAll(tile => tile.sprite == null);

        if (tiles.Count < 2)
        {
            Debug.LogWarning(
                "InfiniteSky: Assign at least two sky tiles.",
                this
            );

            isActive = false;
            return;
        }

        if (targetCamera == null)
            targetCamera = Camera.main;

        StackTiles();
    }

    // Keep the lowest tile where it is and line the rest up above it.
    private void StackTiles()
    {
        tiles.Sort((a, b) => GetBottom(a).CompareTo(GetBottom(b)));

        for (int i = 1; i < tiles.Count; i++)
            PlaceAbove(tiles[i], tiles[i - 1]);
    }

    private void LateUpdate()
    {
        if (!isActive || targetCamera == null)
            return;

        float cameraBottom = targetCamera.orthographic
            ? targetCamera.transform.position.y - targetCamera.orthographicSize
            : targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, -targetCamera.transform.position.z)).y;

        // At most one full cycle per frame, in case the camera jumped.
        for (int i = 0; i < tiles.Count; i++)
        {
            SpriteRenderer lowest = tiles[0];

            if (GetTop(lowest) >= cameraBottom)
                break;

            PlaceAbove(lowest, tiles[tiles.Count - 1]);

            tiles.RemoveAt(0);
            tiles.Add(lowest);
        }
    }

    private void PlaceAbove(SpriteRenderer tile, SpriteRenderer below)
    {
        float targetBottom = GetTop(below) - seamOverlap;

        tile.transform.position +=
            Vector3.up * (targetBottom - GetBottom(tile));
    }

    // Worked out from the sprite and transform rather than renderer.bounds,
    // which can lag a frame behind a sprite swap or a move made in code.
    private static float GetBottom(SpriteRenderer tile)
    {
        return tile.transform.position.y +
               tile.sprite.bounds.min.y * tile.transform.lossyScale.y;
    }

    private static float GetTop(SpriteRenderer tile)
    {
        return tile.transform.position.y +
               tile.sprite.bounds.max.y * tile.transform.lossyScale.y;
    }
}
