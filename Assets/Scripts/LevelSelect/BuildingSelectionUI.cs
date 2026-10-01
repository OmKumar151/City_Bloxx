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
        // Update the building-selection UI.
        // -----------------------------------------------------

        ApplySelectionVisual(
            buildingIndex
        );

        // -----------------------------------------------------
        // Find BoardManager if necessary.
        // -----------------------------------------------------

        if (boardManager == null)
        {
            boardManager =
                FindFirstObjectByType<BoardManager>();
        }

        if (boardManager == null)
        {
            Debug.LogWarning(
                "BuildingSelectionUI: " +
                "BoardManager is not assigned."
            );

            return;
        }

        // -----------------------------------------------------
        // Tell BoardManager which building is selected.
        //
        // This will:
        //
        // 1. Find all valid placement cells.
        // 2. Glow those cells.
        // 3. Find the first valid cell.
        // 4. Put the placement cursor there.
        // 5. Show the building preview.
        // -----------------------------------------------------

        boardManager.ShowPlacementHighlights(
            buildingIndex
        );

        Debug.Log(
            "BuildingSelectionUI: Selected building " +
            buildingIndex +
            " and refreshed placement highlights."
        );
    }


    // =========================================================
    // BUILDING VISUAL ONLY
    // =========================================================
    //
    // Used when returning from board placement.
    //
    // IMPORTANT:
    // This intentionally does NOT call
    // ShowPlacementHighlights().
    //
    // BoardManager handles clearing the board visuals when
    // placement is finished/cancelled.
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