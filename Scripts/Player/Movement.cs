using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerCheck))]
public class Movement : MonoBehaviour
{
    #region MovementValues

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float jumpPower = 6f;
    [SerializeField] private float gravity = 18f;
    [SerializeField] private float airDeceleration = 6f;
    public Animator animator;


    [SerializeField, Min(1f)] private float maxFallSpeed = 25f;

    private Vector3 moveDirection = Vector3.zero;

    [Header("External Forces (Ventiladores / Viento)")]
    [SerializeField, Min(0f)] private float maxExternalVelocity = 20f;
    [SerializeField, Min(0f)] private float windAcceleration = 25f;
    [SerializeField, Min(0f)] private float windDeceleration = 15f;

    private readonly Dictionary<plataforms, Vector3> windSources = new Dictionary<plataforms, Vector3>();
    private Vector3 combinedWindForce;
    private Vector3 externalVelocity;

    [Header("Momentum")]
    [SerializeField] private float maxMomentumSpeed = 20f;
    [SerializeField, Min(0f)] private float runAcceleration = 25f;
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
    [SerializeField] private float wallStickForce = 4f;
    private bool isWallRunning;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpUpForce = 11f;
    [SerializeField] private float wallJumpOutForce = 9f;
    private bool isWallSliding;
    private Collider lastWallCollider;

    [Header("Ledge")]
    [SerializeField] private float ledgeBoostPower = 10f;
    [SerializeField] private float ledgeForwardBoost = 5f;
    private bool isHanging;
    private float ledgeGrabTimer;
    private float ledgeClimbTimer;
    private float ledgeGrabCooldown;

    private Transform hangingLedgeTransform;
    private Vector3 lastLedgePosition;

    public bool canMove = true;
    public bool StopInteractions = false;

    [Header("Audio (Efectos de Sonido)")]
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip clipSalto;
    [SerializeField] private AudioClip clipAterrizajeSuelo;
    private float alturaMinimaAterrizaje = 1.5f;
    private float tiempoMinimoAire = 0.15f;
    private float velocidadMinimaCaidaSFX = 6f;

    private bool estabaEnSuelo;
    private float puntoMasAltoEnAire;
    private float tiempoEnAire;
    private float maximaVelocidadCaidaAlcanzada;

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

        if (fuenteAudio == null)
        {
            fuenteAudio = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCheck = GetComponent<PlayerCheck>();
        jumpsLeft = jumpsMax;

        if (playerCameraScript == null)
            playerCameraScript = GetComponentInChildren<PlayerCamera>();

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleWindForces();

        if (ledgeGrabCooldown > 0f)
            ledgeGrabCooldown -= Time.deltaTime;

        bool hasActiveWind = windSources.Count > 0;
        bool windIsLifting = hasActiveWind && combinedWindForce.y > 0.01f && externalVelocity.y > 0.01f;
        bool isGrounded = characterController.isGrounded && moveDirection.y <= 0f && !windIsLifting;

        Vector3 edgeDirection = horizontalVelocity.sqrMagnitude > 0.01f ? horizontalVelocity.normalized : Vector3.zero;
        playerCheck.CheckForFloorEdge(isGrounded, edgeDirection);

        if (isGrounded)
        {
            if (!estabaEnSuelo && tiempoEnAire >= tiempoMinimoAire)
            {
                float distanciaCaida = puntoMasAltoEnAire - transform.position.y;
                if (distanciaCaida >= alturaMinimaAterrizaje && maximaVelocidadCaidaAlcanzada >= velocidadMinimaCaidaSFX)
                {
                    ReproducirAudio(clipAterrizajeSuelo);
                }
            }

            tiempoEnAire = 0f;
            puntoMasAltoEnAire = transform.position.y;
            maximaVelocidadCaidaAlcanzada = 0f;

            coyoteTimeCounter = coyoteTime;
            jumpsLeft = jumpsMax;
            lastWallCollider = null;
            isHanging = false;
            hangingLedgeTransform = null;
            isWallRunning = false;
            isWallSliding = false;

            if (playerCameraScript != null)
                playerCameraScript.SetLockPlayerRotation(false);

            if (moveDirection.y < 0f)
                moveDirection.y = -10f;
        }
        else
        {
            tiempoEnAire += Time.deltaTime;

            float velocidadCaidaActual = -moveDirection.y;
            if (velocidadCaidaActual > maximaVelocidadCaidaAlcanzada)
            {
                maximaVelocidadCaidaAlcanzada = velocidadCaidaActual;
            }

            if (estabaEnSuelo || transform.position.y > puntoMasAltoEnAire)
            {
                puntoMasAltoEnAire = transform.position.y;
            }

            coyoteTimeCounter -= Time.deltaTime;
            playerCheck.CheckForWalls();

            if (!isHanging && !isWallRunning && !isWallSliding && moveDirection.y <= 2f && ledgeClimbTimer <= 0f && ledgeGrabCooldown <= 0f)
            {
                playerCheck.CheckForLedge();
            }
        }

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;

        else
            jumpBufferCounter -= Time.deltaTime;

        if (!isGrounded && !isHanging && playerCheck.CanGrabLedge && !windIsLifting && ledgeClimbTimer <= 0f && ledgeGrabCooldown <= 0f)
        {
            isHanging = true;
            ledgeGrabTimer = 0.15f;

            moveDirection = Vector3.zero;
            horizontalVelocity = Vector3.zero;
            jumpBufferCounter = 0f;

            if (Physics.Raycast(transform.position + Vector3.up * 1.2f, transform.forward, out RaycastHit ledgeHit, 2f) ||
                Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out ledgeHit, 2f))
            {
                transform.rotation = Quaternion.LookRotation(-ledgeHit.normal);

                hangingLedgeTransform = ledgeHit.transform;
                lastLedgePosition = hangingLedgeTransform.position;
            }

            if (playerCameraScript != null)
                playerCameraScript.SetLockPlayerRotation(true);
        }

        if (isHanging)
        {
            ledgeGrabTimer -= Time.deltaTime;
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (ledgeGrabTimer <= 0f && (verticalInput < -0.5f || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C)))
            {
                isHanging = false;
                hangingLedgeTransform = null;
                playerCheck.ResetLedgeGrab();
                ledgeGrabCooldown = 0.5f;

                horizontalVelocity = -transform.forward * 3f;
                moveDirection.x = horizontalVelocity.x;
                moveDirection.z = horizontalVelocity.z;
                moveDirection.y = -4f;

                if (playerCameraScript != null)
                    playerCameraScript.SetLockPlayerRotation(false);
            }
            else if (ledgeGrabTimer <= 0f && (Input.GetButtonDown("Jump") || verticalInput > 0.5f))
            {
                isHanging = false;
                hangingLedgeTransform = null;
                playerCheck.ResetLedgeGrab();

                ledgeClimbTimer = 0.25f;
                moveDirection.y = ledgeBoostPower;
                jumpBufferCounter = 0f;

                if (playerCameraScript != null)
                    playerCameraScript.SetLockPlayerRotation(false);

                ReproducirAudio(clipSalto);
            }
        }

        if (ledgeClimbTimer > 0f)
        {
            ledgeClimbTimer -= Time.deltaTime;

            if (ledgeClimbTimer < 0.12f)
            {
                horizontalVelocity = transform.forward * ledgeForwardBoost;
                moveDirection.x = horizontalVelocity.x;
                moveDirection.z = horizontalVelocity.z;
            }
        }

        Collider currentWall = playerCheck.IsWallRight ? playerCheck.HitRight.collider : (playerCheck.IsWallLeft ? playerCheck.HitLeft.collider : null);
        bool wallDetected = (playerCheck.IsWallLeft || playerCheck.IsWallRight) && currentWall != null;
        bool inputForward = Input.GetAxisRaw("Vertical") > 0;

        if (!isGrounded && !isHanging && wallDetected && inputForward && moveDirection.y <= 1f && currentWall != lastWallCollider)
        {
            if (!isWallRunning)
            {
                isWallRunning = true;

                Vector3 rawWallNormal = playerCheck.IsWallRight ? playerCheck.HitRight.normal : playerCheck.HitLeft.normal;
                Vector3 wallNormal = rawWallNormal.sqrMagnitude > 0.001f ? rawWallNormal.normalized : transform.forward;
                Vector3 wallForward = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;

                if (wallForward.sqrMagnitude > 0.001f && Vector3.Dot(wallForward, transform.forward) < 0f)
                    wallForward = -wallForward;

                if (wallForward.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(wallForward);

                if (playerCameraScript != null)
                    playerCameraScript.SetLockPlayerRotation(true);
            }
        }
        else if (isWallRunning)
        {
            isWallRunning = false;
            if (playerCameraScript != null)
                playerCameraScript.SetLockPlayerRotation(false);
        }

        RaycastHit activeJumpHit = default;
        if (playerCheck.IsWallJumpFront) activeJumpHit = playerCheck.HitJumpFront;
        else if (playerCheck.IsWallJumpRight) activeJumpHit = playerCheck.HitJumpRight;
        else if (playerCheck.IsWallJumpLeft) activeJumpHit = playerCheck.HitJumpLeft;

        bool isAnyWallJumpDetected = activeJumpHit.collider != null;

        isWallSliding = !isGrounded && !isHanging && !isWallRunning && moveDirection.y <= 0f
                        && isAnyWallJumpDetected && activeJumpHit.collider != lastWallCollider;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float movementDirectionY = moveDirection.y;

        #region State Execution

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
        else if (isWallRunning)
        {
            Vector3 rawNormal = playerCheck.IsWallRight ? playerCheck.HitRight.normal : playerCheck.HitLeft.normal;
            Vector3 wallNormal = rawNormal.sqrMagnitude > 0.001f ? rawNormal.normalized : transform.forward;
            Vector3 wallForward = transform.forward;

            float currentSpeed = isRunning ? maxMomentumSpeed : walkSpeed;
            horizontalVelocity = wallForward * currentSpeed;

            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;
            moveDirection.y = -wallGravity;
            moveDirection -= wallNormal * wallStickForce;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(playerCheck.IsWallRight ? -12f : 12f);
                playerCameraScript.SetFOV(true);
            }
        }
        else if (isWallSliding)
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, airDeceleration * Time.deltaTime);

            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;
            moveDirection.y = -wallSlideSpeed;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(0f);
                playerCameraScript.SetFOV(false);
            }
        }
        else if (ledgeClimbTimer > 0f)
        {
            moveDirection.y = movementDirectionY;
        }
        else if (isGrounded)
        {
            Vector3 inputDirection = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");
            if (!canMove) inputDirection = Vector3.zero;

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                inputDirection.Normalize();
                float targetSpeed = isRunning ? maxMomentumSpeed : walkSpeed;
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, inputDirection * targetSpeed, runAcceleration * Time.deltaTime);
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
                bool isMovingForward = Input.GetAxisRaw("Vertical") > 0;
                playerCameraScript.SetFOV(isRunning && isMovingForward);
            }
        }
        else
        {
            Vector3 inputDirection = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");
            if (!canMove) inputDirection = Vector3.zero;

            Vector3 currentHorizontal = new Vector3(moveDirection.x, 0f, moveDirection.z);
            Vector3 newHorizontal;

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                inputDirection.Normalize();
                float targetSpeed = isRunning ? maxMomentumSpeed : walkSpeed;
                newHorizontal = Vector3.MoveTowards(currentHorizontal, inputDirection * targetSpeed, runAcceleration * 0.5f * Time.deltaTime);
            }
            else
            {
                newHorizontal = Vector3.MoveTowards(currentHorizontal, Vector3.zero, airDeceleration * Time.deltaTime);
            }

            newHorizontal = Vector3.ClampMagnitude(newHorizontal, maxMomentumSpeed);

            moveDirection.x = newHorizontal.x;
            moveDirection.z = newHorizontal.z;
            moveDirection.y = movementDirectionY;
            horizontalVelocity = newHorizontal;

            if (playerCameraScript != null)
                playerCameraScript.SetTilt(0f);
        }

        #endregion

        #region Jumping

        if (!isHanging && jumpBufferCounter > 0f && canMove)
        {
            if (isWallRunning || (wallDetected && inputForward && !isGrounded))
            {
                Vector3 rawNormal = playerCheck.IsWallRight ? playerCheck.HitRight.normal : (playerCheck.IsWallLeft ? playerCheck.HitLeft.normal : -transform.forward);
                Vector3 wallNormal = rawNormal.sqrMagnitude > 0.001f ? rawNormal.normalized : -transform.forward;

                Vector3 inheritedVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);

                horizontalVelocity = Vector3.ClampMagnitude(inheritedVelocity + wallNormal * wallJumpOutForce, maxMomentumSpeed);
                moveDirection.x = horizontalVelocity.x;
                moveDirection.z = horizontalVelocity.z;
                moveDirection.y = wallJumpUpForce;

                jumpsLeft = Mathf.Max(0, jumpsLeft - 1);
                lastWallCollider = currentWall != null ? currentWall : activeJumpHit.collider;
                isWallRunning = false;

                if (playerCameraScript != null)
                    playerCameraScript.SetLockPlayerRotation(false);

                jumpBufferCounter = 0f;
                ReproducirAudio(clipSalto);
            }
            else if (isWallSliding && isAnyWallJumpDetected)
            {
                Vector3 wallNormal = activeJumpHit.normal.sqrMagnitude > 0.001f ? activeJumpHit.normal.normalized : -transform.forward;
                ApplyWallJumpVelocity(wallNormal);

                moveDirection.y = wallJumpUpForce;
                lastWallCollider = activeJumpHit.collider;
                isWallSliding = false;
                jumpBufferCounter = 0f;
                ReproducirAudio(clipSalto);
            }
            else if (coyoteTimeCounter > 0f || jumpsLeft > 0)
            {
                moveDirection.y = jumpPower;
                jumpsLeft--;
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
                ReproducirAudio(clipSalto);
            }
        }

        #endregion

        #region Gravity

        if (windIsLifting)
        {
            moveDirection.y = Mathf.Max(moveDirection.y, externalVelocity.y);
        }
        else if (!isGrounded && !isHanging && !isWallRunning && !isWallSliding && ledgeClimbTimer <= 0f)
        {
            moveDirection.y -= gravity * Time.deltaTime;
            moveDirection.y = Mathf.Max(moveDirection.y, -maxFallSpeed);
        }

        #endregion

        #region Platform & Final Movement

        Vector3 platformDelta = playerCheck != null ? playerCheck.GetPlatformDelta(isGrounded || isHanging) : Vector3.zero;

        Vector3 ledgeDelta = Vector3.zero;
        if (isHanging && hangingLedgeTransform != null)
        {
            ledgeDelta = hangingLedgeTransform.position - lastLedgePosition;
            lastLedgePosition = hangingLedgeTransform.position;
        }

        characterController.Move((moveDirection + externalVelocity) * Time.deltaTime + platformDelta + ledgeDelta);

        #endregion

        estabaEnSuelo = isGrounded;
    }

    private void ReproducirAudio(AudioClip clip)
    {
        if (fuenteAudio != null && clip != null)
        {
            fuenteAudio.PlayOneShot(clip);
        }
    }

    private void HandleWindForces()
    {
        combinedWindForce = Vector3.zero;
        List<plataforms> invalidSources = null;

        foreach (KeyValuePair<plataforms, Vector3> wind in windSources)
        {
            if (wind.Key == null)
            {
                if (invalidSources == null) invalidSources = new List<plataforms>();
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

        combinedWindForce = Vector3.ClampMagnitude(combinedWindForce, maxExternalVelocity);
        bool hasActiveWind = windSources.Count > 0;

        float speed = hasActiveWind ? windAcceleration : windDeceleration;
        Vector3 targetVel = hasActiveWind ? combinedWindForce : Vector3.zero;

        externalVelocity = Vector3.MoveTowards(externalVelocity, targetVel, speed * Time.deltaTime);
        externalVelocity = Vector3.ClampMagnitude(externalVelocity, maxExternalVelocity);
    }

    #region Wall Jump

    private void ApplyWallJumpVelocity(Vector3 wallNormal)
    {
        Vector3 inheritedVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);
        horizontalVelocity = Vector3.ClampMagnitude(inheritedVelocity + wallNormal * wallJumpOutForce, maxMomentumSpeed);

        moveDirection.x = horizontalVelocity.x;
        moveDirection.z = horizontalVelocity.z;
    }

    #endregion

    #region Wind API

    public void ApplyWind(plataforms source, Vector3 acceleration)
    {
        if (source != null) windSources[source] = acceleration;
    }

    public void StopWind(plataforms source)
    {
        if (source != null) windSources.Remove(source);
    }

    #endregion
}