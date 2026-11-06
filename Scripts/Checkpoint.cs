using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ControladorVidas.Instancia != null)
            {
                ControladorVidas.Instancia.ActivarCheckpoint(transform);
            }
        }
    }
}