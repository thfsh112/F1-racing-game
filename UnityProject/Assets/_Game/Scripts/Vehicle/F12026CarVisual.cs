using UnityEngine;

public class F12026CarVisual : MonoBehaviour
{
    [Header("Original 2026-style single-seater visual")]
    public Transform body;
    public Transform frontWing;
    public Transform rearWing;
    public Transform steeringWheel;

    [Range(-1f,1f)] public float xModeWingAngle = -8f;
    [Range(-1f,1f)] public float zModeWingAngle = 12f;

    private F12026CarController car;

    private void Awake()
    {
        car = GetComponentInParent<F12026CarController>();
    }

    private void LateUpdate()
    {
        if (car == null) return;

        float steer = car.steerInput;
        if (steeringWheel != null)
            steeringWheel.localRotation = Quaternion.Euler(0f, 0f, -steer * 180f);

        float target = car.activeAeroXMode ? xModeWingAngle : zModeWingAngle;

        if (frontWing != null)
            frontWing.localRotation = Quaternion.Lerp(
                frontWing.localRotation,
                Quaternion.Euler(target,0f,0f), Time.deltaTime * 8f);

        if (rearWing != null)
            rearWing.localRotation = Quaternion.Lerp(
                rearWing.localRotation,
                Quaternion.Euler(target,0f,0f), Time.deltaTime * 8f);
    }
}
