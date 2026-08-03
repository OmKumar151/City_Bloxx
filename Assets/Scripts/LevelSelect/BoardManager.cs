using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public GridCell[,] Grid = new GridCell[5, 5];

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        RegisterCells();
    }

    void RegisterCells()
    {
        GridCell[] cells = FindObjectsByType<GridCell>(FindObjectsSortMode.None);

        foreach (GridCell cell in cells)
        {
            Grid[cell.x, cell.y] = cell;
        }
    }

    public GridCell GetCell(int x, int y)
    {
        if (x < 0 || x >= 5 || y < 0 || y >= 5)
            return null;

        return Grid[x, y];
    }
}