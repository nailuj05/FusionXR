using UnityEngine;
using System.Collections;

public enum PIDTestMode
{
    Force,
    Velocity,
    Position,
    AngularVelocity
}

public class PIDTest : MonoBehaviour {
    [Header("Settings")]
    public PIDTestMode mode;
    public PIDSettings PIDSetttings;
    public ForceMode forceMode;

    public Transform target;
	public Transform current;
    public Rigidbody currentRB;

    private PIDVector _PIDVector;
    private PIDTorque _PIDTorque;

    private void Start()
    {
        _PIDVector = new PIDVector(PIDSetttings);
        _PIDTorque = new PIDTorque(PIDSetttings);
    }

    private void FixedUpdate()
    {
        switch (mode)
        {
            case PIDTestMode.Velocity:
                currentRB.velocity = _PIDVector.CalcVector(target.position, current.position, Time.fixedDeltaTime);
                break;

            case PIDTestMode.Force:
                currentRB.AddForce(_PIDVector.CalcVector(target.position, current.position, Time.fixedDeltaTime), forceMode);
                break;

            case PIDTestMode.Position:
                current.position += _PIDVector.CalcVector(target.position, current.position, Time.fixedDeltaTime);
                break;

            case PIDTestMode.AngularVelocity:
                currentRB.angularVelocity = _PIDTorque.CalcTorque(target.rotation, current.rotation, Time.fixedDeltaTime);
                break;
        }
    }
}
	