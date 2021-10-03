using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.XR;

public class MovementTrigger : MonoBehaviour
{
    public float slowDown;

    private SlowDownMovementOverride movementOverride;

    private void Start()
    {
        movementOverride = new SlowDownMovementOverride();
        movementOverride.slowDownFactor = slowDown;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(other.TryGetComponent(out Movement movement))
            {
                movement.AddMovementOverride(movementOverride);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (other.TryGetComponent(out Movement movement))
            {
                movement.RemoveMovementOverride(movementOverride);
            }
        }
    }
}

public class SlowDownMovementOverride : MovementOverride
{
    public float slowDownFactor = 1;

    public override Vector3 ProcessMovement(Vector3 direction)
    {
        return direction * slowDownFactor;
    }
}
