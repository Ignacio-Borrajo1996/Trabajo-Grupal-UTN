using UnityEngine;

public class spawnScript : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            spawnManager.currentSpawn = transform;
            Debug.Log("New spawn point: " + gameObject.name);
        }

    }
}
