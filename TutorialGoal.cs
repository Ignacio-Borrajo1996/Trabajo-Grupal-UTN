using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialGoal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Level 1");
        }
    }
}
