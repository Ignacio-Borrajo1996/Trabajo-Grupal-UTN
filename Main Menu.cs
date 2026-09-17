using UnityEngine;
using UnityEngine.SceneManagement;
public class Menuinicial : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject Levels;
    [SerializeField] private GameObject Controls;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void controls()
    {
        MainMenu.SetActive(false);
        Controls.SetActive(true);
    }
    public void levels()
    {
        MainMenu.SetActive(false);
        Levels.SetActive(true);
    }

    public void ExitToControls()
    {
        MainMenu.SetActive(true);
        Controls.SetActive(false);
    }
    public void Level1()
    {
        SceneManager.LoadScene("Level1");
    }
    public void Level2()
    {
        SceneManager.LoadScene("Level 2");
    }
    public void Leave()
    {
        Debug.Log("leave... ");
        Application.Quit();
    }
}
