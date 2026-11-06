using UnityEngine;
using UnityEngine.UI;

public class InertiaHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Movement playerMovement;
    [SerializeField] private Image speedBar;

    [Header("Settings")]
    [Tooltip("Velocidad máxima representada por la barra.")]
    [SerializeField, Min(0.1f)] private float displayMaxSpeed = 20f;

    [Tooltip("Suavidad de la animación de la barra.")]
    [SerializeField, Min(0f)] private float smoothSpeed = 8f;

    private float displayedSpeed;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<Movement>();
    }

    private void Update()
    {
        if (playerMovement == null || speedBar == null)
            return;

        float maxSpeed = Mathf.Max(0.1f, displayMaxSpeed);

        float targetFill = Mathf.Clamp01(playerMovement.CurrentSpeed / maxSpeed);

        displayedSpeed = Mathf.MoveTowards(displayedSpeed,targetFill,smoothSpeed * Time.deltaTime);

        speedBar.fillAmount = displayedSpeed;
    }

    private void OnValidate()
    {
        displayMaxSpeed = Mathf.Max(0.1f, displayMaxSpeed);
        smoothSpeed = Mathf.Max(0f, smoothSpeed);
    }
}