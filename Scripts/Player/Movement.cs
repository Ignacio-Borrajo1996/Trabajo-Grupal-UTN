using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerCheck))]
public class Movement : MonoBehaviour
{
    #region MovementValues

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private float gravity = 15f;
    [SerializeField] private float airDeceleration = 6f;

    [Tooltip("Velocidad máxima de caída.")]
    [SerializeField, Min(1f)] private float maxFallSpeed = 25f;

    private Vector3 moveDirection = Vector3.zero;

    [Header("External Forces")]
    [Tooltip("Velocidad máxima de las fuerzas externas.")]
    [SerializeField, Min(0f)] private float maxExternalVelocity = 8f;

    [Tooltip("Velocidad de respuesta de las fuerzas externas.")]
    [SerializeField, Min(0f)] private float windAcceleration = 12f;

    [Tooltip("Velocidad de pérdida del impulso al salir del viento.")]
    [SerializeField, Min(0f)] private float windDeceleration = 3f;

    private readonly Dictionary<plataforms, Vector3> windSources =
        new Dictionary<plataforms, Vector3>();

    private Vector3 combinedWindForce;
    private Vector3 externalVelocity;

    [Header("Momentum")]
    [SerializeField] private float maxMomentumSpeed = 20f;

    [Tooltip("Aceleración al correr desde la velocidad de caminar.")]
    [SerializeField, Min(0f)] private float runAcceleration = 25f;

    [Tooltip("Se conserva por compatibilidad con la configuración anterior; no se usa para frenar en el suelo.")]
    [SerializeField, Min(0f)] private float deceleration = 20f;

    private Vector3 horizontalVelocity = Vector3.zero;

    public float CurrentSpeed => horizontalVelocity.magnitude;
    public float MaxSpeed => maxMomentumSpeed;

    [Header("Jumping")]
    [SerializeField] private int jumpsMax = 2;
    public int jumpsLeft;

    [Header("Juice / Game Feel")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [SerializeField] private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    [Header("Wall Run")]
    [SerializeField] private float wallGravity = 2f;
    [SerializeField] private float wallStickForce = 3f;

    private bool isWallRunning;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideSpeed = 1.5f;
    [SerializeField] private float wallJumpUpForce = 10f;
    [SerializeField] private float wallJumpOutForce = 8f;

    private bool isWallSliding;
    private Collider lastWallJumped;

    [Header("Ledge")]
    [SerializeField] private float ledgeBoostPower = 12f;

    private bool isHanging;
    private bool justGrabbedLedge;
    private Vector3 ledgeEntryVelocity;

    public bool canMove = true;
    public bool StopInteractions = false;

    #endregion

    #region References

    [Header("References")]
    [SerializeField] private PlayerCamera playerCameraScript;

    private PlayerCheck playerCheck;
    private CharacterController characterController;

    public static Movement Instance;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCheck = GetComponent<PlayerCheck>();

        jumpsLeft = jumpsMax;

        if (playerCameraScript == null)
            playerCameraScript = GetComponentInChildren<PlayerCamera>();
    }

    private void Update()
    {
        combinedWindForce = Vector3.zero;

        List<plataforms> invalidSources = null;

        foreach (KeyValuePair<plataforms, Vector3> wind in windSources)
        {
            if (wind.Key == null)
            {
                if (invalidSources == null)
                    invalidSources = new List<plataforms>();

                invalidSources.Add(wind.Key);
                continue;
            }

            combinedWindForce += wind.Value;
        }

        if (invalidSources != null)
        {
            foreach (plataforms source in invalidSources)
                windSources.Remove(source);
        }

        combinedWindForce = Vector3.ClampMagnitude(combinedWindForce,maxExternalVelocity);

        bool hasActiveWind = windSources.Count > 0;

        if (hasActiveWind)
        {
            externalVelocity = Vector3.MoveTowards(externalVelocity,combinedWindForce,windAcceleration * Time.deltaTime);
        }
        else
        {
            externalVelocity = Vector3.MoveTowards(externalVelocity,Vector3.zero,windDeceleration * Time.deltaTime);
        }

        externalVelocity = Vector3.ClampMagnitude(externalVelocity,maxExternalVelocity);

        bool windIsLifting = hasActiveWind && combinedWindForce.y > 0.01f && externalVelocity.y > 0.01f;

        bool isGrounded =
            characterController.isGrounded && !windIsLifting;

        justGrabbedLedge = false;

        #region Floor Edge

        Vector3 edgeDirection = horizontalVelocity.sqrMagnitude > 0.01f ? horizontalVelocity.normalized : Vector3.zero;

        playerCheck.CheckForFloorEdge(isGrounded, edgeDirection);

        #endregion

        #region IsGrounded

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpsLeft = jumpsMax;
            lastWallJumped = null;
            isHanging = false;

            if (moveDirection.y < 0f)
                moveDirection.y = -2f;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            playerCheck.CheckForWalls();

            if (!isHanging && !isWallRunning && !isWallSliding && moveDirection.y <= 2f)
            {
                playerCheck.CheckForLedge();
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (!isGrounded && !isHanging && playerCheck.CanGrabLedge && !windIsLifting)
        {
            ledgeEntryVelocity = horizontalVelocity;
            isHanging = true;
            justGrabbedLedge = true;

            moveDirection = Vector3.zero;
            horizontalVelocity = Vector3.zero;
            jumpBufferCounter = 0f;
        }

        #endregion

        #region Wall Run

        Collider currentWall = playerCheck.IsWallRight ? playerCheck.HitRight.collider : playerCheck.HitLeft.collider;

        bool canWallRun = !isGrounded && !isHanging && (playerCheck.IsWallLeft || playerCheck.IsWallRight) && Input.GetAxisRaw("Vertical") > 0 && moveDirection.y <= 0f && currentWall != null && currentWall != lastWallJumped;

        if (Input.GetKeyDown(KeyCode.Space) && canWallRun && !isWallRunning)
        {
            isWallRunning = true;
            jumpBufferCounter = 0f;
        }

        isWallSliding = !isGrounded && !isHanging && !isWallRunning && moveDirection.y <= 0f && (playerCheck.IsWallLeft || playerCheck.IsWallRight);

        RaycastHit activeJumpHit = default;

        if (Input.GetKey(KeyCode.Space))
        {
            if (playerCheck.IsWallJumpFront)
                activeJumpHit = playerCheck.HitJumpFront;
            else if (playerCheck.IsWallJumpRight)
                activeJumpHit = playerCheck.HitJumpRight;
            else if (playerCheck.IsWallJumpLeft)
                activeJumpHit = playerCheck.HitJumpLeft;
        }

        bool isAnyWallJumpDetected =
            activeJumpHit.collider != null;

        #endregion

        #region Movement Input

        Vector3 forward =
            transform.TransformDirection(Vector3.forward);

        Vector3 right =
            transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float movementDirectionY = moveDirection.y;

        #endregion

        #region Hanging

        if (isHanging)
        {
            moveDirection = Vector3.zero;
            horizontalVelocity = Vector3.zero;
            externalVelocity = Vector3.zero;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(0f);
                playerCameraScript.SetFOV(false);
            }
        }

        #endregion

        #region Wall Running

        else if (isWallRunning)
        {
            Vector3 wallNormal = playerCheck.IsWallRight ? playerCheck.HitRight.normal : playerCheck.HitLeft.normal;

            Vector3 wallForward = Vector3.ProjectOnPlane(transform.forward,wallNormal);

            if (wallForward.sqrMagnitude < 0.01f)
            {
                wallForward = Vector3.ProjectOnPlane(horizontalVelocity,wallNormal);
            }

            if (wallForward.sqrMagnitude < 0.01f)
                wallForward = Vector3.Cross(wallNormal, Vector3.up);

            wallForward.Normalize();

            if (Vector3.Dot(wallForward, transform.forward) < 0f)
                wallForward = -wallForward;

            float currentSpeed = horizontalVelocity.magnitude;

            if (isRunning)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed,maxMomentumSpeed,runAcceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.Max(currentSpeed, walkSpeed);
            }

            horizontalVelocity = wallForward * currentSpeed;

            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;
            moveDirection.y = -wallGravity;

            moveDirection -= wallNormal * wallStickForce;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(
                    playerCheck.IsWallRight ? -12f : 12f
                );

                playerCameraScript.SetFOV(true);
            }
        }

        #endregion

        #region Wall Sliding

        else if (isWallSliding)
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity,Vector3.zero,airDeceleration * Time.deltaTime);

            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;
            moveDirection.y = -wallSlideSpeed;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(0f);
                playerCameraScript.SetFOV(false);
            }
        }

        #endregion

        #region Ground Movement

        else if (isGrounded)
        {
            Vector3 inputDirection = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");

            if (!canMove)
                inputDirection = Vector3.zero;

            bool hasInput = inputDirection.sqrMagnitude > 0.01f;

            if (hasInput)
            {
                inputDirection.Normalize();

                if (isRunning)
                {
                    float currentSpeed = Mathf.Max(horizontalVelocity.magnitude,walkSpeed);

                    currentSpeed = Mathf.MoveTowards(currentSpeed,maxMomentumSpeed,runAcceleration * Time.deltaTime);

                    horizontalVelocity = inputDirection * currentSpeed;
                }
                else
                {
                    horizontalVelocity = inputDirection * walkSpeed;
                }
            }
            else
            {
                horizontalVelocity = Vector3.zero;
            }

            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;
            moveDirection.y = movementDirectionY;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(0f);

                bool isMovingForward =
     Input.GetAxisRaw("Vertical") > 0;

                playerCameraScript.SetFOV(isRunning && isMovingForward);
            }
        }

        #endregion

        #region Air Movement

        else
        {
            Vector3 inputDirection =
                forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");

            if (!canMove)
                inputDirection = Vector3.zero;

            bool hasAirInput =
                inputDirection.sqrMagnitude > 0.01f;

            Vector3 currentHorizontal = new Vector3(moveDirection.x,0f,moveDirection.z);

            Vector3 newHorizontal;

            if (hasAirInput)
            {
                inputDirection.Normalize();

                float targetSpeed =
                    isRunning ? maxMomentumSpeed : walkSpeed;

                newHorizontal = inputDirection * targetSpeed;
            }
            else
            {

                newHorizontal = Vector3.MoveTowards(currentHorizontal,Vector3.zero,airDeceleration * Time.deltaTime );
            }

            if (newHorizontal.magnitude > maxMomentumSpeed)
            {
                newHorizontal =
                    newHorizontal.normalized * maxMomentumSpeed;
            }

            moveDirection.x = newHorizontal.x;
            moveDirection.z = newHorizontal.z;
            moveDirection.y = movementDirectionY;

            horizontalVelocity = newHorizontal;

            if (playerCameraScript != null)
                playerCameraScript.SetTilt(0f);
        }

        #endregion

        #region Jumping

        if (isHanging && !justGrabbedLedge && Input.GetKeyDown(KeyCode.Space))
        {
            isHanging = false;

            playerCheck.ResetLedgeGrab();

            moveDirection.y = ledgeBoostPower;

            Vector3 inheritedVelocity = Vector3.ProjectOnPlane(ledgeEntryVelocity,Vector3.up) * 0.65f;

            horizontalVelocity = Vector3.ClampMagnitude(inheritedVelocity + transform.forward * 3f,maxMomentumSpeed);

            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;

            ledgeEntryVelocity = Vector3.zero;
            jumpBufferCounter = 0f;
        }
        else if (!isHanging && jumpBufferCounter > 0f && canMove)
        {
            if (isGrounded && playerCheck.CanEdgeBoost)
            {
                moveDirection.y = ledgeBoostPower;

                jumpsLeft = Mathf.Max(0, jumpsLeft - 1);

                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
            else if (isWallRunning)
            {
                Vector3 wallNormal = playerCheck.IsWallRight ? playerCheck.HitRight.normal : playerCheck.HitLeft.normal;

                Vector3 inheritedVelocity = Vector3.ProjectOnPlane(horizontalVelocity,wallNormal);

                horizontalVelocity = Vector3.ClampMagnitude(inheritedVelocity + wallNormal * wallJumpOutForce,maxMomentumSpeed);

                moveDirection.x = horizontalVelocity.x;
                moveDirection.z = horizontalVelocity.z;
                moveDirection.y = wallJumpUpForce;

                jumpsLeft = Mathf.Max(0, jumpsLeft - 1);

                lastWallJumped = currentWall;
                isWallRunning = false;

                jumpBufferCounter = 0f;
            }
            else if (isWallSliding && isAnyWallJumpDetected)
            {
                Vector3 wallNormal = activeJumpHit.normal;

                ApplyWallJumpVelocity(wallNormal);

                moveDirection.y = wallJumpUpForce;

                lastWallJumped = activeJumpHit.collider;
                isWallSliding = false;

                jumpBufferCounter = 0f;
            }
            else if (coyoteTimeCounter > 0f || jumpsLeft > 0)
            {
                moveDirection.y = jumpPower;

                jumpsLeft--;

                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
        }
        else if (!isHanging && !isWallRunning && !isWallSliding)
        {
            moveDirection.y = movementDirectionY;
        }

        #endregion

        #region Gravity

        bool activeUpdraft = windSources.Count > 0 && combinedWindForce.y > 0.01f && externalVelocity.y > 0.01f;

        if (activeUpdraft)
        {
            moveDirection.y = Mathf.Max(0f, moveDirection.y);
        }
        else if (!isGrounded && !isHanging && !isWallRunning && !isWallSliding)
        {
            moveDirection.y -= gravity * Time.deltaTime;

            moveDirection.y = Mathf.Max(moveDirection.y,-maxFallSpeed);
        }

        moveDirection.y = Mathf.Clamp(moveDirection.y,-maxFallSpeed,Mathf.Max(jumpPower, ledgeBoostPower, wallJumpUpForce));

        #endregion

        #region Platform Movement

        Vector3 platformDelta = playerCheck != null ? playerCheck.GetPlatformDelta(isGrounded) : Vector3.zero;

        #endregion

        #region Apply Movement

        characterController.Move((moveDirection + externalVelocity) * Time.deltaTime + platformDelta);

        #endregion
    }

    #region Wall Jump

    private void ApplyWallJumpVelocity(Vector3 wallNormal)
    {
        Vector3 inheritedVelocity = Vector3.ProjectOnPlane(horizontalVelocity,wallNormal);

        horizontalVelocity = Vector3.ClampMagnitude(inheritedVelocity + wallNormal * wallJumpOutForce,maxMomentumSpeed);

        moveDirection.x = horizontalVelocity.x;
        moveDirection.z = horizontalVelocity.z;
    }

    #endregion

    #region Wind

    public void ApplyWind(plataforms source, Vector3 acceleration)
    {
        if (source == null)
            return;

        windSources[source] = acceleration;
    }

    public void StopWind(plataforms source)
    {
        if (source == null)
            return;

        windSources.Remove(source);
    }

    #endregion
}