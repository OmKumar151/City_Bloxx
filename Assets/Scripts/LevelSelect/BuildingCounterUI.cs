using TMPro;
using UnityEngine;

public class BuildingCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countText;

    [Header("Board")]
    [SerializeField] private BoardManager boardManager;

    private void Start()
    {
        FindBoardManager();
        UpdateCounter();
    }

    private void Update()
    {
        UpdateCounter();
    }

    private void FindBoardManager()
    {
        if (boardManager != null)
        {
            return;
        }

        boardManager =
            FindFirstObjectByType<BoardManager>();

        if (boardManager == null)
        {
            Debug.LogWarning(
                "BuildingCounterUI: " +
                "Could not find a BoardManager in the scene."
            );
        }
    }

    private void UpdateCounter()
    {
        if (countText == null)
        {
            return;
        }

        if (boardManager == null)
        {
            countText.text = "0/0";
            return;
        }

        int totalCells = 0;
        int occupiedCells = 0;

        foreach (GridCell cell in boardManager.GetAllActiveCells())
        {
            if (cell == null)
            {
                continue;
            }

            totalCells++;

            if (cell.IsOccupied)
            {
                occupiedCells++;
            }
        }

        countText.text =
            occupiedCells +
            "/" +
            totalCells;
    }
}