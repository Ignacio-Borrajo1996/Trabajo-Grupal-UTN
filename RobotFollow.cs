using UnityEngine;

public class RobotFollow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;

    [Header("Parámetros Reactivos")]
    [SerializeField] private float alturaVuelo = 3f;
    [SerializeField] private float distanciaDeseada = 5f;
    [SerializeField] private float tiempoSuavizado = 0.35f;
    [SerializeField] private float velocidadMirada = 6f;

    [Header("Detección de Paredes Sólidas")]
    [SerializeField] private LayerMask capasParedes = ~0;
    [SerializeField] private float radioRobot = 0.45f;

    private Vector3 velocidadActual;

    private void Update()
    {
        if (jugador == null) return;

        Vector3 direccionHorizontal = transform.position - jugador.position;
        direccionHorizontal.y = 0;

        if (direccionHorizontal.sqrMagnitude < 0.001f)
        {
            direccionHorizontal = -jugador.forward;
        }

        Vector3 origenJugador = jugador.position + Vector3.up * 1.5f;

        Vector3 posicionIdeal = jugador.position
                                + (direccionHorizontal.normalized * distanciaDeseada)
                                + (Vector3.up * alturaVuelo);

        Vector3 posicionFinal = posicionIdeal;

        Vector3 dirJugadorAObjetivo = posicionIdeal - origenJugador;
        float distJugadorAObjetivo = dirJugadorAObjetivo.magnitude;

        if (Physics.SphereCast(origenJugador, radioRobot, dirJugadorAObjetivo.normalized, out RaycastHit hitJugador, distJugadorAObjetivo, capasParedes, QueryTriggerInteraction.Ignore))
        {
            if (hitJugador.transform != jugador && hitJugador.transform != transform)
            {
                posicionFinal = hitJugador.point + (hitJugador.normal * radioRobot);
            }
        }

        Vector3 dirRobotAFinal = posicionFinal - transform.position;
        float distRobotAFinal = dirRobotAFinal.magnitude;

        if (distRobotAFinal > 0.02f)
        {
            if (Physics.SphereCast(transform.position, radioRobot, dirRobotAFinal.normalized, out RaycastHit hitRobot, distRobotAFinal, capasParedes, QueryTriggerInteraction.Ignore))
            {
                if (hitRobot.transform != jugador && hitRobot.transform != transform)
                {
                    posicionFinal = hitRobot.point + (hitRobot.normal * radioRobot);
                }
            }
        }
        transform.position = Vector3.SmoothDamp(
            transform.position,
            posicionFinal,
            ref velocidadActual,
            tiempoSuavizado
        );
        Vector3 direccionAlJugador = origenJugador - transform.position;
        if (direccionAlJugador != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlJugador);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionDeseada,
                velocidadMirada * Time.deltaTime
            );
        }
    }
}