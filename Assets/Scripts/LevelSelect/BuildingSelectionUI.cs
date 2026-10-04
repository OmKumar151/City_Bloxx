using UnityEngine;
using UnityEngine.UI;

public class BuildingSelectionUI : MonoBehaviour
{
    [Header("Building Objects")]
    [SerializeField] private RectTransform[] buildingObjects;

    [Header("Building Images")]
    [SerializeField] private Image[] buildingImages;

    [Header("Selection Settings")]
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float normalScale = 1.0f;

    [Header("Building Button Glow")]
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField]
    private Vector2 glowDistance =
        new Vector2(2f, 2f);

    [Header("Board Placement")]
    [SerializeField] private BoardManager boardManager;

    private Outline[] outlines;

    private int currentSelectedBuilding = -1;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (boardManager == null)
        {
            boardManager =
                FindFirstObjectByType<BoardManager>();
        }

        if (buildingObjects == null)
        {
            buildingObjects =
                new RectTransform[0];
        }

        if (buildingImages == null)
        {
            buildingImages =
                new Image[0];
        }

        PrepareBuildingOutlines();

        ResetAllBuildings();
    }


    // =========================================================
    // BUILDING OUTLINE SETUP
    // =========================================================

    private void PrepareBuildingOutlines()
    {
        outlines =
            new Outline[buildingImages.Length];

        for (int i = 0;
             i < buildingImages.Length;
             i++)
        {
            if (buildingImages[i] == null)
            {
                continue;
            }

            Outline outline =
                buildingImages[i]
                    .GetComponent<Outline>();

            if (outline == null)
            {
                outline =
                    buildingImages[i]
                        .gameObject
                        .AddComponent<Outline>();
            }

            outline.effectColor =
                glowColor;

            outline.effectDistance =
                glowDistance;

            outline.enabled = false;

            outlines[i] =
                outline;
        }
    }


    // =========================================================
    // SELECT BUILDING
    // =========================================================

    public void SetSelectedBuilding(
        int buildingIndex)
    {
        if (buildingObjects == null ||
            buildingObjects.Length == 0)
        {
            return;
        }

        if (buildingIndex < 0 ||
            buildingIndex >= buildingObjects.Length)
        {
            return;
        }

        currentSelectedBuilding =
            buildingIndex;

        // -----------------------------------------------------
        // Update ONLY the building-selection UI.
        //
        // Placement highlights are intentionally NOT updated
        // here.
        //
        // The player is still in Building Selection Mode.
        // Board placement begins only after pressing OK.
        // -----------------------------------------------------

        ApplySelectionVisual(
            buildingIndex
        );

        Debug.Log(
            "BuildingSelectionUI: Selected building " +
            buildingIndex +
            ". Placement visuals will appear after OK."
        );
    }


    // =========================================================
    // BUILDING VISUAL ONLY
    // =========================================================
    //
    // Used when returning from board placement.
    //
    // This only restores the selected building's UI visual.
    // It does NOT interact with the board.
    // =========================================================

    public void SetSelectedBuildingVisualOnly(
        int buildingIndex)
    {
        if (buildingObjects == null ||
            buildingObjects.Length == 0)
        {
            return;
        }

        if (buildingIndex < 0 ||
            buildingIndex >= buildingObjects.Length)
        {
            return;
        }

        currentSelectedBuilding =
            buildingIndex;

        ApplySelectionVisual(
            buildingIndex
        );
    }


    // =========================================================
    // APPLY BUILDING SELECTION VISUAL
    // =========================================================

    private void ApplySelectionVisual(
        int buildingIndex)
    {
        for (int i = 0;
             i < buildingObjects.Length;
             i++)
        {
            if (buildingObjects[i] != null)
            {
                bool selected =
                    i == buildingIndex;

                buildingObjects[i].localScale =
                    Vector3.one *
                    (
                        selected
                            ? selectedScale
                            : normalScale
                    );
            }

            if (outlines != null &&
                i < outlines.Length &&
                outlines[i] != null)
            {
                outlines[i].effectColor =
                    glowColor;

                outlines[i].effectDistance =
                    glowDistance;

                outlines[i].enabled =
                    i == buildingIndex;
            }
        }
    }


    // =========================================================
    // CLEAR BUILDING SELECTION
    // =========================================================

    public void ClearBuildingSelectionGlow()
    {
        currentSelectedBuilding = -1;

        ResetAllBuildings();

        // Also make sure no board placement visuals remain.
        if (boardManager == null)
        {
            boardManager =
                FindFirstObjectByType<BoardManager>();
        }

        if (boardManager != null)
        {
            boardManager.ClearPlacementHighlights();
        }
    }


    // =========================================================
    // RESET BUILDING VISUALS
    // =========================================================

    private void ResetAllBuildings()
    {
        if (buildingObjects == null)
        {
            return;
        }

        for (int i = 0;
             i < buildingObjects.Length;
             i++)
        {
            if (buildingObjects[i] != null)
            {
                buildingObjects[i].localScale =
                    Vector3.one *
                    normalScale;
            }

            if (outlines != null &&
                i < outlines.Length &&
                outlines[i] != null)
            {
                outlines[i].enabled = false;
            }
        }
    }


    // =========================================================
    // GET CURRENT BUILDING
    // =========================================================

    public int GetCurrentSelectedBuilding()
    {
        return currentSelectedBuilding;
    }
}