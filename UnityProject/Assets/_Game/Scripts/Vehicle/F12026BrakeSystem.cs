using UnityEngine;

public class F12026BrakeSystem : MonoBehaviour
{
    [Range(0f,1f)] public float brakeInput;
    [Range(0f,1f)] public float frontBias = 0.56f;
    [Min(0f)] public float maxBrakeTorqueNm = 4200f;

    public float FrontTorque => maxBrakeTorqueNm * brakeInput * frontBias;
    public float RearTorque => maxBrakeTorqueNm * brakeInput * (1f - frontBias);
}
