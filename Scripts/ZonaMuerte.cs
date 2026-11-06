using UnityEngine;

public class ZonaMuerte : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ControladorVidas.Instancia != null)
            {
                ControladorVidas.Instancia.QuitarVida(other.gameObject);
            }
        }
    }
}