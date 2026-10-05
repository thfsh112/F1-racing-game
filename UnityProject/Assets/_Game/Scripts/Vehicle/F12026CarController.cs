using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class F12026CarController : MonoBehaviour
{
    [Header("2026 Vehicle")]
    [Min(100f)] public float vehicleMassKg = 768f;
    [Min(0.1f)] public float wheelbase = 3.40f;
    [Min(0.1f)] public float frontTrack = 1.62f;
    [Min(0.1f)] public float rearTrack = 1.58f;
    [Range(0.2f, 0.8f)] public float staticFrontWeight = 0.46f;
    [Min(0.05f)] public float centreOfMassHeight = 0.30f;

    [Header("2026 Systems")]\n    public F12026PowerUnit powerUnit;\n    public F12026Gearbox gearbox;\n    public F12026BrakeSystem brakeSystem;\n    public F12026Differential differential;\n\n    [Header("Power Unit")]
    [Min(0f)] public float icePowerKw = 400f;
    [Min(0f)] public float mgukNormalPowerKw = 350f;
    [Min(0f)] public float mgukOtherLapPowerKw = 250f;

    [Header("Controls")]
    [Range(-1f, 1f)] public float steerInput;
    [Range(0f, 1f)] public float throttleInput;
    [Range(0f, 1f)] public float brakeInput;

    [Header("Active Aero X / Z")]
    public bool activeAeroXMode;
    [Min(0f)] public float zDownforceAt70ms = 17000f;
    [Min(0f)] public float xDownforceAt70ms = 12000f;
    [Min(0f)] public float zDragAt70ms = 5200f;
    [Min(0f)] public float xDragAt70ms = 3200f;
    [Range(0f, 1f)] public float aeroFrontDistribution = 0.46f;

    [Header("Steering")]
    [Min(0.1f)] public float maxSteerDegrees = 18f;
    [Min(0.1f)] public float steeringResponse = 8f;

    [Header("Brakes")]
    [Min(0f)] public float maxBrakeTorqueNm = 4200f;
    [Range(0f, 1f)] public float frontBrakeBias = 0.56f;

    [Header("Tyres")]
    public F12026Tire frontLeftTire;
    public F12026Tire frontRightTire;
    public F12026Tire rearLeftTire;
    public F12026Tire rearRightTire;

    [Header("Suspension")]
    public F12026Suspension frontLeftSuspension;
    public F12026Suspension frontRightSuspension;
    public F12026Suspension rearLeftSuspension;
    public F12026Suspension rearRightSuspension;

    [Header("Vertical Load")]
    public F12026VerticalLoad verticalLoad;

    public float SpeedKph { get; private set; }
    public float LongitudinalAcceleration { get; private set; }
    public float LateralAcceleration { get; private set; }
    public float TotalVerticalLoadN =>
        verticalLoad != null ? verticalLoad.TotalLoadN : 0f;

    private Rigidbody rb;
    private float filteredSteer;
    private Vector3 previousLocalVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = vehicleMassKg;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.centerOfMass = new Vector3(0f, -centreOfMassHeight, 0f);

        if (verticalLoad == null)
            verticalLoad = GetComponent<F12026VerticalLoad>();

        InitializeSuspension(frontLeftSuspension);
        InitializeSuspension(frontRightSuspension);
        InitializeSuspension(rearLeftSuspension);
        InitializeSuspension(rearRightSuspension);
    }

    private void FixedUpdate()
    {
        ReadInput();

        float dt = Time.fixedDeltaTime;
        Vector3 velocity = rb.linearVelocity;
        Vector3 localVelocity =
            transform.InverseTransformDirection(velocity);

        SpeedKph = velocity.magnitude * 3.6f;

        Vector3 localAcceleration =
            (localVelocity - previousLocalVelocity) /
            Mathf.Max(dt, 0.0001f);

        LongitudinalAcceleration = localAcceleration.z;
        LateralAcceleration = localAcceleration.x;
        previousLocalVelocity = localVelocity;

        ApplyAero(velocity);

        SimulateCorner(frontLeftSuspension, frontLeftTire, true);
        SimulateCorner(frontRightSuspension, frontRightTire, true);
        SimulateCorner(rearLeftSuspension, rearLeftTire, false);
        SimulateCorner(rearRightSuspension, rearRightTire, false);

        if (verticalLoad != null)
        {
            verticalLoad.Calculate(
                rb,
                frontLeftSuspension != null ? frontLeftSuspension.SuspensionForce : 0f,
                frontRightSuspension != null ? frontRightSuspension.SuspensionForce : 0f,
                rearLeftSuspension != null ? rearLeftSuspension.SuspensionForce : 0f,
                rearRightSuspension != null ? rearRightSuspension.SuspensionForce : 0f,
                LongitudinalAcceleration,
                LateralAcceleration,
                velocity.magnitude);
        }
    }

    private void InitializeSuspension(F12026Suspension suspension)
    {
        if (suspension != null)
            suspension.Initialize(rb);
    }

    private void SimulateCorner(
        F12026Suspension suspension,
        F12026Tire tire,
        bool front)
    {
        if (suspension == null || tire == null)
            return;

        suspension.Simulate(Time.fixedDeltaTime);

        if (!suspension.Grounded)
            return;

        Transform anchor =
            suspension.suspensionAnchor != null
                ? suspension.suspensionAnchor
                : transform;

        Vector3 wheelForward = anchor.forward;

        if (front)
        {
            float targetSteer = steerInput * maxSteerDegrees;

            filteredSteer = Mathf.Lerp(
                filteredSteer,
                targetSteer,
                1f - Mathf.Exp(
                    -steeringResponse * Time.fixedDeltaTime));

            wheelForward =
                Quaternion.AngleAxis(
                    filteredSteer,
                    suspension.ContactNormal) *
                wheelForward;
        }

        wheelForward = Vector3.ProjectOnPlane(
            wheelForward,
            suspension.ContactNormal).normalized;

        Vector3 wheelRight = Vector3.Cross(
            suspension.ContactNormal,
            wheelForward).normalized;

        Vector3 pointVelocity =
            rb.GetPointVelocity(suspension.ContactPoint);

        Vector3 wheelVelocity = new Vector3(
            Vector3.Dot(pointVelocity, wheelRight),
            0f,
            Vector3.Dot(pointVelocity, wheelForward));

        float normalLoad =
            Mathf.Max(0f, suspension.SuspensionForce);

        float driveTorque = 0f;

        if (!front)
        {
            float combinedPowerKw =
                Mathf.Max(0f, icePowerKw + mgukNormalPowerKw);

            float speedTerm =
                Mathf.Max(1f, SpeedKph / 3.6f + 5f);

            float estimatedTorque =
                combinedPowerKw * 1000f / speedTerm;

            driveTorque =
                estimatedTorque * throttleInput * 0.5f;
        }

        float brakeTorque =
            maxBrakeTorqueNm *
            brakeInput *
            (front ? frontBrakeBias : 1f - frontBrakeBias);

        Vector3 tyreForceLocal =
            tire.Simulate(
                wheelVelocity,
                normalLoad,
                driveTorque,
                brakeTorque,
                filteredSteer * Mathf.Deg2Rad,
                Time.fixedDeltaTime,
                true);

        Vector3 tyreForceWorld =
            wheelRight * tyreForceLocal.x +
            wheelForward * tyreForceLocal.z;

        rb.AddForceAtPosition(
            tyreForceWorld,
            suspension.ContactPoint,
            ForceMode.Force);
    }

    private void ApplyAero(Vector3 velocity)
    {
        float speed = velocity.magnitude;

        if (speed < 1f)
            return;

        bool x = activeAeroXMode;

        float downforceRef =
            x ? xDownforceAt70ms : zDownforceAt70ms;

        float dragRef =
            x ? xDragAt70ms : zDragAt70ms;

        float ratio = speed / 70f;
        float downforce = downforceRef * ratio * ratio;
        float drag = dragRef * ratio * ratio;

        Vector3 down = -transform.up * downforce;
        Vector3 dragForce = -velocity.normalized * drag;

        Vector3 frontPoint =
            transform.TransformPoint(
                new Vector3(
                    0f,
                    0f,
                    wheelbase *
                    (1f - aeroFrontDistribution) *
                    0.5f));

        Vector3 rearPoint =
            transform.TransformPoint(
                new Vector3(
                    0f,
                    0f,
                    -wheelbase *
                    aeroFrontDistribution *
                    0.5f));

        rb.AddForceAtPosition(
            down * aeroFrontDistribution,
            frontPoint,
            ForceMode.Force);

        rb.AddForceAtPosition(
            down * (1f - aeroFrontDistribution),
            rearPoint,
            ForceMode.Force);

        rb.AddForce(dragForce, ForceMode.Force);
    }

    public void SetXMode(bool enabled) => activeAeroXMode = enabled;\n\n    private void ReadInput()
    {
        float steer = Input.GetAxisRaw("Horizontal");
        float throttle = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(steer) > 0.01f)
            steerInput = Mathf.Clamp(steer, -1f, 1f);

        throttleInput =
            Mathf.Clamp01(Mathf.Max(0f, throttle));

        brakeInput =
            Mathf.Clamp01(Mathf.Max(0f, -throttle));
    }
}
