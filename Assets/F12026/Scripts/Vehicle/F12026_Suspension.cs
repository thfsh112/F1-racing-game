using UnityEngine;

public class F12026_Suspension : MonoBehaviour
{
    public WheelCollider frontLeft, frontRight, rearLeft, rearRight;
    public float spring = 35000f;
    public float damper = 5000f;
    public float antiRoll = 7000f;

    private void FixedUpdate()
    {
        Configure(frontLeft); Configure(frontRight);
        Configure(rearLeft); Configure(rearRight);
        AntiRoll(frontLeft, frontRight);
        AntiRoll(rearLeft, rearRight);
    }

    private void Configure(WheelCollider wheel)
    {
        if (!wheel) return;
        var s = wheel.suspensionSpring;
        s.spring = spring;
        s.damper = damper;
        wheel.suspensionSpring = s;
    }

    private void AntiRoll(WheelCollider left, WheelCollider right)
    {
        if (!left || !right) return;
        float travelL = Travel(left);
        float travelR = Travel(right);
        float force = (travelL - travelR) * antiRoll;

        if (left.GetGroundHit(out var hitL) && left.attachedRigidbody)
            left.attachedRigidbody.AddForceAtPosition(left.transform.up * -force, hitL.point);

        if (right.GetGroundHit(out var hitR) && right.attachedRigidbody)
            right.attachedRigidbody.AddForceAtPosition(right.transform.up * force, hitR.point);
    }

    private static float Travel(WheelCollider wheel)
    {
        if (!wheel.GetGroundHit(out var hit)) return 1f;
        Vector3 local = wheel.transform.InverseTransformPoint(hit.point);
        return Mathf.Clamp01((-local.y - wheel.radius) /
                             Mathf.Max(wheel.suspensionDistance, 0.001f));
    }
}