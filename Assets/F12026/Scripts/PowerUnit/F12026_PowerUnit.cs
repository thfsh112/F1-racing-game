using UnityEngine;

public class F12026_PowerUnit : MonoBehaviour
{
    public float icePowerHP = 750f;
    public float mguKPowerHP = 450f;
    [Range(0f,1f)] public float stateOfCharge = 1f;
    public float deployRate = 0.12f;
    public float regenRate = 0.08f;
    public bool overdrive;

    public float ElectricalPowerHP { get; private set; }

    public float CalculatePower(float throttle)
    {
        throttle = Mathf.Clamp01(throttle);
        float electrical = stateOfCharge > 0f ? mguKPowerHP * throttle : 0f;
        if (overdrive) electrical *= 1.25f;

        ElectricalPowerHP = electrical;
        stateOfCharge = Mathf.Clamp01(
            stateOfCharge - deployRate * throttle * Time.fixedDeltaTime);

        return icePowerHP * throttle + electrical;
    }

    public void Regenerate(float brake)
    {
        stateOfCharge = Mathf.Clamp01(
            stateOfCharge + regenRate * Mathf.Clamp01(brake) * Time.fixedDeltaTime);
    }
}