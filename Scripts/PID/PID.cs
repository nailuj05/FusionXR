using UnityEngine;

[System.Serializable]
public class PIDSettings
{
    [SerializeField]
    public float P, I, D;
}

public class PID {
    public PIDSettings s;

	public PID(float P, float I, float D) {
        s = new PIDSettings();
		s.P = P;
		s.I = I;
		s.D = D;
	}

    public PID(PIDSettings settings)
    {
        s = settings;
    }

    float error;
    public float CalcScalar(float target, float current, float deltaTime)
    {
        error = target - current;
        return CalcScalar(error, deltaTime);
    }

    float derivative, lastError, integral;
	public float CalcScalar(float error, float deltaTime) {
		integral += error * deltaTime;
		derivative = (error - lastError) / deltaTime;
		lastError = error;
		return error * s.P + integral * s.I + derivative * s.D;
	}
}
