using UnityEngine;

public class F12026Tire : MonoBehaviour
{
    [Header("Tyre")]
    [Min(0.1f)] public float radius = 0.36f;
    [Min(1f)] public float wheelInertia = 1.15f;
    [Min(0.1f)] public float nominalLoad = 1900f;

    [Header("Grip / Slip")]
    [Range(0.1f, 2f)] public float peakFriction = 1.72f;
    [Range(0.1f, 2f)] public float loadSensitivity = 0.78f;
    [Min(0.01f)] public float longitudinalSlipAtPeak = 0.10f;
    [Min(0.01f)] public float lateralSlipAtPeakDeg = 7.5f;
    [Range(0.1f, 2f)] public float combinedSlipExponent = 1.35f;
    [Range(0f, 1f)] public float rollingResistance = 0.012f;

    public float WheelAngularSpeedRadPerSec { get; private set; }
    public float LongitudinalSlip { get; private set; }
    public float SlipAngleDeg { get; private set; }
    public float LongitudinalForce { get; private set; }
    public float LateralForce { get; private set; }
    public float GripMultiplier { get; private set; } = 1f;

    public Vector3 Simulate(
        Vector3 localVelocity,
        float normalLoad,
        float driveTorque,
        float brakeTorque,
        float steeringAngleRad,
        float dt,
        bool grounded)
    {
        if (!grounded || normalLoad <= 0f)
        {
            LongitudinalSlip = 0f;
            SlipAngleDeg = 0f;
            LongitudinalForce = 0f;
            LateralForce = 0f;
            return Vector3.zero;
        }

        float forwardSpeed = localVelocity.z;
        float lateralSpeed = localVelocity.x;
        float wheelSurfaceSpeed = WheelAngularSpeedRadPerSec * radius;
        float denominator = Mathf.Max(Mathf.Abs(forwardSpeed), 2f);

        LongitudinalSlip = Mathf.Clamp(
            (wheelSurfaceSpeed - forwardSpeed) / denominator, -3f, 3f);

        float slipAngleRad = Mathf.Atan2(
            -lateralSpeed,
            Mathf.Max(Mathf.Abs(forwardSpeed), 1f));

        SlipAngleDeg = slipAngleRad * Mathf.Rad2Deg;

        float loadRatio = Mathf.Max(normalLoad, 1f) / Mathf.Max(nominalLoad, 1f);
        float loadGrip = Mathf.Pow(loadRatio, -loadSensitivity + 1f);
        GripMultiplier = Mathf.Clamp(loadGrip, 0.70f, 1.12f);

        float mu = peakFriction * GripMultiplier;
        float maxForce = mu * normalLoad;

        float sx = LongitudinalSlip / longitudinalSlipAtPeak;
        float sy = Mathf.Tan(slipAngleRad) /
                   Mathf.Tan(lateralSlipAtPeakDeg * Mathf.Deg2Rad);

        float combined = Mathf.Pow(
            Mathf.Pow(Mathf.Abs(sx), combinedSlipExponent) +
            Mathf.Pow(Mathf.Abs(sy), combinedSlipExponent),
            1f / combinedSlipExponent);

        float scale = combined > 1f ? 1f / combined : 1f;
        float longitudinalShape = Mathf.Tanh(Mathf.Abs(sx) * 1.45f) * Mathf.Sign(sx);
        float lateralShape = Mathf.Tanh(Mathf.Abs(sy) * 1.45f) * Mathf.Sign(sy);

        float fx = maxForce * longitudinalShape * scale;
        float fy = maxForce * lateralShape * scale;

        fx -= rollingResistance * normalLoad * Mathf.Sign(forwardSpeed);

        LongitudinalForce = fx;
        LateralForce = fy;

        float brakeSign = Mathf.Abs(WheelAngularSpeedRadPerSec) > 0.5f
            ? Mathf.Sign(WheelAngularSpeedRadPerSec)
            : Mathf.Sign(forwardSpeed);

        float reactionTorque = fx * radius;
        float netTorque = driveTorque - brakeTorque * brakeSign - reactionTorque;
        WheelAngularSpeedRadPerSec += (netTorque / wheelInertia) * dt;

        if (forwardSpeed > 1f && WheelAngularSpeedRadPerSec < 0f)
            WheelAngularSpeedRadPerSec = 0f;

        return new Vector3(fy, 0f, fx);
    }

    public void SetWheelSpeedFromVehicle(float forwardSpeed)
    {
        WheelAngularSpeedRadPerSec = forwardSpeed / Mathf.Max(radius, 0.01f);
    }
}
