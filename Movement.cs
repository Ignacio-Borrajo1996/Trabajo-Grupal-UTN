using UnityEngine;

public class PlayerMc : MonoBehaviour
{
    //PlayerMMC (Player Movement, Mechanics and Camera)

    //Movement Values
    public float walkSpeed = 8f;
    public float runSpeed = 15f;
    public float jumpPower = 5f;
    public float gravity = 10f;
    Vector3 moveDirection = Vector3.zero;

    public int jumpsMax = 2;
    public int jumpsLeft;

    //Wall Run Values
    public LayerMask wallLayer;
    public float wallCheckDistance = 1.2f;
    public float wallGravity = 2f;
    public float wallStickForce = 3f;

    bool isWallLeft;
    bool isWallRight;
    bool isWallRunning;
    RaycastHit hitLeft;
    RaycastHit hitRight;
    Collider lastWallRun;

    //Wall Jump Values
    public LayerMask wallJumpLayer;
    public float wallJumpCheckDistance = 1.2f;
    public float wallSlideSpeed = 1.5f;
    public float wallJumpUpForce = 10f;
    public float wallJumpOutForce = 8f;

    bool isWallJumpLeft;
    bool isWallJumpRight;
    bool isWallSliding;
    RaycastHit hitJumpLeft;
    RaycastHit hitJumpRight;
    Collider lastWallJumped;

    // Cam Values
    public Camera playerCamera;
    public bool canMove = true;
    public bool StopInteractions = false;
    public float lookSpeed = 8f;
    public float lookXLimit = 45f;
    float rotationX = 0;

    CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        jumpsLeft = jumpsMax;
    }

    void Update()
    {
        CheckForWallRun();
        CheckForWallJump();

        if (characterController.isGrounded)
        {
            jumpsLeft = jumpsMax;
            lastWallRun = null;
            lastWallJumped = null;

            if (moveDirection.y < 0)
            {
                moveDirection.y = -2f;
            }
        }

        //Wall run
        if (!characterController.isGrounded && (isWallLeft || isWallRight) && Input.GetAxis("Vertical") > 0 && moveDirection.y <= 0)
        {
            isWallRunning = true;
        }
        else
        {
            isWallRunning = false;
        }

        if (isWallRunning)
        {
            Collider currentWall = isWallRight ? hitRight.collider : hitLeft.collider;
            if (currentWall != null && currentWall != lastWallRun)
            {
                jumpsLeft = jumpsMax;
                lastWallRun = currentWall;
            }
        }

        RaycastHit activeJumpHit = isWallJumpRight ? hitJumpRight : hitJumpLeft;
        if (!characterController.isGrounded && !isWallRunning && (isWallJumpLeft || isWallJumpRight) && moveDirection.y < 0 && activeJumpHit.collider != lastWallJumped)
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
            //Wall run
            Vector3 wallNormal = isWallRight ? hitRight.normal : hitLeft.normal;
            Vector3 wallForward = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            moveDirection = wallForward * currentSpeed;
            moveDirection.y = -wallGravity;
            moveDirection -= wallNormal * wallStickForce;
        }
        else if (isWallSliding)
        {
            moveDirection.x = 0;
            moveDirection.z = 0;
            moveDirection.y = -wallSlideSpeed;
        }
        else
        {
            float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
            moveDirection.y = movementDirectionY;
        }
        #endregion

        // Jump
        #region Handles Jumping
        if (Input.GetButtonDown("Jump") && canMove)
        {
            if (isWallRunning)
            {
                //Wall run jump
                Vector3 wallNormal = isWallRight ? hitRight.normal : hitLeft.normal;
                Vector3 jumpDirection = (Vector3.up * 1.2f + wallNormal * 1.5f).normalized;

                moveDirection = jumpDirection * jumpPower;
                jumpsLeft--;
                isWallRunning = false;
            }
            else if (isWallSliding)
            {
                //Wall jump
                Vector3 wallNormal = activeJumpHit.normal;
                Vector3 jumpDirection = (Vector3.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);

                moveDirection = jumpDirection;
                lastWallJumped = activeJumpHit.collider;
                isWallSliding = false;
            }
            else if (characterController.isGrounded || jumpsLeft > 0)
            {
                //Jump
                moveDirection.y = jumpPower;
                jumpsLeft--;
            }
        }
        else if (!isWallRunning && !isWallSliding)
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded && !isWallRunning && !isWallSliding)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        #endregion

        //Cam
        #region Handles Rotation
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        #endregion
    }

    void CheckForWallRun()
    {
        isWallRight = Physics.Raycast(transform.position, transform.right, out hitRight, wallCheckDistance, wallLayer);
        isWallLeft = Physics.Raycast(transform.position, -transform.right, out hitLeft, wallCheckDistance, wallLayer);
    }

    void CheckForWallJump()
    {
        isWallJumpRight = Physics.Raycast(transform.position, transform.right, out hitJumpRight, wallJumpCheckDistance, wallJumpLayer);
        isWallJumpLeft = Physics.Raycast(transform.position, -transform.right, out hitJumpLeft, wallJumpCheckDistance, wallJumpLayer);
    }

    public static PlayerMc Instance;

    private void Awake()
    {
        Instance = this;
    }
}
