using UnityEngine;

public class F12026Differential : MonoBehaviour
{
    [Range(0f,1f)] public float preload = 0.15f;
    [Range(0f,1f)] public float powerLock = 0.35f;
    [Range(0f,1f)] public float coastLock = 0.25f;

    public float LimitTorqueDifference(float leftTorque, float rightTorque, float throttle01)
    {
        float lock = Mathf.Lerp(coastLock, powerLock, Mathf.Clamp01(throttle01));
        float average = (leftTorque + rightTorque) * 0.5f;
        float maxDifference = Mathf.Max(0f, Mathf.Abs(average) * (1f - lock) + preload * 1000f);
        float difference = Mathf.Clamp(leftTorque - rightTorque, -maxDifference, maxDifference);
        return average + difference * 0.5f;
    }
}
