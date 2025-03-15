using UnityEngine;

public class WrongWay : MonoBehaviour
{
    public Transform nextCheckpoint;
    private Vector3 lastPosition;
    private bool goingWrongWay = false;

    void Update()
    {
        Vector3 movementDirection = (transform.position - lastPosition).normalized;
        Vector3 correctDirection = (nextCheckpoint.position - transform.position).normalized;

        if (Vector3.Dot(movementDirection, correctDirection) < 0) // Moving opposite direction
        {
            goingWrongWay = true;
            Debug.Log("Wrong Way!");
        }
        else
        {
            goingWrongWay = false;
        }

        lastPosition = transform.position;
    }
}
