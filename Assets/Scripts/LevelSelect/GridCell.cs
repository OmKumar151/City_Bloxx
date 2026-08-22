using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    // =========================================================
    // BUILDING TYPES
    // =========================================================

    public enum BuildingType
    {
        None,
        Blue,
        Red,
        Green,
        Yellow
    }


    // =========================================================
    // GRID POSITION
    // =========================================================

    [Header("Grid Position")]
    [SerializeField] private int x;
    [SerializeField] private int y;


    // =========================================================
    // SELECTION
    // =========================================================

    [Header("Selection")]
    [SerializeField] private GameObject highlight;


    // =========================================================
    // BUILDING
    // =========================================================

    [Header("Building")]
    [SerializeField] private GameObject buildingHolder;
    [SerializeField] private Image buildingImage;


    // =========================================================
    // PUBLIC PROPERTIES
    // =========================================================

    public int X => x;
    public int Y => y;

    public bool IsOccupied { get; private set; }

    public BuildingType CurrentBuilding { get; private set; }
        = BuildingType.None;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        // Every cell starts unselected.
        SetHighlight(false);

        // Every cell starts empty.
        ClearBuilding();
    }


    // =========================================================
    // SELECTION
    // =========================================================

    public void SetHighlight(bool selected)
    {
        if (highlight != null)
        {
            highlight.SetActive(selected);
        }
    }


    // =========================================================
    // BUILDING
    // =========================================================

    public void SetBuilding(
        Sprite buildingSprite,
        BuildingType buildingType)
    {
        if (buildingHolder == null)
        {
            Debug.LogWarning(
                "GridCell " + name +
                ": Building Holder is not assigned."
            );

            return;
        }


        if (buildingImage == null)
        {
            Debug.LogWarning(
                "GridCell " + name +
                ": Building Image is not assigned."
            );

            return;
        }


        if (buildingSprite == null)
        {
            Debug.LogWarning(
                "GridCell " + name +
                ": Building sprite is null."
            );

            return;
        }


        // -----------------------------------------------------
        // Assign the building sprite
        // -----------------------------------------------------

        buildingImage.sprite = buildingSprite;

        // Make sure the Image component is enabled.
        buildingImage.enabled = true;

        // Make the building visible.
        buildingHolder.SetActive(true);


        // -----------------------------------------------------
        // Store building information
        // -----------------------------------------------------

        CurrentBuilding = buildingType;

        IsOccupied = true;


        Debug.Log(
            "Building placed on Cell (" +
            x +
            ", " +
            y +
            "): " +
            buildingType
        );
    }


    // =========================================================
    // CLEAR BUILDING
    // =========================================================

    public void ClearBuilding()
    {
        if (buildingHolder != null)
        {
            buildingHolder.SetActive(false);
        }

        if (buildingImage != null)
        {
            buildingImage.sprite = null;
        }

        IsOccupied = false;

        CurrentBuilding = BuildingType.None;
    }


    // =========================================================
    // DEBUG INFORMATION
    // =========================================================

    public override string ToString()
    {
        return "GridCell (" +
               x +
               ", " +
               y +
               ") - " +
               CurrentBuilding;
    }
}