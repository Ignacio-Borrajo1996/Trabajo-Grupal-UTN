using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public bool IsWin = false;
    public bool GameOver = false;

    [SerializeField] public GameObject gameOverScreen;
    [SerializeField] public GameObject winScreen;

    public TextMeshProUGUI ScoreCounter;
    
    private void Start()
    {
        gameOverScreen.SetActive(false);
        winScreen.SetActive(false);
    }

    void Update()
    {
        if (GameOver)
        {
            Lose();
        }
    }

    public void Win()
    {
        if (IsWin) return;
        
        IsWin = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        winScreen.SetActive(true);

        if (PlayerMc.Instance != null)
        {
            PlayerMc.Instance.canMove = false;
            PlayerMc.Instance.StopInteractions = true;
        }
    }

    public void Lose()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameOverScreen.SetActive(true);

        if (PlayerMc.Instance != null)
        {
            PlayerMc.Instance.canMove = false;
            PlayerMc.Instance.StopInteractions = true;
        }
    }

    public void PlayAgain()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}
