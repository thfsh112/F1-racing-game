using UnityEngine;

namespace F12026
{
    public class F12026_PowerUnit : MonoBehaviour
    {
        [Header("ICE")]
        public float maxIcePowerKw = 560f;
        public float rpm = 4000f;
        public float idleRpm = 3500f;
        public float maxRpm = 15000f;

        [Header("MGU-K / Battery")]
        public float maxMguKPowerKw = 120f;
        [Range(0f, 1f)] public float stateOfCharge = 0.82f;
        public float deployRate = 0.12f;
        public float regenRate = 0.08f;

        [Header("Overdrive")]
        public bool overdrive;
        public float overdriveMultiplier = 1.08f;

        public float AvailablePowerKw
        {
            get
            {
                float ice = maxIcePowerKw;
                float electric = stateOfCharge > 0.02f ? maxMguKPowerKw : 0f;
                float total = ice + electric;
                return overdrive ? total * overdriveMultiplier : total;
            }
        }

        public void Simulate(float throttle, float brake, float dt)
        {
            float targetRpm = Mathf.Lerp(idleRpm, maxRpm, Mathf.Clamp01(throttle));
            rpm = Mathf.Lerp(rpm, targetRpm, dt * 5f);

            if (overdrive && throttle > 0.8f)
                stateOfCharge = Mathf.Clamp01(stateOfCharge - deployRate * dt);
            else if (brake > 0.1f)
                stateOfCharge = Mathf.Clamp01(stateOfCharge + regenRate * brake * dt);
        }
    }
}
