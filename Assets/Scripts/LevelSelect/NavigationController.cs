
using UnityEngine;

public class NavigationController : MonoBehaviour
{
    public LevelSelectManager manager;

    public void Left()
    {
        if (manager != null)
        {
            manager.PreviousLevel();
        }
        else
        {
            Debug.LogWarning(
                "NavigationController: LevelSelectManager " +
                "is not assigned.",
                this
            );
        }
    }

    public void Right()
    {
        if (manager != null)
        {
            manager.NextLevel();
        }
        else
        {
            Debug.LogWarning(
                "NavigationController: LevelSelectManager " +
                "is not assigned.",
                this
            );
        }
    }

    public void OK()
    {
        if (manager != null)
        {
            manager.PlaySelectedLevel();
        }
        else
        {
            Debug.LogWarning(
                "NavigationController: LevelSelectManager " +
                "is not assigned.",
                this
            );
        }
    }

    public void Back()
    {
        if (manager != null)
        {
            manager.BackToMenu();
        }
        else
        {
            Debug.LogWarning(
                "NavigationController: LevelSelectManager " +
                "is not assigned.",
                this
            );
        }
    }
}
