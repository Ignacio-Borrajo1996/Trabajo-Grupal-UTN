using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WindZoneDetect : MonoBehaviour
{
    private plataforms windSource;
    private BoxCollider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
    }

    public void Initialize(plataforms source)
    {
        windSource = source;

        if (triggerCollider == null)
            triggerCollider = GetComponent<BoxCollider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterWind(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Actualiza la fuerza si el jugador sigue dentro.
        RegisterWind(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (windSource == null || other == null)
            return;

        windSource.ExitWind(other);
    }

    private void RegisterWind(Collider other)
    {
        if (windSource == null || other == null)
            return;

        windSource.ProcessWind(other);
    }

    private void OnDisable()
    {
        // Evita mantener referencias si la zona se desactiva.
        if (windSource == null)
            return;

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null && col != triggerCollider)
                windSource.ExitWind(col);
        }
    }
}