using UnityEngine;
public class PlayerCamera : MonoBehaviour
{
    #region Cam Values
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerBody;
    [SerializeField] private float lookSpeed = 8f;
    [SerializeField] private float lookXLimit = 85f;

    [Header("Efectos de Cámara")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float runFOV = 78f;
    [SerializeField] private float fovSpeed = 8f;
    [SerializeField] private float tiltSpeed = 8f;

    [Header("Colisiones de Cámara")]
    [SerializeField] private bool enableCameraCollision = true;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float collisionSmoothSpeed = 15f;
    [SerializeField] private float returnSmoothSpeed = 5f;
    [SerializeField] private float pivotHeight = 1.5f;
    [SerializeField] private float collisionOffset = 0.1f;
    [SerializeField] private LayerMask collisionLayers = ~0;

    private float rotationX;
    private float currentTilt;
    private float targetTilt;
    private float targetFOV;
    private bool canMove = true;

    private Transform cameraTransform;
    private Vector3 originalCameraLocalPosition;
    private float currentCameraDistance;
    private float desiredCameraDistance;

    #endregion
    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerBody == null)
            playerBody = transform;

        if (playerCamera == null)
            return;

        cameraTransform = playerCamera.transform;
        targetFOV = normalFOV;
        playerCamera.fieldOfView = normalFOV;

        originalCameraLocalPosition = cameraTransform.localPosition;
        currentCameraDistance = originalCameraLocalPosition.magnitude;
        desiredCameraDistance = currentCameraDistance;
    }
    private void Update()
    {
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView,targetFOV,Time.deltaTime * fovSpeed);
        }
    }
    private void LateUpdate()
    {
        if (canMove && playerCamera != null && playerBody != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, currentTilt);
            playerBody.Rotate(Vector3.up * mouseX);
        }
         HandleCameraCollision();
    }
    #region Collisions
    private void HandleCameraCollision()
    {
        if (!enableCameraCollision || playerCamera == null || playerBody == null || cameraTransform == null)
            return;

        Vector3 desiredPosition;

        if (cameraTransform.parent != null)
            desiredPosition = cameraTransform.parent.TransformPoint(originalCameraLocalPosition);
        else
            desiredPosition = playerBody.TransformPoint(originalCameraLocalPosition);

        Vector3 pivotPosition = playerBody.position + Vector3.up * pivotHeight;
        Vector3 direction = desiredPosition - pivotPosition;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return;

        direction /= distance;
        desiredCameraDistance = distance;

        if (Physics.SphereCast(pivotPosition,collisionRadius,direction,out RaycastHit hit,distance,collisionLayers,QueryTriggerInteraction.Ignore))
        {
            desiredCameraDistance = Mathf.Max(0f, hit.distance - collisionOffset);
        }

        float smoothSpeed = desiredCameraDistance < currentCameraDistance ? collisionSmoothSpeed : returnSmoothSpeed;

        currentCameraDistance = Mathf.Lerp(currentCameraDistance,desiredCameraDistance,Time.deltaTime * smoothSpeed);

        cameraTransform.position = pivotPosition + direction * currentCameraDistance;
    }
    #endregion
    public void SetTilt(float angle)
    {
        targetTilt = angle;
    }
    public void SetFOV(bool isRunning)
    {
        targetFOV = isRunning ? runFOV : normalFOV;
    }
}
