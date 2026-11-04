using UnityEngine;

public class Coleccionable : MonoBehaviour
{
    [Header("Efectos Visuales")]
    [SerializeField] private bool rotar = true;
    [SerializeField] private float velocidadRotacion = 90f;

    [Header("Punto de Reaparici�n")]
    [SerializeField] private Transform puntoDeInicio;

    private void Update()
    {
        if (rotar)
        {
            transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ContadorContrarreloj.Instancia != null)
            {
                ContadorContrarreloj.Instancia.RegistrarColeccionable();
            }

            TeletransportarJugador(other.gameObject);

            Destroy(gameObject);
        }
    }

    private void TeletransportarJugador(GameObject jugador)
    {
        Transform destino = puntoDeInicio;

        if (destino == null)
        {
            GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");
            if (spawnObj != null)
            {
                destino = spawnObj.transform;
            }
        }

        if (destino != null)
        {
            CharacterController controller = jugador.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                jugador.transform.position = destino.position;
                jugador.transform.rotation = destino.rotation;
                controller.enabled = true;
            }
            else
            {
                jugador.transform.position = destino.position;
                jugador.transform.rotation = destino.rotation;
            }

            Rigidbody rb = jugador.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}