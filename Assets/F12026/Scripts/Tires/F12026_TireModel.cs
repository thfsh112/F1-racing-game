using UnityEngine;

namespace F12026
{
    public class F12026_TireModel : MonoBehaviour
    {
        public enum Compound { Soft, Medium, Hard, Intermediate, Wet }
        public Compound compound = Compound.Medium;

        [Header("Grip")]
        public float peakGrip = 1.55f;
        public float longitudinalStiffness = 10f;
        public float lateralStiffness = 12f;

        [Header("Slip")]
        public float slipAngle;
        public float slipRatio;
        public float normalizedGrip = 1f;

        public float EvaluateGrip(float speedKph, float temperatureC)
        {
            float tempFactor = Mathf.Clamp01(1f - Mathf.Abs(temperatureC - 92f) / 65f);
            float speedFactor = Mathf.Clamp01(0.72f + speedKph / 400f);
            normalizedGrip = Mathf.Clamp01(tempFactor * speedFactor);
            return peakGrip * normalizedGrip;
        }

        public float EvaluateLateralForce(float slip)
        {
            slipAngle = slip;
            return Mathf.Clamp(slip * lateralStiffness, -peakGrip, peakGrip) * normalizedGrip;
        }

        public float EvaluateLongitudinalForce(float slip)
        {
            slipRatio = slip;
            return Mathf.Clamp(slip * longitudinalStiffness, -peakGrip, peakGrip) * normalizedGrip;
        }
    }
}
