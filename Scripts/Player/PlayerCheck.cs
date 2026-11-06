using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class PlayerCheck : MonoBehaviour
{
    #region Values
    [Header("Wall Run")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 1.2f;

    [Header("Wall Jump")]
    [SerializeField] private LayerMask wallJumpLayer;
    [SerializeField] private float wallJumpCheckDistance = 1.2f;

    [Header("Ledge Grab")]
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField] private float reachDistance = 0.8f;
    [SerializeField] private float chestHeight = 1.2f;
    [SerializeField] private float headHeight = 2.2f;

    [Header("Floor Edge Boost")]
    [SerializeField] private LayerMask floorEdgeLayers = ~0;
    [SerializeField, Min(0.1f)] private float edgeCheckDistance = 0.8f;
    [SerializeField, Min(0.05f)] private float edgeCheckHeight = 0.25f;
    [SerializeField, Min(0.1f)] private float edgeGroundCheckDepth = 0.6f;

    private CharacterController characterController;
    private plataforms currentPlatform;
    #endregion
    public bool IsWallLeft { get; private set; }
    public bool IsWallRight { get; private set; }
    public RaycastHit HitLeft { get; private set; }
    public RaycastHit HitRight { get; private set; }

    public bool IsWallJumpLeft { get; private set; }
    public bool IsWallJumpRight { get; private set; }
    public bool IsWallJumpFront { get; private set; }

    public RaycastHit HitJumpLeft { get; private set; }
    public RaycastHit HitJumpRight { get; private set; }
    public RaycastHit HitJumpFront { get; private set; }

    public bool CanGrabLedge { get; private set; }
    public bool CanEdgeBoost { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    #region CheckWalls
    public void CheckForWalls()
    {
        // El centro del CharacterController suele dar una detección de paredes más fiable que el pivote.
        Vector3 wallOrigin = characterController != null ? characterController.bounds.center : transform.position;

        IsWallRight = Physics.Raycast(wallOrigin, transform.right,
        out RaycastHit rightHit, wallCheckDistance, wallLayer);
        HitRight = rightHit;

        IsWallLeft = Physics.Raycast(wallOrigin, -transform.right,
        out RaycastHit leftHit, wallCheckDistance, wallLayer);
        HitLeft = leftHit;

        IsWallJumpRight = Physics.Raycast(wallOrigin, transform.right,
        out RaycastHit jumpRightHit, wallJumpCheckDistance, wallJumpLayer);
        HitJumpRight = jumpRightHit;

        IsWallJumpLeft = Physics.Raycast(wallOrigin, -transform.right,
        out RaycastHit jumpLeftHit, wallJumpCheckDistance, wallJumpLayer);
        HitJumpLeft = jumpLeftHit;

        IsWallJumpFront = Physics.Raycast(wallOrigin, transform.forward,
        out RaycastHit jumpFrontHit, wallJumpCheckDistance, wallJumpLayer);
        HitJumpFront = jumpFrontHit;
    }
    #endregion 
    #region CheckLedge
    public void CheckForLedge()
    {
        CanGrabLedge = false;
        Vector3 chestOrigin = transform.position + Vector3.up * chestHeight;

        if (!Physics.Raycast(chestOrigin, transform.forward, out RaycastHit wallHit,
            reachDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
            return;

        if (Mathf.Abs(wallHit.normal.y) > 0.2f)
            return;

        Vector3 topCheckOrigin = transform.position + Vector3.up * headHeight + transform.forward * (wallHit.distance + 0.05f);
        float topCheckDistance = Mathf.Max(0.05f, headHeight - chestHeight);

        if (Physics.Raycast(topCheckOrigin, Vector3.down, out RaycastHit ledgeHit,
        topCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
        CanGrabLedge = ledgeHit.normal.y >= 0.7f;
    }
    #endregion
    #region CheckFLorEdge
    public void CheckForFloorEdge(bool isGrounded, Vector3 movementDirection)
    {
        CanEdgeBoost = false;
        if (!isGrounded || characterController == null)
          return;

        movementDirection.y = 0f;
        // Sin movimiento horizontal no hay impulso de borde que activar.
        if (movementDirection.sqrMagnitude < 0.01f)
          return;
        movementDirection.Normalize();

        float feetY = characterController.bounds.min.y;
        Vector3 probeOrigin = new Vector3(transform.position.x,feetY + edgeCheckHeight, transform.position.z) + movementDirection * edgeCheckDistance;

        bool groundAhead = Physics.Raycast(probeOrigin, Vector3.down,
            edgeCheckHeight + edgeGroundCheckDepth, floorEdgeLayers,
            QueryTriggerInteraction.Ignore);

        CanEdgeBoost = !groundAhead;
    }
    #endregion
    public void RegisterPlatformHit(ControllerColliderHit hit)
    {
        if (hit.normal.y < 0.5f)
            return;

        plataforms platform = hit.collider.GetComponentInParent<plataforms>();
        if (platform != null)
        currentPlatform = platform;
    }
    public void ResetLedgeGrab()
    {
        CanGrabLedge = false;
    }
    public Vector3 GetPlatformDelta(bool isGrounded)
    {
        if (!isGrounded)
        {
            currentPlatform = null;
            return Vector3.zero;
        }

        if (currentPlatform == null)
            return Vector3.zero;

        return currentPlatform.DeltaThisFrame;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        RegisterPlatformHit(hit);
    }
}
