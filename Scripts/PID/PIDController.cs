using UnityEngine;

[System.Serializable]
public abstract class PIDController { }

public class PIDVector : PIDController
{
    private PID pidX, pidY, pidZ;
    float lastErrorX, lastErrorY, lastErrorZ;

    public PIDVector(PIDSettings settings)
    {
        pidZ = new PID(settings);
        pidX = new PID(settings);
        pidY = new PID(settings);
    }

    public Vector3 CalcVector(Vector3 target, Vector3 current, float deltaTime)
    {
        return new Vector3(
            pidX.CalcScalar(target.x, current.x, deltaTime),
            pidY.CalcScalar(target.y, current.y, deltaTime),
            pidZ.CalcScalar(target.z, current.z, deltaTime)
        );
    }
}

public class PIDFloat : PIDController
{
    private PID pid;
    float lastError;

    public PIDFloat(PIDSettings settings)
    {
        pid = new PID(settings);
    }

    public float CalcFloat(float target, float current, float deltaTime)
    {
        return pid.CalcScalar(target, current, deltaTime);
    }
}

public class PIDTorque : PIDController
{
    PID pid;

    public PIDTorque(PIDSettings settings)
    {
        pid = new PID(settings);
    }

    Quaternion d;
    Vector3 angVel;
    public Vector3 CalcTorque(Quaternion target, Quaternion current, float deltaTime)
    {
        d = Quaternion.Inverse(current) * target;
        d.ToAngleAxis(out float angleError, out Vector3 axisError);

        angVel = axisError * pid.CalcScalar(angleError, deltaTime);

        return angVel;
    }
}

#region WIP

//public float rotFrequency, rotDampening;

//Vector3 torque;
//Quaternion deltaQ;
//float kp, kd, g, ksg, kdg;
///// <summary>
///// Returns a torque (ACCELERATION)
///// </summary>
///// <param name="targetRot"></param>
///// <param name="currentRot"></param>
///// <param name="currentVel"></param>
///// <param name="deltaTime"></param>
///// <returns></returns>
//public Vector3 CalcTorque(Quaternion targetRot, Quaternion currentRot, Vector3 currentVel, float deltaTime)
//{
//    kp = (6f * rotFrequency) * (6f * rotFrequency) * 0.25f;
//    kd = 4.5f * rotFrequency * rotDampening;
//    g = 1 / (1 + kd * deltaTime + kp * deltaTime * deltaTime);
//    ksg = kp * g;
//    kdg = (kd + kp * deltaTime) * g;
//    deltaQ = targetRot * Quaternion.Inverse(currentRot);

//    if (deltaQ.w < 0)
//    {
//        deltaQ.x = -deltaQ.x;
//        deltaQ.y = -deltaQ.y;
//        deltaQ.z = -deltaQ.z;
//        deltaQ.w = -deltaQ.w;
//    }
//    deltaQ.ToAngleAxis(out float angle, out Vector3 axis);
//    axis.Normalize();
//    axis *= Mathf.Deg2Rad;
//    torque = ksg * axis * angle + -currentVel * kdg;
//    return torque;
//}

//Quaternion q, d;
//public Quaternion CalcQuaternion(Quaternion target, Quaternion current, float deltaTime)
//{
//    target.ToAngleAxis(out float targetAngle, out Vector3 targetAxis);
//    current.ToAngleAxis(out float currentAngle, out Vector3 currentAxis);

//    q = Quaternion.AngleAxis(CalcScalar(targetAngle, currentAngle, deltaTime), CalcVector(targetAxis, currentAxis, deltaTime));

//    return q;
//}
#endregion