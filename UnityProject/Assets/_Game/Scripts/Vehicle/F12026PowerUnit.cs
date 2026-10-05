using UnityEngine;

public class F12026PowerUnit : MonoBehaviour
{
    [Header("2026 configurable baseline")]
    [Min(0f)] public float icePowerKw = 400f;
    [Min(0f)] public float mgukNormalPowerKw = 350f;
    [Min(0f)] public float mgukReducedPowerKw = 250f;
    [Min(0f)] public float batteryCapacityMJ = 4f;
    [Min(0f)] public float batteryEnergyMJ = 3f;
    [Min(0f)] public float overtakeExtraEnergyMJ = 0.5f;
    public bool overtakeEnabled;

    public float AvailableElectricPowerKw => overtakeEnabled ? mgukNormalPowerKw : mgukReducedPowerKw;

    public float GetDrivePowerKw(float throttle01)
    {
        float ice = icePowerKw * Mathf.Clamp01(throttle01);
        float electric = AvailableElectricPowerKw * Mathf.Clamp01(throttle01);
        return ice + electric;
    }

    public void ConsumeEnergy(float powerKw, float seconds)
    {
        batteryEnergyMJ = Mathf.Clamp(
            batteryEnergyMJ - powerKw * seconds / 1000f,
            0f, batteryCapacityMJ);
    }

    public void Recharge(float powerKw, float seconds)
    {
        batteryEnergyMJ = Mathf.Clamp(
            batteryEnergyMJ + powerKw * seconds / 1000f,
            0f, batteryCapacityMJ);
    }
}
