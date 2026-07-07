using UnityEngine;

public class NavigationController : MonoBehaviour
{
    public LevelSelectManager manager;

    public void Left()
    {
        manager.PreviousLevel();
    }

    public void Right()
    {
        manager.NextLevel();
    }

    public void OK()
    {
        manager.PlaySelectedLevel();
    }

    public void Back()
    {
        manager.BackToMenu();
    }
}