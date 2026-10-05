using UnityEngine;

[System.Serializable]
public class F12026_TireTemperature
{
    [Range(40f, 140f)] public float temperatureC = 75f;
    public float ambientC = 25f;
    public float targetOperatingC = 90f;
    public float heatingRate = 18f;
    public float coolingRate = 4f;

    public void Update(float slipEnergy, float dt)
    {
        float target = targetOperatingC + slipEnergy * heatingRate;
        float rate = target > temperatureC ? heatingRate : coolingRate;
        temperatureC = Mathf.MoveTowards(temperatureC, target, rate * dt);
        temperatureC = Mathf.Clamp(temperatureC, 20f, 160f);
    }
}