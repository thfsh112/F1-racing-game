using UnityEngine;

[System.Serializable]
public class F12026_TireModel
{
    [Range(0.5f, 2f)] public float peakGrip = 1.45f;
    [Range(0.01f, 1f)] public float peakSlip = 0.12f;
    [Range(0.1f, 2f)] public float combinedGrip = 1f;

    public float CalculateGrip(float slipRatio, float slipAngle)
    {
        float longitudinal = 1f - Mathf.Exp(-Mathf.Abs(slipRatio) / Mathf.Max(peakSlip, 0.001f));
        float lateral = 1f - Mathf.Exp(-Mathf.Abs(slipAngle) / Mathf.Max(peakSlip, 0.001f));
        float combined = Mathf.Clamp01(Mathf.Sqrt(longitudinal * longitudinal + lateral * lateral));
        return peakGrip * combinedGrip * Mathf.Clamp01(combined);
    }
}