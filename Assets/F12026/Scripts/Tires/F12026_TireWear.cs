using UnityEngine;

[System.Serializable]
public class F12026_TireWear
{
    [Range(0f, 1f)] public float remaining = 1f;
    public float wearRate = 0.002f;

    public void Update(float loadFactor, float slipEnergy, float dt)
    {
        float wear = wearRate * Mathf.Max(0f, loadFactor) * (1f + slipEnergy * 2f) * dt;
        remaining = Mathf.Clamp01(remaining - wear);
    }

    public float GripMultiplier => Mathf.Lerp(0.72f, 1f, remaining);
}