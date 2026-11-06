using UnityEngine;
using UnityEngine.SceneManagement;

public class doorCode : MonoBehaviour
{
    [SerializeField] private GameObject player;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered trigger");


        if (other.gameObject == player)
        {
            Debug.Log("Player entered trigger");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}