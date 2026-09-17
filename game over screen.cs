using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOverScreen : MonoBehaviour

{
    public void PlayAgain()
    {
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.name);
    }
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ReturnToMainMenu()
    {
     SceneManager.LoadScene("Main Menu");
    }
}
