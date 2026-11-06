using TMPro;
using UnityEngine;
public class UIManager : MonoBehaviour
{
    public bool IsWin = false;
    public bool GameOver = false;

    [SerializeField] public GameObject HUD;
    [SerializeField] public GameObject gameOverScreen;
    [SerializeField] public GameObject winScreen;

    private void Start()
    {
        HUD.SetActive(true);
        gameOverScreen.SetActive(false);
        winScreen.SetActive(false);
    }

    void Update()
    {
        #region Condicionals

        //Game Over Condicional

        if (VGM.Instance != null && VGM.Instance.Lifes <= 0)
        {
         GameOver = true;
        }

        //Win condicional

        if (VGM.Instance != null &&
        VGM.Instance.Score >= VGM.Instance.ScoreOfWin)
        {
            IsWin = true;
        }

        #endregion

        #region WinScreen

        void Win()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            HUD.SetActive(false);
            winScreen.SetActive(true);

            Movement.Instance.canMove = false;
            Movement.Instance.StopInteractions = true;
        }

        if (IsWin)
        {
            Win();
        }

        #endregion

        #region GameOverScreen
        void Lose()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            HUD.SetActive(false);
            gameOverScreen.SetActive(true);

            Movement.Instance.canMove = false;
            Movement.Instance.StopInteractions = true;
        }

        if (GameOver)
        {
            Lose();
        }

        #endregion
    }

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}

