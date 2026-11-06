using UnityEngine;
using UnityEngine.UI;

public class ControladorVidas : MonoBehaviour
{
    public static ControladorVidas Instancia { get; private set; }

    [Header("Configuración de Vidas")]
    [SerializeField] private int vidasIniciales = 3;

    [Header("Punto Inicial del Nivel")]
    [SerializeField] private Transform spawnPointInicial;

    [Header("Interfaz UI (Opcional)")]
    [SerializeField] private Text textoVidas;
    [SerializeField] private GameObject panelGameOver;

    [Header("Audio y Voces (Bloque E)")]
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip clipTeQuedan2Vidas;
    [SerializeField] private AudioClip clipTeQueda1Vida;
    [SerializeField] private AudioClip clipGameOver;
    [SerializeField] private AudioClip clipPerdisteEn10Seg;

    [Header("Efectos de Sonido (SFX)")]
    [SerializeField] private AudioClip clipSonidoGameOver;

    private int vidasActuales;
    private Transform ultimoCheckpoint;

    private float tiempoPrimeraMuerte = -1f;
    private bool contandoTiempo = false;

    private void Awake()
    {
        Instancia = this;
        if (fuenteAudio == null)
        {
            fuenteAudio = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        vidasActuales = vidasIniciales;
        ActualizarUI();

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }
    }

    public void QuitarVida(GameObject jugador)
    {
        if (!contandoTiempo && vidasActuales == vidasIniciales)
        {
            tiempoPrimeraMuerte = Time.time;
            contandoTiempo = true;
        }

        vidasActuales--;
        ActualizarUI();

        if (vidasActuales <= 0)
        {
            float tiempoTranscurrido = Time.time - tiempoPrimeraMuerte;

            ReproducirVozGameOver(tiempoTranscurrido);

            if (fuenteAudio != null && clipSonidoGameOver != null)
            {
                fuenteAudio.PlayOneShot(clipSonidoGameOver);
            }

            vidasActuales = vidasIniciales;
            ultimoCheckpoint = null;
            contandoTiempo = false;

            if (panelGameOver != null)
            {
                panelGameOver.SetActive(true);
            }
            Transform destinoFinal = spawnPointInicial != null ? spawnPointInicial : transform;
            TeletransportarJugador(jugador, destinoFinal);
            ActualizarUI();
        }
        else
        {
            ReproducirVozSegunVidas();

            Transform destinoCheckpoint = ultimoCheckpoint != null ? ultimoCheckpoint : spawnPointInicial;

            if (destinoCheckpoint != null)
            {
                TeletransportarJugador(jugador, destinoCheckpoint);
            }
        }
    }

    private void ReproducirVozSegunVidas()
    {
        if (fuenteAudio == null) return;

        if (vidasActuales == 2 && clipTeQuedan2Vidas != null)
        {
            fuenteAudio.PlayOneShot(clipTeQuedan2Vidas);
        }
        else if (vidasActuales == 1 && clipTeQueda1Vida != null)
        {
            fuenteAudio.PlayOneShot(clipTeQueda1Vida);
        }
    }

    private void ReproducirVozGameOver(float tiempoTranscurrido)
    {
        if (fuenteAudio == null) return;

        if (contandoTiempo && tiempoTranscurrido <= 10f && clipPerdisteEn10Seg != null)
        {
            fuenteAudio.PlayOneShot(clipPerdisteEn10Seg);
        }
        else if (clipGameOver != null)
        {
            fuenteAudio.PlayOneShot(clipGameOver);
        }
    }

    public void ActivarCheckpoint(Transform nuevoCheckpoint)
    {
        ultimoCheckpoint = nuevoCheckpoint;
    }

    private void ActualizarUI()
    {
        if (textoVidas != null)
        {
            textoVidas.text = "Vidas: " + vidasActuales;
        }
    }

    private void TeletransportarJugador(GameObject jugador, Transform destino)
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