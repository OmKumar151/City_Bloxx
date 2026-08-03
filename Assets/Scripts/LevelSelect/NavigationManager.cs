using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;

    [Header("Navigation State")]
    public NavigationState CurrentState = NavigationState.BuildingSelection;

    [Header("Building Selection")]
    [Tooltip("0 = Blue, 1 = Red, 2 = Green, 3 = Yellow")]
    public int selectedBuilding = 0;

    [Header("Board Position")]
    public int boardX = 0;
    public int boardY = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Navigation Input

    public void Up()
    {
        switch (CurrentState)
        {
            case NavigationState.BuildingSelection:

                if (selectedBuilding > 0)
                    selectedBuilding--;

                Debug.Log("Selected Building: " + selectedBuilding);
                break;

            case NavigationState.BoardPlacement:

                boardY--;

                Debug.Log("Board Position: " + boardX + ", " + boardY);
                break;
        }
    }

    public void Down()
    {
        switch (CurrentState)
        {
            case NavigationState.BuildingSelection:

                if (selectedBuilding < 3)
                    selectedBuilding++;

                Debug.Log("Selected Building: " + selectedBuilding);
                break;

            case NavigationState.BoardPlacement:

                boardY++;

                Debug.Log("Board Position: " + boardX + ", " + boardY);
                break;
        }
    }

    public void Left()
    {
        if (CurrentState == NavigationState.BoardPlacement)
        {
            boardX--;

            Debug.Log("Board Position: " + boardX + ", " + boardY);
        }
    }

    public void Right()
    {
        if (CurrentState == NavigationState.BoardPlacement)
        {
            boardX++;

            Debug.Log("Board Position: " + boardX + ", " + boardY);
        }
    }

    public void OK()
    {
        switch (CurrentState)
        {
            case NavigationState.BuildingSelection:

                CurrentState = NavigationState.BoardPlacement;

                Debug.Log("Entered Board Placement Mode");
                break;

            case NavigationState.BoardPlacement:

                Debug.Log("Attempt Place Building");

                CurrentState = NavigationState.BuildingSelection;

                Debug.Log("Returned to Building Selection");
                break;
        }
    }

    public void CancelPlacement()
    {
        CurrentState = NavigationState.BuildingSelection;

        Debug.Log("Placement Cancelled");
    }

    #endregion
}