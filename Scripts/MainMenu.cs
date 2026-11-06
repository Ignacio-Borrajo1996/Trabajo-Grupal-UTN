using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menuinicial : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameplaySceneName = "Nivel1";

    [Header("Audio Settings")]
    public AudioSource clickAudioSource;

    [Header("UI Panels (Optional)")]
    public GameObject mainMenuPanel;
    public GameObject levelsPanel;
    public GameObject controlsPanel;

    public void PlayGame()
    {
        if (clickAudioSource != null)
        {
            clickAudioSource.Play();
             
            Invoke("LoadGameplayScene", 0.30f);
        }
        else
        {
            LoadGameplayScene();
        }
    }

    private void LoadGameplayScene()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ExitGame()
    {
        if (clickAudioSource != null)
        {
            clickAudioSource.Play();
            Invoke("QuitApplication", 0.15f);
        }
        else
        {
            QuitApplication();
        }
    }

    private void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Player exited the game.");
    }
}
