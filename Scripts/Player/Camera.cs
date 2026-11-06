using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region Cam Values
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerBody;

    [Header("Sensibilidad y Límites")]
    [SerializeField] private float lookSpeed = 8f;
    [SerializeField] private float lookXLimitMin = -40f;
    [SerializeField] private float lookXLimitMax = 70f;

    [Header("Rotación del Cuerpo")]
    [SerializeField] private bool rotatePlayerWithCamera = true;
    [SerializeField] private float bodyRotationSpeed = 8f;
    [SerializeField] private float angleThreshold = 60f;

    [Header("Posicionamiento 3ª Persona")]
    [SerializeField] private float pivotHeight = 1.6f;
    [SerializeField] private float defaultDistance = 3.5f;
    [SerializeField] private Vector2 shoulderOffset = new Vector2(0.5f, 0f);

    [Header("Efectos de Cámara")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float runFOV = 78f;
    [SerializeField] private float fovSpeed = 8f;
    [SerializeField] private float tiltSpeed = 8f;

    [Header("Colisiones de Cámara")]
    [SerializeField] private bool enableCameraCollision = true;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionSmoothSpeed = 25f;
    [SerializeField] private float returnSmoothSpeed = 8f;
    [SerializeField] private float collisionOffset = 0.1f;
    [SerializeField] private LayerMask collisionLayers;

    private float rotationX;
    private float rotationY;
    private float currentTilt;
    private float targetTilt;
    private float targetFOV;
    private float currentDistance;
    private float targetDistance;
    private bool isMoving;
    private bool canMove = true;
    private bool lockPlayerRotation = false;

    #endregion

    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    }

    public void SetLockPlayerRotation(bool lockRotation)
    {
        lockPlayerRotation = lockRotation;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerBody == null)
            playerBody = transform;

        if (playerCamera != null)
        {
            targetFOV = normalFOV;
            playerCamera.fieldOfView = normalFOV;
        }

        if (playerBody != null)
        {
            rotationY = playerBody.eulerAngles.y;
            rotationX = playerBody.eulerAngles.x;
        }

        currentDistance = defaultDistance;
        targetDistance = defaultDistance;
    }

    private void Update()
    {
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovSpeed);
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        isMoving = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputY) > 0.1f);
    }

    private void LateUpdate()
    {
        if (!canMove || playerCamera == null || playerBody == null)
            return;

        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, lookXLimitMin, lookXLimitMax);
        rotationY += mouseX;

        if (rotatePlayerWithCamera && !lockPlayerRotation)
        {
            Quaternion targetBodyRotation = Quaternion.Euler(0f, rotationY, 0f);
            float angleDifference = Quaternion.Angle(playerBody.rotation, targetBodyRotation);

            if (isMoving || angleDifference > angleThreshold)
            {
                float speedMultiplier = isMoving ? bodyRotationSpeed * 2.5f : bodyRotationSpeed;
                playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetBodyRotation, Time.deltaTime * speedMultiplier);
            }
        }

        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, currentTilt);
        Vector3 pivotPosition = playerBody.position + Vector3.up * pivotHeight;

        targetDistance = defaultDistance;

        if (enableCameraCollision)
        {
            Vector3 rayDirection = targetRotation * new Vector3(shoulderOffset.x, shoulderOffset.y, -defaultDistance);
            float rayLength = rayDirection.magnitude;

            if (Physics.SphereCast(pivotPosition, collisionRadius, rayDirection.normalized, out RaycastHit hit, rayLength, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != playerBody && !hit.transform.IsChildOf(playerBody))
                {
                    targetDistance = Mathf.Clamp(hit.distance - collisionOffset, 0.2f, defaultDistance);
                }
            }
        }

        float smoothSpeed = targetDistance < currentDistance ? collisionSmoothSpeed : returnSmoothSpeed;
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

        Vector3 finalOffset = new Vector3(shoulderOffset.x, shoulderOffset.y, -currentDistance);
        Vector3 finalPosition = pivotPosition + (targetRotation * finalOffset);

        playerCamera.transform.position = finalPosition;
        playerCamera.transform.rotation = targetRotation;
    }

    public void SetTilt(float angle)
    {
        targetTilt = angle;
    }

    public void SetFOV(bool isRunning)
    {
        targetFOV = isRunning ? runFOV : normalFOV;
    }
}