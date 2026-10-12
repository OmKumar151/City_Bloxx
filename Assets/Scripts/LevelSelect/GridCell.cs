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


    [Header("Placement Glow")]
    [SerializeField]
    private float defaultGlowPadding = 6f;


    private Image highlightImage;

    private RectTransform highlightRectTransform;

    private RectTransform cellRectTransform;


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


    public bool IsOccupied
    {
        get;
        private set;
    }


    public BuildingType CurrentBuilding
    {
        get;
        private set;
    } =
        BuildingType.None;


    public int BuildingPopulation
    {
        get;
        private set;
    }


    // =========================================================
    // BUILDING SCORES
    // =========================================================

    public static int GetBuildingScore(
        BuildingType buildingType)
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
        cellRectTransform =
            GetComponent<RectTransform>();

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
        // GLOW SPRITE
        // -----------------------------------------------------

        highlightImage.sprite =
            GetGlowSprite();

        highlightImage.type =
            Image.Type.Simple;

        highlightImage.preserveAspect =
            false;

        highlightImage.raycastTarget =
            false;


        // -----------------------------------------------------
        // INITIAL SIZE
        // -----------------------------------------------------
        //
        // Use the actual GridCell dimensions.
        //
        // The glow is NOT a giant fixed-size object anymore.
        // -----------------------------------------------------

        Vector2 cellSize =
            GetCellSize();


        highlightRectTransform.sizeDelta =
            cellSize +
            new Vector2(
                defaultGlowPadding,
                defaultGlowPadding
            );


        // -----------------------------------------------------
        // GLOW MUST BE BEHIND THE CELL
        // -----------------------------------------------------

        highlight.transform.SetAsFirstSibling();
    }


    // =========================================================
    // GET ACTUAL CELL SIZE
    // =========================================================

    private Vector2 GetCellSize()
    {
        if (cellRectTransform == null)
        {
            cellRectTransform =
                GetComponent<RectTransform>();
        }


        if (cellRectTransform == null)
        {
            return new Vector2(
                50f,
                50f
            );
        }


        Vector2 size =
            cellRectTransform.rect.size;


        // Safety fallback in case the RectTransform
        // has not calculated its size yet.
        if (size.x <= 0f ||
            size.y <= 0f)
        {
            size =
                cellRectTransform.sizeDelta;
        }


        if (size.x <= 0f)
        {
            size.x = 50f;
        }


        if (size.y <= 0f)
        {
            size.y = 50f;
        }


        return size;
    }


    // =========================================================
    // SQUARE GLOW / HALO
    // =========================================================
    //
    // The glow texture itself is still a square halo.
    //
    // IMPORTANT:
    //
    // The center is transparent.
    //
    // The GridCell sits in front of this image.
    //
    // Therefore only the outer edge of the halo is visible,
    // making it look like the colour is coming from BEHIND
    // the grid cell.
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
                textureSize *
                textureSize
            ];


        float center =
            (textureSize - 1) *
            0.5f;


        float halfSize =
            center;


        // -----------------------------------------------------
        // GLOW PROFILE
        // -----------------------------------------------------
        //
        // The centre is completely transparent.
        //
        // The visible glow is concentrated close to the
        // outside edge.
        //
        // This works with the GridCell being placed above it.
        // -----------------------------------------------------

        const float innerEdge = 0.56f;

        const float brightEdge = 0.72f;

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


                float squareDistance =
                    Mathf.Max(
                        normalizedX,
                        normalizedY
                    );


                float alpha =
                    0f;


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
                // BRIGHT EDGE
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
                    py * textureSize +
                    px
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }


        glowTexture.SetPixels(
            pixels
        );

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
    // PLACEMENT INDICATOR SETUP
    // =========================================================

    private void PreparePlacementIndicator()
    {
        // -----------------------------------------------------
        // CURSOR
        // -----------------------------------------------------

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
                new Vector2(
                    0.5f,
                    0.5f
                );


            placementCursorRectTransform.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );


            placementCursorRectTransform.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );


            placementCursorRectTransform.anchoredPosition =
                Vector2.zero;
        }


        // -----------------------------------------------------
        // BUILDING PREVIEW
        // -----------------------------------------------------

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
                new Vector2(
                    0.5f,
                    0.5f
                );


            placementPreviewRectTransform.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );


            placementPreviewRectTransform.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );


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

        if (highlight != null)
        {
            highlight.transform.SetAsFirstSibling();
        }


        // -----------------------------------------------------
        // CURSOR
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
                textureSize *
                textureSize
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
                    y * textureSize +
                    x
                ] =
                    border
                        ? Color.white
                        : Color.clear;
            }
        }


        cursorTexture.SetPixels(
            pixels
        );

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


        // -----------------------------------------------------
        // CURSOR
        // -----------------------------------------------------

        if (placementCursorImage != null)
        {
            placementCursorImage.enabled =
                true;

            placementCursorImage.color =
                color;
        }


        if (placementCursorRectTransform != null)
        {
            placementCursorRectTransform.sizeDelta =
                new Vector2(
                    cursorSize,
                    cursorSize
                );
        }


        // -----------------------------------------------------
        // BUILDING PREVIEW
        // -----------------------------------------------------

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
            placementCursorImage.enabled =
                false;
        }


        if (placementPreviewImage != null)
        {
            placementPreviewImage.enabled =
                false;

            placementPreviewImage.sprite =
                null;
        }
    }


    // =========================================================
    // NORMAL HIGHLIGHT
    // =========================================================

    public void SetHighlight(
        bool selected)
    {
        if (highlight == null)
        {
            return;
        }


        if (selected)
        {
            PrepareHighlight();
        }


        highlight.SetActive(
            selected
        );


        if (selected &&
            highlightImage != null)
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
    //
    // IMPORTANT CHANGE:
    //
    // "glowPadding" is NOT the size of the glow.
    //
    // It is how much larger than the ACTUAL GRID CELL
    // the glow is allowed to be.
    //
    // Example:
    //
    // Cell = 50 x 50
    // Padding = 6
    //
    // Glow = 56 x 56
    //
    // The GridCell itself sits in front of the glow,
    // leaving only the 3px-ish halo around each side.
    // =========================================================

    public void SetPlacementHighlight(
        bool selected,
        Color color,
        float glowPadding,
        float intensity)
    {
        if (highlight == null)
        {
            return;
        }


        PrepareHighlight();


        // -----------------------------------------------------
        // TURN GLOW OFF
        // -----------------------------------------------------

        highlight.SetActive(
            selected
        );


        if (!selected ||
            highlightImage == null)
        {
            return;
        }


        // -----------------------------------------------------
        // GET ACTUAL GRID CELL SIZE
        // -----------------------------------------------------

        Vector2 cellSize =
            GetCellSize();


        // -----------------------------------------------------
        // ADD ONLY A SMALL AMOUNT AROUND THE CELL
        // -----------------------------------------------------

        float padding =
            Mathf.Max(
                0f,
                glowPadding
            );


        highlightRectTransform.sizeDelta =
            cellSize +
            new Vector2(
                padding,
                padding
            );


        // -----------------------------------------------------
        // KEEP GLOW DIRECTLY BEHIND CELL
        // -----------------------------------------------------

        highlight.transform.SetAsFirstSibling();


        // -----------------------------------------------------
        // APPLY COLOUR + INTENSITY
        // -----------------------------------------------------

        Color glowColor =
            color;


        glowColor.a =
            Mathf.Clamp01(
                glowColor.a *
                intensity
            );


        highlightImage.color =
            glowColor;
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
            buildingHolder.SetActive(
                false
            );
        }


        if (buildingImage != null)
        {
            buildingImage.sprite =
                null;

            buildingImage.enabled =
                false;
        }


        IsOccupied =
            false;


        CurrentBuilding =
            BuildingType.None;


        BuildingPopulation =
            0;
    }
}