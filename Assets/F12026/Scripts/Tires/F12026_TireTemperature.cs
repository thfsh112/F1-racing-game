using UnityEngine;

namespace F12026
{
    public class F12026_TireTemperature : MonoBehaviour
    {
        public float temperatureC = 82f;
        public float coreTemperatureC = 82f;
        public float ambientTemperatureC = 25f;
        public float heatingRate = 0.35f;
        public float coolingRate = 0.08f;

        public void Simulate(float slipEnergy, float brakeEnergy, float dt)
        {
            float heating = (Mathf.Abs(slipEnergy) + Mathf.Abs(brakeEnergy)) * heatingRate;
            float cooling = (temperatureC - ambientTemperatureC) * coolingRate;
            temperatureC = Mathf.Clamp(temperatureC + (heating - cooling) * dt, 20f, 160f);
            coreTemperatureC = Mathf.Lerp(coreTemperatureC, temperatureC, dt * 0.4f);
        }
    }
}
