using UnityEngine;
public class GrapplingHook : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform hookPoint;

    [Header("Hook Settings")]
    public float maxDistance = 50f;
    public LayerMask grappleLayer;

    [Header("Rope")]
    public LineRenderer rope;

    private bool isGrappling = false;
    private Vector3 grapplePoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryGrapple();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        if (isGrappling)
        {
            UpdateRope();
        }
    }

    void TryGrapple()
    {
        Ray ray = new Ray(playerCamera.transform.position,playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleLayer))
        {
            isGrappling = true;
            grapplePoint = hit.point;

            rope.enabled = true;
            rope.positionCount = 2;

            UpdateRope();
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        rope.enabled = false;
    }

    void UpdateRope()
    {
        rope.SetPosition(0, hookPoint.position);
        rope.SetPosition(1, grapplePoint);
    }
}