using UnityEngine;

public class spikeScript : MonoBehaviour
{
    [SerializeField] private spawnScript[] spawnPoints;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null && spawnManager.currentSpawn != null)
            {
                controller.enabled = false;

                other.transform.position = spawnManager.currentSpawn.position;

                controller.enabled = true;

                Debug.Log("Teleported to " + spawnManager.currentSpawn.name);
            }
        }
    }
    
}
