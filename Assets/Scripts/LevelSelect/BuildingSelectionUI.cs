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

    [Header("Glow Settings")]
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private Vector2 glowDistance = new Vector2(2f, 2f);

    private Outline[] outlines;

    private int currentSelectedBuilding = -1;

    private void Awake()
    {
        outlines = new Outline[buildingImages.Length];

        for (int i = 0; i < buildingImages.Length; i++)
        {
            if (buildingImages[i] == null)
                continue;

            Outline outline = buildingImages[i].GetComponent<Outline>();

            if (outline == null)
            {
                outline = buildingImages[i].gameObject.AddComponent<Outline>();
            }

            outline.effectColor = glowColor;
            outline.effectDistance = glowDistance;
            outline.enabled = false;

            outlines[i] = outline;
        }

        ResetAllBuildings();
    }

    public void SetSelectedBuilding(int buildingIndex)
    {
        if (buildingObjects == null || buildingObjects.Length == 0)
            return;

        if (buildingIndex < 0 || buildingIndex >= buildingObjects.Length)
            return;

        currentSelectedBuilding = buildingIndex;

        for (int i = 0; i < buildingObjects.Length; i++)
        {
            if (buildingObjects[i] == null)
                continue;

            bool selected = i == buildingIndex;

            buildingObjects[i].localScale =
                Vector3.one * (selected ? selectedScale : normalScale);

            if (outlines != null &&
                i < outlines.Length &&
                outlines[i] != null)
            {
                outlines[i].enabled = selected;
            }
        }
    }

    private void ResetAllBuildings()
    {
        for (int i = 0; i < buildingObjects.Length; i++)
        {
            if (buildingObjects[i] != null)
                buildingObjects[i].localScale = Vector3.one * normalScale;

            if (outlines != null &&
                i < outlines.Length &&
                outlines[i] != null)
            {
                outlines[i].enabled = false;
            }
        }
    }

    public int GetCurrentSelectedBuilding()
    {
        return currentSelectedBuilding;
    }
}