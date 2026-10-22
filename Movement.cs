using UnityEngine;

public class PlayerMc : MonoBehaviour
{
    //PlayerMMC (Player Movement, Mechanics and Camera)

    //Movement Values
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private float gravity = 12f;
    [SerializeField] private float airControl = 4f;
    private Vector3 moveDirection = Vector3.zero;

    [SerializeField] private int jumpsMax = 2;
    public int jumpsLeft;

    // Game Feel
    [Header("Juice / Game Feel")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [SerializeField] private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    //Wall Run Values
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 1.2f;
    [SerializeField] private float wallGravity = 2f;
    [SerializeField] private float wallStickForce = 3f;

    private bool isWallLeft;
    private bool isWallRight;
    private bool isWallRunning;
    private RaycastHit hitLeft;
    private RaycastHit hitRight;
    private Collider lastWallRun;

    //Wall Jump Values
    [SerializeField] private LayerMask wallJumpLayer;
    [SerializeField] private float wallJumpCheckDistance = 1.2f;
    [SerializeField] private float wallSlideSpeed = 1.5f;
    [SerializeField] private float wallJumpUpForce = 10f;
    [SerializeField] private float wallJumpOutForce = 8f;

    private bool isWallJumpLeft;
    private bool isWallJumpRight;
    private bool isWallJumpFront;
    private bool isWallSliding;
    private RaycastHit hitJumpLeft;
    private RaycastHit hitJumpRight;
    private RaycastHit hitJumpFront;
    private Collider lastWallJumped;

    //Grab / Ledge
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField] private float reachDistance = 0.8f;
    [SerializeField] private float chestHeight = 1.2f;
    [SerializeField] private float headHeight = 2.2f;
    [SerializeField] private float ledgeBoostPower = 8f;

    public bool canMove = true;
    public bool StopInteractions = false;

    [Header("Referencias")]
    [SerializeField] private PlayerCamera playerCameraScript;

    private CharacterController characterController;

    public static PlayerMc Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        jumpsLeft = jumpsMax;

        if (playerCameraScript == null)
        {
            playerCameraScript = GetComponentInChildren<PlayerCamera>();
        }
    }

    private void Update()
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpsLeft = jumpsMax;
            lastWallRun = null;
            lastWallJumped = null;

            if (moveDirection.y < 0)
            {
                moveDirection.y = -2f;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            CheckForWallRun();
            CheckForWallJump();

            if (!isWallRunning && !isWallSliding && moveDirection.y <= 2f)
            {
                CheckForLedgeBoost();
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

        // WALL RUN
        Collider currentWall = isWallRight ? hitRight.collider : hitLeft.collider;
        bool canWallRun = !isGrounded && (isWallLeft || isWallRight) && Input.GetAxisRaw("Vertical") > 0 && moveDirection.y <= 0;

        if (canWallRun)
        {
            if (isWallRunning || currentWall != lastWallRun)
            {
                isWallRunning = true;
            }
            else
            {
                isWallRunning = false;
            }
        }
        else
        {
            isWallRunning = false;
        }

        if (isWallRunning)
        {
            if (currentWall != null && currentWall != lastWallRun)
            {
                jumpsLeft = jumpsMax;
                lastWallRun = currentWall;
            }
        }

        // WALL JUMP
        RaycastHit activeJumpHit = default;
        bool isAnyWallJumpDetected = false;

        if (isWallJumpFront)
        {
            activeJumpHit = hitJumpFront;
            isAnyWallJumpDetected = true;
        }
        else if (isWallJumpRight)
        {
            activeJumpHit = hitJumpRight;
            isAnyWallJumpDetected = true;
        }
        else if (isWallJumpLeft)
        {
            activeJumpHit = hitJumpLeft;
            isAnyWallJumpDetected = true;
        }

        if (!isGrounded && !isWallRunning && isAnyWallJumpDetected && moveDirection.y < 0 && activeJumpHit.collider != lastWallJumped)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        // Movement
        #region Handles Movment
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float movementDirectionY = moveDirection.y;

        if (isWallRunning)
        {
            // Wall run
            Vector3 wallNormal = isWallRight ? hitRight.normal : hitLeft.normal;
            Vector3 wallForward = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            moveDirection = wallForward * currentSpeed;
            moveDirection.y = -wallGravity;
            moveDirection -= wallNormal * wallStickForce;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(isWallRight ? -12f : 12f);
                playerCameraScript.SetFOV(true);
            }
        }
        else if (isWallSliding)
        {
            moveDirection.x = 0;
            moveDirection.z = 0;
            moveDirection.y = -wallSlideSpeed;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(0f);
                playerCameraScript.SetFOV(false);
            }
        }
        else if (isGrounded)
        {
            float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxisRaw("Vertical") : 0;
            float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxisRaw("Horizontal") : 0;

            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
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
            float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxisRaw("Vertical") : 0;
            float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxisRaw("Horizontal") : 0;

            Vector3 targetInput = (forward * curSpeedX) + (right * curSpeedY);
            Vector3 currentHorizontal = new Vector3(moveDirection.x, 0f, moveDirection.z);

            Vector3 newHorizontal = Vector3.Lerp(currentHorizontal, targetInput, airControl * Time.deltaTime);

            moveDirection.x = newHorizontal.x;
            moveDirection.z = newHorizontal.z;
            moveDirection.y = movementDirectionY;

            if (playerCameraScript != null)
            {
                playerCameraScript.SetTilt(0f);
            }
        }
        #endregion

        // Jump
        #region Handles Jumping
        if (jumpBufferCounter > 0f && canMove)
        {
            if (isWallRunning)
            {
                // Wall run jump
                Vector3 wallNormal = isWallRight ? hitRight.normal : hitLeft.normal;
                Vector3 jumpDirection = (Vector3.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);

                moveDirection = jumpDirection;
                jumpsLeft--;
                isWallRunning = false;
                jumpBufferCounter = 0f;
            }
            else if (isWallSliding)
            {
                // Wall jump
                Vector3 wallNormal = activeJumpHit.normal;
                Vector3 jumpDirection = (Vector3.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);

                moveDirection = jumpDirection;
                lastWallJumped = activeJumpHit.collider;
                isWallSliding = false;
                jumpBufferCounter = 0f;
            }
            else if (coyoteTimeCounter > 0f || jumpsLeft > 0)
            {
                // Jump normal
                moveDirection.y = jumpPower;
                jumpsLeft--;
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
        }
        else if (!isWallRunning && !isWallSliding)
        {
            moveDirection.y = movementDirectionY;
        }

        if (!isGrounded && !isWallRunning && !isWallSliding)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        #endregion

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void CheckForWallRun()
    {
        isWallRight = Physics.Raycast(transform.position, transform.right, out hitRight, wallCheckDistance, wallLayer);
        isWallLeft = Physics.Raycast(transform.position, -transform.right, out hitLeft, wallCheckDistance, wallLayer);
    }

    private void CheckForWallJump()
    {
        isWallJumpRight = Physics.Raycast(transform.position, transform.right, out hitJumpRight, wallJumpCheckDistance, wallJumpLayer);
        isWallJumpLeft = Physics.Raycast(transform.position, -transform.right, out hitJumpLeft, wallJumpCheckDistance, wallJumpLayer);
        isWallJumpFront = Physics.Raycast(transform.position, transform.forward, out hitJumpFront, wallJumpCheckDistance, wallJumpLayer);
    }

    private void CheckForLedgeBoost()
    {
        Vector3 originChest = transform.position + Vector3.up * chestHeight;

        if (Physics.Raycast(originChest, transform.forward, out RaycastHit wallHit, reachDistance, obstacleLayers))
        {
            Vector3 originHead = transform.position + Vector3.up * headHeight + transform.forward * (reachDistance * 0.8f);

            if (Physics.Raycast(originHead, Vector3.down, out RaycastHit ledgeHit, headHeight - chestHeight + 0.5f, obstacleLayers))
            {
                moveDirection.y = ledgeBoostPower;
                moveDirection += transform.forward * 3f;
            }
        }
    }

    public void AplicarEmpuje(Vector3 fuerza)
    {
        if (characterController != null && characterController.enabled)
        {
            characterController.Move(fuerza * Time.deltaTime);

            if (fuerza.y > 0f && moveDirection.y < fuerza.y)
            {
                moveDirection.y = Mathf.Max(moveDirection.y, fuerza.y * 0.5f);
            }
        }
    }
}
