using UnityEngine;

namespace F12026
{
    public class F12026_TireWear : MonoBehaviour
    {
        [Range(0f, 1f)] public float wear;
        public float baseWearRate = 0.000015f;

        public float GripMultiplier => Mathf.Lerp(1f, 0.72f, wear);

        public void Simulate(float load, float slip, float temperature, float dt)
        {
            float temperaturePenalty = Mathf.Max(0f, temperature - 105f) / 80f;
            wear = Mathf.Clamp01(wear + baseWearRate * (1f + load * 0.0005f) *
                (1f + Mathf.Abs(slip)) * (1f + temperaturePenalty) * dt);
        }
    }
}
