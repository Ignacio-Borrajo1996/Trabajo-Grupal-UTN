using UnityEngine;

public class PlayerMc : MonoBehaviour
{
    //PlayerMMC (Player Movement, Mechanics and Camera)

    // Movement Values
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

    bool isWallLeft;                     
    bool isWallRight;                 
    bool isWallRunning;
  
    RaycastHit hitLeft;
    RaycastHit hitRight;
    Collider lastWall;

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
        //Wall Run Check
        CheckForWall();

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

            if (currentWall != null && currentWall != lastWall)
            {
                jumpsLeft = jumpsMax;
                lastWall = currentWall;
            }
        }

        float movementDirectionY = moveDirection.y;

        //Movement
        #region Handles Movment
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isWallRunning)
        {
            //WallRun
            float currentWallSpeed = isRunning ? runSpeed : walkSpeed;
            moveDirection = forward * currentWallSpeed;
            moveDirection.y = -wallGravity;
        }
        else
        {
            // Normal Movement
            float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
            moveDirection.y = movementDirectionY;
        }
        #endregion

        //Jump
        if (characterController.isGrounded)
        {
            jumpsLeft = jumpsMax;
            lastWall = null;
        }

        #region Handles Jumping
        if (Input.GetButtonDown("Jump") && canMove && (characterController.isGrounded || jumpsLeft > 0))
        {
            moveDirection.y = jumpPower;
            jumpsLeft--;
        }
        else if (!isWallRunning)
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded && !isWallRunning)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        #endregion

        // Cam
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

    void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, transform.right, out hitRight, wallCheckDistance, wallLayer);
        isWallLeft = Physics.Raycast(transform.position, -transform.right, out hitLeft, wallCheckDistance, wallLayer);
    }

    public static PlayerMc Instance;

    private void Awake()
    {
        Instance = this;
    }
}
