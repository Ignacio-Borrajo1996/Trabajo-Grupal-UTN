using UnityEngine;

public class MovingObstacle : MovingObject
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();
            
            if (controller != null)
            {
                controller.enabled = false;
            }

            if (spawnManager.currentSpawn != null)
            {
                other.transform.position = spawnManager.currentSpawn.position;
            }

            if (controller != null)
            {
                controller.enabled = true;
            }
        }
    }
}
