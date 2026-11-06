using UnityEngine;
public class VGM : MonoBehaviour
{
    //Values in a Gameplay Play
    public int Score = 0;
    public int ScoreOfKill = 20;
    public int ScoreOfWin = 5000;
    public float Lifes = 3;

    public static VGM Instance;

    private void Awake()
    {
        Instance = this;
    }
}