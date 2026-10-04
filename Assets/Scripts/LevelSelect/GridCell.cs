using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    public enum BuildingType
    {
        None,
        Blue,
        Red,
        Green,
        Yellow
    }

    [Header("Grid Position")]
    [SerializeField] private int x;
    [SerializeField] private int y;

    [Header("Selection")]
    [SerializeField] private GameObject highlight;

    [Header("Building")]
    [SerializeField] private GameObject buildingHolder;
    [SerializeField] private Image buildingImage;

    private Image highlightImage;
    private RectTransform highlightRectTransform;

    private static Sprite glowSprite;
    private static Texture2D glowTexture;

    private static Sprite cursorSprite;
    private static Texture2D cursorTexture;

    private Image placementCursorImage;
    private Image placementPreviewImage;

    private RectTransform placementCursorRectTransform;
    private RectTransform placementPreviewRectTransform;

    public int X => x;
    public int Y => y;

    public bool IsOccupied { get; private set; }

    public BuildingType CurrentBuilding { get; private set; } =
        BuildingType.None;

    public int BuildingPopulation { get; private set; }


    // =========================================================
    // BUILDING SCORES
    // =========================================================

    public static int GetBuildingScore(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.Blue:
                return 100;

            case BuildingType.Red:
                return 250;

            case BuildingType.Green:
                return 500;

            case BuildingType.Yellow:
                return 750;

            default:
                return 0;
        }
    }


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        PrepareHighlight();
        PreparePlacementIndicator();

        SetHighlight(false);
        HidePlacementIndicator();
        ClearBuilding();
    }


    // =========================================================
    // HIGHLIGHT SETUP
    // =========================================================

    private void PrepareHighlight()
    {
        if (highlight == null)
        {
            return;
        }

        highlightImage =
            highlight.GetComponent<Image>();

        if (highlightImage == null)
        {
            highlightImage =
                highlight.AddComponent<Image>();
        }

        highlightRectTransform =
            highlight.GetComponent<RectTransform>();

        if (highlightRectTransform == null)
        {
            return;
        }

        // -----------------------------------------------------
        // USE THE SQUARE HALO SPRITE
        // -----------------------------------------------------

        highlightImage.sprite =
            GetGlowSprite();

        highlightImage.type =
            Image.Type.Simple;

        highlightImage.preserveAspect =
            false;

        highlightImage.raycastTarget =
            false;

        // Large default size.
        // BoardManager overrides this when placement glow
        // is shown.
        highlightRectTransform.sizeDelta =
            new Vector2(200f, 200f);

        // IMPORTANT:
        // Keep the glow behind the tile.
        highlight.transform.SetAsFirstSibling();
    }


    // =========================================================
    // SQUARE GLOW / HALO
    // =========================================================
    //
    // Creates a square-shaped glowing border.
    //
    // CENTER:
    // Completely transparent.
    //
    // MIDDLE:
    // Bright colored glow.
    //
    // OUTSIDE:
    // Soft fade.
    //
    // This allows the tile to remain completely visible
    // while the glow is behind it.
    // =========================================================

    private static Sprite GetGlowSprite()
    {
        if (glowSprite != null)
        {
            return glowSprite;
        }

        const int textureSize = 128;

        glowTexture =
            new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.RGBA32,
                false
            );

        glowTexture.name =
            "GridCell_SquarePlacementGlow";

        glowTexture.wrapMode =
            TextureWrapMode.Clamp;

        glowTexture.filterMode =
            FilterMode.Bilinear;

        Color[] pixels =
            new Color[
                textureSize * textureSize
            ];

        float center =
            (textureSize - 1) * 0.5f;

        float halfSize =
            center;


        // -----------------------------------------------------
        // SQUARE GLOW SETTINGS
        // -----------------------------------------------------

        // Transparent area around the tile.
        //
        // The glow starts close to the edge of the tile.
        const float innerEdge = 0.56f;

        // Bright part of the glow.
        const float brightEdge = 0.72f;

        // Maximum outer edge of the glow.
        const float outerEdge = 1.0f;


        for (int py = 0;
             py < textureSize;
             py++)
        {
            for (int px = 0;
                 px < textureSize;
                 px++)
            {
                float normalizedX =
                    Mathf.Abs(
                        (px - center) /
                        halfSize
                    );

                float normalizedY =
                    Mathf.Abs(
                        (py - center) /
                        halfSize
                    );

                // -------------------------------------------------
                // SQUARE DISTANCE
                // -------------------------------------------------
                //
                // Unlike the previous circular glow, this uses
                // the larger of X/Y distance.
                //
                // That creates a square instead of a circle.
                // -------------------------------------------------

                float squareDistance =
                    Mathf.Max(
                        normalizedX,
                        normalizedY
                    );

                float alpha = 0f;


                // -------------------------------------------------
                // TRANSPARENT CENTER
                // -------------------------------------------------

                if (squareDistance <= innerEdge)
                {
                    alpha = 0f;
                }


                // -------------------------------------------------
                // SOFT INNER GLOW
                // -------------------------------------------------

                else if (squareDistance < brightEdge)
                {
                    float t =
                        Mathf.InverseLerp(
                            innerEdge,
                            brightEdge,
                            squareDistance
                        );

                    alpha =
                        Mathf.SmoothStep(
                            0f,
                            0.95f,
                            t
                        );
                }


                // -------------------------------------------------
                // BRIGHT SQUARE BORDER
                // -------------------------------------------------

                else if (squareDistance <= 0.86f)
                {
                    alpha = 1f;
                }


                // -------------------------------------------------
                // SOFT OUTER FADE
                // -------------------------------------------------

                else if (squareDistance < outerEdge)
                {
                    float t =
                        Mathf.InverseLerp(
                            0.86f,
                            outerEdge,
                            squareDistance
                        );

                    alpha =
                        Mathf.SmoothStep(
                            1f,
                            0f,
                            t
                        );
                }


                // -------------------------------------------------
                // OUTSIDE
                // -------------------------------------------------

                else
                {
                    alpha = 0f;
                }


                pixels[
                    py * textureSize + px
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }

        glowTexture.SetPixels(pixels);
        glowTexture.Apply();

        glowSprite =
            Sprite.Create(
                glowTexture,
                new Rect(
                    0f,
                    0f,
                    textureSize,
                    textureSize
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );

        glowSprite.name =
            "GridCell_SquarePlacementGlow";

        return glowSprite;
    }


    // =========================================================
    // PLACEMENT INDICATOR
    // =========================================================

    private void PreparePlacementIndicator()
    {
        if (placementCursorImage == null)
        {
            GameObject cursorObject =
                new GameObject(
                    "Placement Cursor",
                    typeof(RectTransform),
                    typeof(Image)
                );

            cursorObject.transform.SetParent(
                transform,
                false
            );

            placementCursorImage =
                cursorObject.GetComponent<Image>();

            placementCursorRectTransform =
                cursorObject.GetComponent<RectTransform>();

            placementCursorImage.sprite =
                GetCursorSprite();

            placementCursorImage.type =
                Image.Type.Simple;

            placementCursorImage.preserveAspect =
                false;

            placementCursorImage.raycastTarget =
                false;

            placementCursorRectTransform.anchorMin =
                new Vector2(0.5f, 0.5f);

            placementCursorRectTransform.anchorMax =
                new Vector2(0.5f, 0.5f);

            placementCursorRectTransform.pivot =
                new Vector2(0.5f, 0.5f);

            placementCursorRectTransform.anchoredPosition =
                Vector2.zero;
        }


        if (placementPreviewImage == null)
        {
            GameObject previewObject =
                new GameObject(
                    "Placement Building Preview",
                    typeof(RectTransform),
                    typeof(Image)
                );

            previewObject.transform.SetParent(
                transform,
                false
            );

            placementPreviewImage =
                previewObject.GetComponent<Image>();

            placementPreviewRectTransform =
                previewObject.GetComponent<RectTransform>();

            placementPreviewImage.type =
                Image.Type.Simple;

            placementPreviewImage.preserveAspect =
                true;

            placementPreviewImage.raycastTarget =
                false;

            placementPreviewRectTransform.anchorMin =
                new Vector2(0.5f, 0.5f);

            placementPreviewRectTransform.anchorMax =
                new Vector2(0.5f, 0.5f);

            placementPreviewRectTransform.pivot =
                new Vector2(0.5f, 0.5f);

            placementPreviewRectTransform.anchoredPosition =
                Vector2.zero;
        }

        UpdatePlacementIndicatorOrder();
        HidePlacementIndicator();
    }


    // =========================================================
    // UI ORDER
    // =========================================================

    private void UpdatePlacementIndicatorOrder()
    {
        // -----------------------------------------------------
        // GLOW
        // -----------------------------------------------------
        //
        // The glow MUST remain behind the tile.
        // -----------------------------------------------------

        if (highlight != null)
        {
            highlight.transform.SetAsFirstSibling();
        }


        // -----------------------------------------------------
        // PLACEMENT CURSOR
        // -----------------------------------------------------

        if (placementCursorImage != null)
        {
            placementCursorImage.transform.SetAsLastSibling();
        }


        // -----------------------------------------------------
        // BUILDING PREVIEW
        // -----------------------------------------------------

        if (placementPreviewImage != null)
        {
            placementPreviewImage.transform.SetAsLastSibling();
        }
    }


    // =========================================================
    // CURSOR SPRITE
    // =========================================================

    private static Sprite GetCursorSprite()
    {
        if (cursorSprite != null)
        {
            return cursorSprite;
        }

        const int textureSize = 64;
        const int borderSize = 4;

        cursorTexture =
            new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.RGBA32,
                false
            );

        cursorTexture.name =
            "GridCell_PlacementCursor";

        cursorTexture.wrapMode =
            TextureWrapMode.Clamp;

        cursorTexture.filterMode =
            FilterMode.Bilinear;

        Color[] pixels =
            new Color[
                textureSize * textureSize
            ];

        for (int y = 0;
             y < textureSize;
             y++)
        {
            for (int x = 0;
                 x < textureSize;
                 x++)
            {
                bool border =
                    x < borderSize ||
                    x >= textureSize - borderSize ||
                    y < borderSize ||
                    y >= textureSize - borderSize;

                pixels[
                    y * textureSize + x
                ] =
                    border
                        ? Color.white
                        : Color.clear;
            }
        }

        cursorTexture.SetPixels(pixels);
        cursorTexture.Apply();

        cursorSprite =
            Sprite.Create(
                cursorTexture,
                new Rect(
                    0f,
                    0f,
                    textureSize,
                    textureSize
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );

        cursorSprite.name =
            "GridCell_PlacementCursor";

        return cursorSprite;
    }


    // =========================================================
    // PLACEMENT INDICATOR
    // =========================================================

    public void SetPlacementIndicator(
        bool visible,
        Sprite buildingSprite,
        Color color,
        float cursorSize,
        float previewAlpha)
    {
        PreparePlacementIndicator();

        if (!visible)
        {
            HidePlacementIndicator();
            return;
        }

        if (placementCursorImage != null)
        {
            placementCursorImage.enabled = true;
            placementCursorImage.color = color;
        }

        if (placementCursorRectTransform != null)
        {
            placementCursorRectTransform.sizeDelta =
                new Vector2(
                    cursorSize,
                    cursorSize
                );
        }

        if (placementPreviewImage != null)
        {
            placementPreviewImage.sprite =
                buildingSprite;

            placementPreviewImage.enabled =
                buildingSprite != null;

            Color previewColor =
                Color.white;

            previewColor.a =
                Mathf.Clamp01(
                    previewAlpha
                );

            placementPreviewImage.color =
                previewColor;
        }

        if (placementPreviewRectTransform != null)
        {
            placementPreviewRectTransform.sizeDelta =
                new Vector2(
                    cursorSize,
                    cursorSize
                );
        }

        UpdatePlacementIndicatorOrder();
    }


    public void HidePlacementIndicator()
    {
        if (placementCursorImage != null)
        {
            placementCursorImage.enabled = false;
        }

        if (placementPreviewImage != null)
        {
            placementPreviewImage.enabled = false;
            placementPreviewImage.sprite = null;
        }
    }


    // =========================================================
    // NORMAL HIGHLIGHT
    // =========================================================

    public void SetHighlight(bool selected)
    {
        if (highlight == null)
        {
            return;
        }

        if (selected)
        {
            PrepareHighlight();
        }

        highlight.SetActive(selected);

        if (selected && highlightImage != null)
        {
            highlightImage.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.45f
                );
        }
    }


    // =========================================================
    // PLACEMENT GLOW
    // =========================================================

    public void SetPlacementHighlight(
        bool selected,
        Color color,
        float glowSize,
        float intensity)
    {
        if (highlight == null)
        {
            return;
        }

        PrepareHighlight();

        // -----------------------------------------------------
        // THIS CONTROLS WHETHER THE GLOW IS VISIBLE.
        //
        // BoardManager calls this with "true" when the player
        // has selected a building and the cell is valid.
        // -----------------------------------------------------

        highlight.SetActive(selected);

        if (!selected ||
            highlightImage == null)
        {
            return;
        }

        // -----------------------------------------------------
        // SIZE
        // -----------------------------------------------------

        if (highlightRectTransform != null)
        {
            highlightRectTransform.sizeDelta =
                new Vector2(
                    glowSize,
                    glowSize
                );
        }

        // -----------------------------------------------------
        // KEEP GLOW BEHIND TILE
        // -----------------------------------------------------

        highlight.transform.SetAsFirstSibling();

        // -----------------------------------------------------
        // APPLY BUILDING COLOR
        // -----------------------------------------------------

        color.a =
            Mathf.Clamp01(
                color.a * intensity
            );

        highlightImage.color =
            color;
    }


    // =========================================================
    // BUILDING
    // =========================================================

    public void SetBuilding(
        Sprite buildingSprite,
        BuildingType buildingType,
        int population)
    {
        if (buildingHolder == null)
        {
            Debug.LogWarning(
                "GridCell " +
                name +
                ": Building Holder is not assigned."
            );

            return;
        }

        if (buildingImage == null)
        {
            Debug.LogWarning(
                "GridCell " +
                name +
                ": Building Image is not assigned."
            );

            return;
        }

        if (buildingSprite == null)
        {
            Debug.LogWarning(
                "GridCell " +
                name +
                ": Building sprite is null."
            );

            return;
        }

        buildingImage.sprite =
            buildingSprite;

        buildingImage.enabled =
            true;

        buildingHolder.SetActive(
            true
        );

        CurrentBuilding =
            buildingType;

        BuildingPopulation =
            population;

        IsOccupied =
            true;

        Debug.Log(
            "Building placed on Cell (" +
            x +
            ", " +
            y +
            "): " +
            buildingType +
            " | Population: " +
            population
        );
    }


    // =========================================================
    // CLEAR BUILDING
    // =========================================================

    public void ClearBuilding()
    {
        HidePlacementIndicator();

        if (buildingHolder != null)
        {
            buildingHolder.SetActive(false);
        }

        if (buildingImage != null)
        {
            buildingImage.sprite = null;
            buildingImage.enabled = false;
        }

        IsOccupied = false;

        CurrentBuilding =
            BuildingType.None;

        BuildingPopulation = 0;
    }
}
