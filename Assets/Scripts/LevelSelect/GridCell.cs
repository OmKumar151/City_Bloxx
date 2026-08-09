using UnityEngine;

public class GridCell : MonoBehaviour
{
    [Header("Grid Position")]
    [SerializeField] private int x;
    [SerializeField] private int y;

    [Header("Selection")]
    [SerializeField] private GameObject highlight;

    public int X => x;
    public int Y => y;

    private void Awake()
    {
        // Every cell starts unselected.
        SetHighlight(false);
    }

    public void SetHighlight(bool selected)
    {
        if (highlight != null)
        {
            highlight.SetActive(selected);
        }
    }
}