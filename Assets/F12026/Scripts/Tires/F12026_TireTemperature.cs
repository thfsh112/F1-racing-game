using UnityEngine;

[System.Serializable]
public class F12026_TireTemperature
{
    [Range(20f,160f)] public float temperatureC = 75f;
    public float targetOperatingC = 90f;
    public float heatingRate = 18f;
    public float coolingRate = 4f;

    public void UpdateTemperature(float slipEnergy, float dt)
    {
        float target = targetOperatingC + Mathf.Clamp01(slipEnergy) * 35f;
        float rate = target > temperatureC ? heatingRate : coolingRate;
        temperatureC = Mathf.MoveTowards(temperatureC, target, rate * dt);
    }
}