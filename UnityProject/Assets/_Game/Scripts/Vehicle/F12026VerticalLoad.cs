using UnityEngine;

public class F12026VerticalLoad : MonoBehaviour
{
    [Header("Vehicle Geometry")]
    [Min(0.1f)] public float wheelbase = 3.40f;
    [Min(0.1f)] public float frontTrack = 1.62f;
    [Min(0.1f)] public float rearTrack = 1.58f;
    [Range(0.20f, 0.80f)] public float staticFrontWeight = 0.46f;
    [Min(0.05f)] public float centreOfMassHeight = 0.30f;

    [Header("Aerodynamic Load")]
    [Min(0f)] public float aeroReferenceSpeed = 70f;
    [Min(0f)] public float downforceAtReferenceSpeed = 17000f;
    [Range(0f, 1f)] public float aeroFrontDistribution = 0.46f;

    public float FrontLeftN { get; private set; }
    public float FrontRightN { get; private set; }
    public float RearLeftN { get; private set; }
    public float RearRightN { get; private set; }
    public float TotalLoadN { get; private set; }
    public float TheoreticalTotalN { get; private set; }

    public void Calculate(
        Rigidbody body,
        float actualFrontLeft,
        float actualFrontRight,
        float actualRearLeft,
        float actualRearRight,
        float longitudinalAcceleration,
        float lateralAcceleration,
        float speed)
    {
        float mass = body.mass;
        float g = Mathf.Abs(Physics.gravity.y);
        float staticLoad = mass * g;

        float aero = downforceAtReferenceSpeed *
                     Mathf.Pow(
                         Mathf.Abs(speed) / Mathf.Max(aeroReferenceSpeed, 1f),
                         2f);

        float longitudinalTransfer =
            mass * longitudinalAcceleration * centreOfMassHeight /
            Mathf.Max(wheelbase, 0.1f);

        float lateralTransferFront =
            mass * lateralAcceleration * centreOfMassHeight /
            Mathf.Max(frontTrack, 0.1f);

        float lateralTransferRear =
            mass * lateralAcceleration * centreOfMassHeight /
            Mathf.Max(rearTrack, 0.1f);

        float frontBase = staticLoad * staticFrontWeight;
        float rearBase = staticLoad - frontBase;

        float frontAero = aero * aeroFrontDistribution;
        float rearAero = aero - frontAero;

        float frontAxle = frontBase - longitudinalTransfer + frontAero;
        float rearAxle = rearBase + longitudinalTransfer + rearAero;

        float frontLeftTheory =
            frontAxle * 0.5f - lateralTransferFront * 0.5f;

        float frontRightTheory =
            frontAxle * 0.5f + lateralTransferFront * 0.5f;

        float rearLeftTheory =
            rearAxle * 0.5f - lateralTransferRear * 0.5f;

        float rearRightTheory =
            rearAxle * 0.5f + lateralTransferRear * 0.5f;

        FrontLeftN = Mathf.Max(0f, actualFrontLeft);
        FrontRightN = Mathf.Max(0f, actualFrontRight);
        RearLeftN = Mathf.Max(0f, actualRearLeft);
        RearRightN = Mathf.Max(0f, actualRearRight);

        TotalLoadN =
            FrontLeftN + FrontRightN + RearLeftN + RearRightN;

        TheoreticalTotalN =
            Mathf.Max(
                0f,
                frontLeftTheory +
                frontRightTheory +
                rearLeftTheory +
                rearRightTheory);
    }

    public float GetLoad(int wheelIndex)
    {
        return wheelIndex switch
        {
            0 => FrontLeftN,
            1 => FrontRightN,
            2 => RearLeftN,
            _ => RearRightN
        };
    }
}
