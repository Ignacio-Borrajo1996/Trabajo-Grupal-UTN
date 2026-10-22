using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Ventilador : MonoBehaviour
{
    [Header("Parámetros del Viento")]
    [Tooltip("Fuerza del empuje aplicado al personaje")]
    [SerializeField] private float fuerza = 20f;

    [Tooltip("Alcance del aire en metros (longitud de la zona invisible)")]
    [SerializeField] private float alcance = 5f;

    [Header("Configuración Intermitente")]
    [Tooltip("Si está activo, el ventilador alternará entre encendido y apagado")]
    [SerializeField] private bool esIntermitente = false;

    [Tooltip("Tiempo encendido (en SEGUNDOS)")]
    [SerializeField] private float tiempoEncendido = 3f;

    [Tooltip("Tiempo apagado (en SEGUNDOS)")]
    [SerializeField] private float tiempoApagado = 5f;

    [Header("Efectos Visuales")]
    [SerializeField] private ParticleSystem efectoParticulasViento;

    private BoxCollider colliderCuerpoSolido;
    private BoxCollider areaVientoTrigger;
    private bool estaActivo = true;
    private Coroutine corrutinaIntermitente;

    private void Awake()
    {
        ConfigurarEstructuraVentilador();
    }

    private void Start()
    {
        ActualizarAlcance();
        GestionarEstadoIntermitente();
    }

    private void Update()
    {
        GestionarEstadoIntermitente();
    }

    private void OnValidate()
    {
        AjustarColliderBase();

        Transform hijoArea = transform.Find("ZonaDeAire");
        if (hijoArea != null)
        {
            areaVientoTrigger = hijoArea.GetComponent<BoxCollider>();
            ActualizarAlcance();
        }
    }

    private void AjustarColliderBase()
    {
        if (colliderCuerpoSolido == null) colliderCuerpoSolido = GetComponent<BoxCollider>();
        if (colliderCuerpoSolido != null)
        {
            colliderCuerpoSolido.isTrigger = false;
            colliderCuerpoSolido.size = Vector3.one;
            colliderCuerpoSolido.center = Vector3.zero;
        }
    }

    private void ConfigurarEstructuraVentilador()
    {
        AjustarColliderBase();

        Transform hijoArea = transform.Find("ZonaDeAire");
        if (hijoArea == null)
        {
            GameObject objHijo = new GameObject("ZonaDeAire");
            objHijo.transform.SetParent(transform);
            objHijo.transform.localPosition = Vector3.zero;
            objHijo.transform.localRotation = Quaternion.identity;
            objHijo.transform.localScale = Vector3.one;
            hijoArea = objHijo.transform;
        }

        areaVientoTrigger = hijoArea.GetComponent<BoxCollider>();
        if (areaVientoTrigger == null)
        {
            areaVientoTrigger = hijoArea.gameObject.AddComponent<BoxCollider>();
        }

        areaVientoTrigger.isTrigger = true;

        ZonaVientoDetector detector = hijoArea.GetComponent<ZonaVientoDetector>();
        if (detector == null)
        {
            detector = hijoArea.gameObject.AddComponent<ZonaVientoDetector>();
        }
        detector.Inicializar(this);
    }

    private void GestionarEstadoIntermitente()
    {
        if (esIntermitente && corrutinaIntermitente == null)
        {
            corrutinaIntermitente = StartCoroutine(RutinaIntermitente());
        }
        else if (!esIntermitente && corrutinaIntermitente != null)
        {
            StopCoroutine(corrutinaIntermitente);
            corrutinaIntermitente = null;
            CambiarEstado(true);
        }
    }

    private void ActualizarAlcance()
    {
        if (areaVientoTrigger != null)
        {
            Vector3 tamano = Vector3.one;
            tamano.z = alcance;
            areaVientoTrigger.size = tamano;

            Vector3 centro = Vector3.zero;
            centro.z = alcance / 2f;
            areaVientoTrigger.center = centro;
        }
    }

    private IEnumerator RutinaIntermitente()
    {
        while (esIntermitente)
        {
            CambiarEstado(true);
            yield return new WaitForSeconds(tiempoEncendido);

            CambiarEstado(false);
            yield return new WaitForSeconds(tiempoApagado);
        }
    }

    private void CambiarEstado(bool activo)
    {
        estaActivo = activo;
        if (areaVientoTrigger != null)
        {
            areaVientoTrigger.enabled = activo;
        }

        if (efectoParticulasViento != null)
        {
            if (activo) efectoParticulasViento.Play();
            else efectoParticulasViento.Stop();
        }
    }

    public void ProcesarEmpuje(Collider other)
    {
        if (!estaActivo) return;

        Vector3 direccionAire = transform.forward;

        PlayerMc jugador = other.GetComponent<PlayerMc>();
        if (jugador != null)
        {
            jugador.AplicarEmpuje(direccionAire * fuerza);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = estaActivo ? Color.cyan : Color.gray;
        Gizmos.DrawRay(transform.position, transform.forward * alcance);
    }
}

public class ZonaVientoDetector : MonoBehaviour
{
    private Ventilador ventiladorPadre;

    public void Inicializar(Ventilador padre)
    {
        ventiladorPadre = padre;
    }

    private void OnTriggerStay(Collider other)
    {
        if (ventiladorPadre != null)
        {
            ventiladorPadre.ProcesarEmpuje(other);
        }
    }
}
