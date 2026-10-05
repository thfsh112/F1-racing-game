using UnityEngine;

public class F12026CarVisual : MonoBehaviour
{
    [Header("Generated 2026-style car visual")]
    public Transform body;
    public Transform frontWing;
    public Transform rearWing;
    public Transform steeringWheel;
    public Transform frontLeftWheel;
    public Transform frontRightWheel;

    [Range(-20f, 20f)] public float xModeWingAngle = -8f;
    [Range(-20f, 20f)] public float zModeWingAngle = 12f;
    public float steeringWheelAngle = 180f;
    public float wheelSteerAngle = 22f;

    private F12026CarController car;

    private void Awake()
    {
        car = GetComponentInParent<F12026CarController>();
    }

    private void LateUpdate()
    {
        if (car == null) return;

        float steer = Mathf.Clamp(car.steerInput, -1f, 1f);

        if (steeringWheel != null)
        {
            steeringWheel.localRotation = Quaternion.Slerp(
                steeringWheel.localRotation,
                Quaternion.Euler(0f, 0f, -steer * steeringWheelAngle),
                Time.deltaTime * 10f);
        }

        if (frontLeftWheel != null)
        {
            frontLeftWheel.localRotation = Quaternion.Slerp(
                frontLeftWheel.localRotation,
                Quaternion.Euler(0f, -steer * wheelSteerAngle, 90f),
                Time.deltaTime * 12f);
        }

        if (frontRightWheel != null)
        {
            frontRightWheel.localRotation = Quaternion.Slerp(
                frontRightWheel.localRotation,
                Quaternion.Euler(0f, -steer * wheelSteerAngle, 90f),
                Time.deltaTime * 12f);
        }

        float target = car.activeAeroXMode ? xModeWingAngle : zModeWingAngle;

        if (frontWing != null)
        {
            frontWing.localRotation = Quaternion.Slerp(
                frontWing.localRotation,
                Quaternion.Euler(target, 0f, 0f),
                Time.deltaTime * 8f);
        }

        if (rearWing != null)
        {
            rearWing.localRotation = Quaternion.Slerp(
                rearWing.localRotation,
                Quaternion.Euler(target, 0f, 0f),
                Time.deltaTime * 8f);
        }
    }
}
