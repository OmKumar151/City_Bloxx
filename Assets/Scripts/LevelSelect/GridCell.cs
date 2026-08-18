using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    [Header("Grid Position")]
    [SerializeField] private int x;
    [SerializeField] private int y;

    [Header("Selection")]
    [SerializeField] private GameObject highlight;

    [Header("Building")]
    [SerializeField] private GameObject buildingHolder;
    [SerializeField] private Image buildingImage;

    public int X => x;
    public int Y => y;

    public bool IsOccupied { get; private set; }


    private void Awake()
    {
        // Every cell starts unselected.
        SetHighlight(false);

        // Every cell starts without a building.
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

    public void SetBuilding(Sprite buildingSprite)
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


        // Assign the correct building sprite.
        buildingImage.sprite = buildingSprite;

        // Make sure the Image itself is enabled.
        buildingImage.enabled = true;

        // Show the building holder.
        buildingHolder.SetActive(true);

        // Mark this cell as occupied.
        IsOccupied = true;

        Debug.Log(
            "Building placed on Cell (" +
            x + ", " +
            y + ")"
        );
    }


    public void ClearBuilding()
    {
        if (buildingHolder != null)
        {
            buildingHolder.SetActive(false);
        }

        IsOccupied = false;
    }
}