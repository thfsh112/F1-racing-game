using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class F12026_VehiclePhysics : MonoBehaviour
{
    [Header("Wheels")]
    public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR;
    public Transform wheelMeshFL, wheelMeshFR, wheelMeshRL, wheelMeshRR;

    [Header("Active Aero")]
    public Transform frontWing, rearWing;
    public float frontWingZAngle = 0f;
    public float frontWingXAngle = -18f;
    public float rearWingZAngle = 0f;
    public float rearWingXAngle = -25f;
    public float aeroLerpSpeed = 5f;

    [Header("Aerodynamics")]
    [Tooltip("Base aerodynamic downforce coefficient.")]
    public float downforceCoefficient = 3f;
    [Tooltip("Base aerodynamic drag coefficient.")]
    public float dragCoefficient = 0.32f;
    public float zModeDownforceMultiplier = 1f;
    public float zModeDragMultiplier = 1f;
    public float xModeDownforceMultiplier = 0.55f;
    public float xModeDragMultiplier = 0.55f;
    public float frontalArea = 1.5f;
    public float airDensity = 1.225f;

    [Header("Power Unit")]
    [Tooltip("ICE maximum horsepower.")]
    public float iceHorsePower = 750f;
    [Tooltip("MGU-K maximum electrical horsepower.")]
    public float mguKHorsePower = 450f;
    public float maxRPM = 15000f;
    public float idleRPM = 2500f;

    [Header("Transmission")]
    public float finalDriveRatio = 3.2f;
    public float[] gearRatios = { 3.20f, 2.40f, 1.90f, 1.55f, 1.30f, 1.10f, 0.95f, 0.82f };
    [SerializeField] private int currentGear = 1;
    public float maximumEngineTorque = 520f;

    [Header("Steering")]
    public float maxSteeringAngle = 23f;
    public float steeringSpeed = 7f;

    [Header("Brakes")]
    public float maximumBrakeTorque = 4500f;
    [Range(0f, 1f)] public float brakeBias = 0.58f;

    [Header("ERS / Battery")]
    [Range(0f, 1f)] public float batterySoC = 1f;
    public float ersConsumptionRate = 0.12f;
    public float regenerationRate = 0.08f;

    [Header("Overdrive")]
    public bool overdriveEnabled = true;
    public float overdrivePowerMultiplier = 1.35f;
    public float overdriveConsumptionRate = 0.22f;

    [Header("Input")]
    [Range(-1f, 1f)] public float steeringInput;
    [Range(0f, 1f)] public float throttleInput;
    [Range(0f, 1f)] public float brakeInput;
    public bool overtakeInput;
    public bool xMode;

    [Header("Telemetry")]
    [SerializeField] private float speedKmh;
    [SerializeField] private float engineRPM;
    [SerializeField] private float totalPower;
    [SerializeField] private float aerodynamicDownforce;
    [SerializeField] private float aerodynamicDrag;

    private Rigidbody rb;
    private Quaternion frontWingInitialRotation;
    private Quaternion rearWingInitialRotation;
    private float currentSteeringAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.35f, 0f);
        if (frontWing) frontWingInitialRotation = frontWing.localRotation;
        if (rearWing) rearWingInitialRotation = rearWing.localRotation;
    }

    private void FixedUpdate()
    {
        ReadInput();
        UpdateSpeed();
        UpdateSteering();
        UpdateTransmission();
        UpdatePowerUnit();
        UpdateBrakes();
        UpdateRegeneration();
        UpdateAerodynamics();
        UpdateActiveAero();
        UpdateWheelVisuals();
    }

    private void ReadInput()
    {
        steeringInput = Input.GetAxis("Horizontal");
        throttleInput = Mathf.Clamp01(Input.GetAxis("Vertical"));
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        overtakeInput = Input.GetKey(KeyCode.X);
    }

    private void UpdateSpeed()
    {
        speedKmh = rb.linearVelocity.magnitude * 3.6f;
    }

    private void UpdateSteering()
    {
        float target = steeringInput * maxSteeringAngle;
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, target, steeringSpeed * Time.fixedDeltaTime);
        if (wheelFL) wheelFL.steerAngle = currentSteeringAngle;
        if (wheelFR) wheelFR.steerAngle = currentSteeringAngle;
    }

    private void UpdateTransmission()
    {
        if (gearRatios == null || gearRatios.Length == 0) return;
        if (engineRPM > 12500f && currentGear < gearRatios.Length) currentGear++;
        if (engineRPM < 5000f && currentGear > 1) currentGear--;
    }

    private void UpdatePowerUnit()
    {
        if (gearRatios == null || gearRatios.Length == 0) return;

        float wheelRPM = 0f;
        int count = 0;
        foreach (var wheel in new[] { wheelRL, wheelRR })
        {
            if (!wheel) continue;
            wheelRPM += Mathf.Abs(wheel.rpm);
            count++;
        }
        if (count > 0) wheelRPM /= count;

        float ratio = gearRatios[Mathf.Clamp(currentGear - 1, 0, gearRatios.Length - 1)];
        engineRPM = Mathf.Clamp(wheelRPM * ratio * finalDriveRatio, idleRPM, maxRPM);

        float icePower = iceHorsePower * throttleInput;
        float electricalPower = 0f;

        if (batterySoC > 0f)
        {
            electricalPower = mguKHorsePower * throttleInput;
            if (overtakeInput && overdriveEnabled)
            {
                electricalPower *= overdrivePowerMultiplier;
                batterySoC -= overdriveConsumptionRate * Time.fixedDeltaTime;
            }
            else
            {
                batterySoC -= ersConsumptionRate * throttleInput * Time.fixedDeltaTime;
            }
        }

        batterySoC = Mathf.Clamp01(batterySoC);
        totalPower = icePower + electricalPower;

        float omega = Mathf.Max(engineRPM * Mathf.PI / 30f, 1f);
        float torque = Mathf.Clamp(totalPower * 745.7f / omega, 0f, maximumEngineTorque);
        if (wheelRL) wheelRL.motorTorque = torque;
        if (wheelRR) wheelRR.motorTorque = torque;
    }

    private void UpdateBrakes()
    {
        float total = brakeInput * maximumBrakeTorque;
        float front = total * brakeBias;
        float rear = total * (1f - brakeBias);
        if (wheelFL) wheelFL.brakeTorque = front;
        if (wheelFR) wheelFR.brakeTorque = front;
        if (wheelRL) wheelRL.brakeTorque = rear;
        if (wheelRR) wheelRR.brakeTorque = rear;
    }

    private void UpdateRegeneration()
    {
        if (brakeInput <= 0f || batterySoC >= 1f) return;
        batterySoC = Mathf.Clamp01(batterySoC + regenerationRate * brakeInput * Time.fixedDeltaTime);
    }

    private void UpdateAerodynamics()
    {
        float v = rb.linearVelocity.magnitude;
        float downMultiplier = xMode ? xModeDownforceMultiplier : zModeDownforceMultiplier;
        float dragMultiplier = xMode ? xModeDragMultiplier : zModeDragMultiplier;

        aerodynamicDownforce = 0.5f * airDensity * frontalArea * v * v * downforceCoefficient * downMultiplier;
        aerodynamicDrag = 0.5f * airDensity * frontalArea * v * v * dragCoefficient * dragMultiplier;

        rb.AddForce(Vector3.down * aerodynamicDownforce);
        if (v > 0.1f) rb.AddForce(-rb.linearVelocity.normalized * aerodynamicDrag);
    }

    private void UpdateActiveAero()
    {
        float frontAngle = xMode ? frontWingXAngle : frontWingZAngle;
        float rearAngle = xMode ? rearWingXAngle : rearWingZAngle;

        if (frontWing)
        {
            var target = frontWingInitialRotation * Quaternion.Euler(frontAngle, 0f, 0f);
            frontWing.localRotation = Quaternion.Slerp(frontWing.localRotation, target, aeroLerpSpeed * Time.fixedDeltaTime);
        }

        if (rearWing)
        {
            var target = rearWingInitialRotation * Quaternion.Euler(rearAngle, 0f, 0f);
            rearWing.localRotation = Quaternion.Slerp(rearWing.localRotation, target, aeroLerpSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateWheelVisuals()
    {
        UpdateWheel(wheelFL, wheelMeshFL);
        UpdateWheel(wheelFR, wheelMeshFR);
        UpdateWheel(wheelRL, wheelMeshRL);
        UpdateWheel(wheelRR, wheelMeshRR);
    }

    private static void UpdateWheel(WheelCollider collider, Transform visual)
    {
        if (!collider || !visual) return;
        collider.GetWorldPose(out var position, out var rotation);
        visual.SetPositionAndRotation(position, rotation);
    }

    public void SetXMode(bool enabled) => xMode = enabled;
    public void SetOverdrive(bool enabled) => overtakeInput = enabled;
    public float GetSpeedKmh() => speedKmh;
    public float GetRPM() => engineRPM;
    public int GetGear() => currentGear;
    public float GetBatterySoC() => batterySoC;
}
