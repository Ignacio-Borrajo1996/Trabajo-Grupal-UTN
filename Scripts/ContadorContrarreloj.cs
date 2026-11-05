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

    private float tiempoRestante;
    private int coleccionablesActuales = 0;
    private const int COLECCIONABLES_REQUERIDOS = 3;
    private bool juegoTerminado = false;

    private void Awake()
    {
        Instancia = this; 
    }

    private void Start()
    {
        tiempoRestante = minutosIniciales * 60f;
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
        }
        else
        {
            tiempoRestante = 0;
            ActualizarRelojUI();
            PerderPorTiempo();
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