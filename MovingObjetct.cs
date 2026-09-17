using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 3f;

    protected Vector3 currentTarget;

    protected virtual void Start()
    {
        currentTarget = pointB.position;
    }

    protected virtual void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
        {
            if (currentTarget == pointB.position)
            {
                currentTarget = pointA.position;
            }
            else
            {
                currentTarget = pointB.position;
            }
        }
    }
}
