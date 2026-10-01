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
                "MainMenuManager: AudioManager is not available.");
        }
    }

    public void OpenBuildCity()
    {
        PlayWindowSwitchSound();

        SceneManager.LoadScene("LevelSelect");
    }

    public void OpenQuickGame()
    {
        PlayWindowSwitchSound();

        SceneManager.LoadScene("Gameplay1");
    }

    public void OpenRanking()
    {
        PlayWindowSwitchSound();

        SceneManager.LoadScene("Ranking");
    }

    public void OpenShop()
    {
        PlayWindowSwitchSound();

        SceneManager.LoadScene("Shop");
    }

    public void QuitGame()
    {
        PlayButtonSound();

        Application.Quit();
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayButtonSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "MainMenuManager: AudioManager is not available.");
            return;
        }

        AudioManager.Instance.PlayButtonClick();
    }

    private void PlayWindowSwitchSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "MainMenuManager: AudioManager is not available.");
            return;
        }

        AudioManager.Instance.PlayWindowSwitch();
    }
}