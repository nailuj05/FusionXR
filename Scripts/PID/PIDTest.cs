using UnityEngine;
using System.Collections;

public enum PIDTestMode
{
    Force,
    Velocity,
    Position,
    Rotation
}

public class PIDTest : MonoBehaviour {
    [Header("Settings")]
    public PIDTestMode mode;
    public PIDSettings PIDSetttings;
    public ForceMode forceMode;

    public Transform target;
	public Transform current;
    public Rigidbody currentRB;

    private PIDVector _PIDController;
	
    private void Start()
    {
        _PIDController = new PIDVector(PIDSetttings);
    }

    private void FixedUpdate()
    {
        if (mode == PIDTestMode.Velocity)
            currentRB.velocity = _PIDController.CalcVector(target.position, current.position, Time.deltaTime);

        else if (mode == PIDTestMode.Force)
            currentRB.AddForce(_PIDController.CalcVector(target.position, current.position, Time.fixedDeltaTime), forceMode);

        else if (mode == PIDTestMode.Position)
            current.position += _PIDController.CalcVector(target.position, current.position, Time.deltaTime);
    }
}
	