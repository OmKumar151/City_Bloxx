using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    Empty,
    Blue,
    Red,
    Green,
    Yellow
}

public class GridCell : MonoBehaviour
{
    [Header("Grid Position")]
    public int x;
    public int y;

    [Header("Can this tile be used?")]
    public bool isBuildable = true;

    [Header("Current Building")]
    public BuildingType currentBuilding = BuildingType.Empty;

    [Header("References")]
    public Image tileImage;
    public Image highlight;
    public Transform buildingAnchor;

    public bool IsEmpty()
    {
        return currentBuilding == BuildingType.Empty;
    }

    public void SetHighlight(Color color)
    {
        highlight.gameObject.SetActive(true);
        highlight.color = color;
    }

    public void HideHighlight()
    {
        highlight.gameObject.SetActive(false);
    }
}