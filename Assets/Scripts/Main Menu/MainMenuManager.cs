using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // =========================================================
    // MAIN MENU BUTTONS
    // =========================================================

    public void OpenBuildCity()
    {
        PlayButtonSound();

        SceneManager.LoadScene("LevelSelect");
    }

    public void OpenQuickGame()
    {
        PlayButtonSound();

        SceneManager.LoadScene("Gameplay1");
    }

    public void OpenRanking()
    {
        PlayButtonSound();

        SceneManager.LoadScene("Ranking");
    }

    public void OpenShop()
    {
        PlayButtonSound();

        SceneManager.LoadScene("Shop");
    }

    public void QuitGame()
    {
        PlayButtonSound();

        Application.Quit();
    }

    // =========================================================
    // BUTTON SOUND
    // =========================================================

    private void PlayButtonSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "MainMenuManager: AudioManager is not available."
            );

            return;
        }

        AudioManager.Instance.PlayButtonClick();
    }
}