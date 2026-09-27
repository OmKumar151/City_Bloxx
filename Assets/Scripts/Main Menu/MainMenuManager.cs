using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OpenBuildCity()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void OpenQuickGame()
    {
        SceneManager.LoadScene("Gameplay1");
    }

    public void OpenRanking()
    {
        SceneManager.LoadScene("Ranking");
    }

    public void OpenShop()
    {
        SceneManager.LoadScene("Shop");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}