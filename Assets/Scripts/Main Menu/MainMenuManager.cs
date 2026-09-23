using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuMusic();
        }
        else
        {
            Debug.LogWarning(
                "MainMenuManager: AudioManager is not available."
            );
        }
    }

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

        // This uses SFX only.
        AudioManager.Instance.PlayButtonClick();
    }
}