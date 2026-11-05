using UnityEngine;

public class RotarColeccionable : MonoBehaviour
{
    [SerializeField] private Vector3 direccionRotacion = new Vector3(0, 1, 0); // Rota en el eje Y por defecto
    [SerializeField] private float velocidadRotacion = 100f;

    void Update()
    {
        // Multiplicar por Time.deltaTime asegura que rote fluido sin importar los FPS
        transform.Rotate(direccionRotacion * velocidadRotacion * Time.deltaTime);
    }
}
