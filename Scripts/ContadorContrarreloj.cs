using UnityEngine;
using UnityEngine.UI;

public class ContadorContrarreloj : MonoBehaviour
{
    public static ContadorContrarreloj Instancia { get; private set; }

    [Header("Configuración del Tiempo")]
    [Tooltip("Minutos para el desafío (editable desde Unity)")]
    [SerializeField] private float minutosIniciales = 2f;

    [Header("Referencias UI")]
    [SerializeField] private Text textoTiempo;
    [SerializeField] private GameObject panelGanaste;
    [SerializeField] private GameObject panelPerdiste;

    [Header("Audio y Voces (Bloque E)")]
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip clipVoz9;
    [SerializeField] private AudioClip clipVoz10;
    [SerializeField] private AudioClip Reloj;

    private float tiempoTotal;
    private float tiempoRestante;
    private int coleccionablesActuales = 0;
    private const int COLECCIONABLES_REQUERIDOS = 3;
    private bool juegoTerminado = false;

    private bool voz50Reproducida = false;
    private bool voz20Reproducida = false;
    private bool RelojReproducido = false;
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
        tiempoTotal = minutosIniciales * 60f;
        tiempoRestante = tiempoTotal;

        if (panelGanaste != null) panelGanaste.SetActive(false);
        if (panelPerdiste != null) panelPerdiste.SetActive(false);
        ActualizarRelojUI();
    }

    private void Update()
    {
        if (juegoTerminado) return;

        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            ActualizarRelojUI();
            VerificarAlertasDeTiempo();
        }
        else
        {
            tiempoRestante = 0;
            ActualizarRelojUI();
            PerderPorTiempo();
        }
    }

    private void VerificarAlertasDeTiempo()
    {
        float porcentajeRestante = tiempoRestante / tiempoTotal;

        if (!voz50Reproducida && porcentajeRestante <= 0.5f)
        {
            voz50Reproducida = true;
            ReproducirVoz(clipVoz9);
        }
        if (!voz20Reproducida && porcentajeRestante <= 0.2f)
        {
            voz20Reproducida = true;
            ReproducirVoz(clipVoz10);
        }
        if (!RelojReproducido && porcentajeRestante <= 0.05f)
        {
            RelojReproducido = true;
            ReproducirVoz(Reloj);
        }
    }

    private void ReproducirVoz(AudioClip clip)
    {
        if (clip == null) return;

        if (fuenteAudio != null)
        {
            fuenteAudio.PlayOneShot(clip);
        }
        else if (ControladorVidas.Instancia != null)
        {
            AudioSource audioControlador = ControladorVidas.Instancia.GetComponent<AudioSource>();
            if (audioControlador != null)
            {
                audioControlador.PlayOneShot(clip);
            }
        }
    }

    public void RegistrarColeccionable()
    {
        if (juegoTerminado) return;

        coleccionablesActuales++;
        if (coleccionablesActuales >= COLECCIONABLES_REQUERIDOS)
        {
            GanarJuego();
        }
    }

    private void ActualizarRelojUI()
    {
        if (textoTiempo == null) return;
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    private void GanarJuego()
    {
        juegoTerminado = true;
        if (panelGanaste != null) panelGanaste.SetActive(true);
    }

    private void PerderPorTiempo()
    {
        juegoTerminado = true;
        if (panelPerdiste != null) panelPerdiste.SetActive(true);
    }
}