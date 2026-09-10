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

    public int X => x;
    public int Y => y;

    public bool IsOccupied { get; private set; }

    public BuildingType CurrentBuilding { get; private set; } =
        BuildingType.None;

    public int BuildingPopulation { get; private set; }

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
    // SETUP
    // =========================================================

    private void Awake()
    {
        SetHighlight(false);
        ClearBuilding();
    }

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
        BuildingType buildingType,
        int population)
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

        buildingImage.sprite = buildingSprite;
        buildingImage.enabled = true;

        buildingHolder.SetActive(true);

        CurrentBuilding = buildingType;
        BuildingPopulation = population;
        IsOccupied = true;

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

    public void ClearBuilding()
    {
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