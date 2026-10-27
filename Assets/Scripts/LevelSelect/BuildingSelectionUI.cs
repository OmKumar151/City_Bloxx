
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    [SerializeField] private Vector2 glowDistance = new Vector2(2f, 2f);

    [Header("Board Placement")]
    [SerializeField] private BoardManager boardManager;

    private Outline[] outlines;
    private int currentSelectedBuilding = -1;

    private const int BlueBuildingIndex = 0;

    private void Awake()
    {
        FindBoardManager();

        if (buildingObjects == null)
            buildingObjects = new RectTransform[0];

        if (buildingImages == null)
            buildingImages = new Image[0];

        PrepareBuildingOutlines();
        ResetAllBuildings();
    }

    private void FindBoardManager()
    {
        if (boardManager == null)
            boardManager = FindAnyObjectByType<BoardManager>();
    }

    private void PrepareBuildingOutlines()
    {
        outlines = new Outline[buildingImages.Length];

        for (int i = 0; i < buildingImages.Length; i++)
        {
            if (buildingImages[i] == null)
                continue;

            Outline outline =
                buildingImages[i].GetComponent<Outline>();

            if (outline == null)
            {
                outline =
                    buildingImages[i].gameObject.AddComponent<Outline>();
            }

            outline.effectColor = glowColor;
            outline.effectDistance = glowDistance;
            outline.enabled = false;

            outlines[i] = outline;
        }
    }

    // Updates the selected building's UI only.
    // It must not activate board placement mode.
    public void SetSelectedBuilding(int buildingIndex)
    {
        SetSelectedBuildingVisualOnly(buildingIndex);
    }

    public void SetSelectedBuildingVisualOnly(int buildingIndex)
    {
        if (!IsValidBuildingIndex(buildingIndex))
            return;

        currentSelectedBuilding = buildingIndex;

        ApplySelectionVisual(buildingIndex);
        SelectNavigationObject(buildingIndex);
    }

    // Returns the selection visuals to Blue without
    // starting placement mode.
    public void ResetNavigationToBlue()
    {
        if (!IsValidBuildingIndex(BlueBuildingIndex))
        {
            Debug.LogWarning(
                "BuildingSelectionUI: Cannot select Blue. " +
                "Check the Building Objects array in the Inspector."
            );
            return;
        }

        SetSelectedBuildingVisualOnly(BlueBuildingIndex);

        Debug.Log(
            "BuildingSelectionUI: Selection returned to Blue. " +
            "Placement mode was not started."
        );
    }

    private bool IsValidBuildingIndex(int buildingIndex)
    {
        return buildingObjects != null &&
               buildingObjects.Length > 0 &&
               buildingIndex >= 0 &&
               buildingIndex < buildingObjects.Length;
    }

    private void SelectNavigationObject(int buildingIndex)
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning(
                "BuildingSelectionUI: No EventSystem exists in the scene."
            );
            return;
        }

        if (!IsValidBuildingIndex(buildingIndex) ||
            buildingObjects[buildingIndex] == null)
        {
            Debug.LogWarning(
                "BuildingSelectionUI: Invalid navigation target for index " +
                buildingIndex
            );
            return;
        }

        RectTransform buildingObject = buildingObjects[buildingIndex];

        Selectable selectable =
            buildingObject.GetComponent<Selectable>();

        if (selectable == null)
        {
            selectable =
                buildingObject.GetComponentInParent<Selectable>();
        }

        if (selectable == null)
        {
            selectable =
                buildingObject.GetComponentInChildren<Selectable>();
        }

        if (selectable == null)
        {
            Debug.LogWarning(
                "BuildingSelectionUI: No Selectable was found for building " +
                "index " + buildingIndex + "."
            );
            return;
        }

        if (!selectable.gameObject.activeInHierarchy ||
            !selectable.IsInteractable())
        {
            Debug.LogWarning(
                "BuildingSelectionUI: Navigation target for building index " +
                buildingIndex + " is inactive or not interactable."
            );
            return;
        }

        selectable.Select();
    }

    private void ApplySelectionVisual(int buildingIndex)
    {
        if (buildingObjects == null)
            return;

        for (int i = 0; i < buildingObjects.Length; i++)
        {
            if (buildingObjects[i] != null)
            {
                bool selected = i == buildingIndex;

                buildingObjects[i].localScale =
                    Vector3.one *
                    (selected ? selectedScale : normalScale);
            }

            if (outlines != null &&
                i < outlines.Length &&
                outlines[i] != null)
            {
                outlines[i].effectColor = glowColor;
                outlines[i].effectDistance = glowDistance;
                outlines[i].enabled = i == buildingIndex;
            }
        }
    }

    public void ClearBuildingSelectionGlow()
    {
        currentSelectedBuilding = -1;
        ResetAllBuildings();

        FindBoardManager();

        if (boardManager != null)
            boardManager.EndPlacementMode();
    }

    private void ResetAllBuildings()
    {
        if (buildingObjects != null)
        {
            for (int i = 0; i < buildingObjects.Length; i++)
            {
                if (buildingObjects[i] != null)
                {
                    buildingObjects[i].localScale =
                        Vector3.one * normalScale;
                }
            }
        }

        if (outlines != null)
        {
            for (int i = 0; i < outlines.Length; i++)
            {
                if (outlines[i] != null)
                    outlines[i].enabled = false;
            }
        }
    }

    public int GetCurrentSelectedBuilding()
    {
        return currentSelectedBuilding;
    }
}
