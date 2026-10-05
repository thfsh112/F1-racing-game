using UnityEngine;

public class F12026Gearbox : MonoBehaviour
{
    [Header("8-speed gearbox")]
    public int gear = 1;
    [Min(1)] public int maxGear = 8;
    public float finalDrive = 3.1f;
    public float[] gearRatios = { 3.10f, 2.35f, 1.85f, 1.52f, 1.30f, 1.12f, 0.98f, 0.86f };

    public float CurrentRatio
    {
        get
        {
            int index = Mathf.Clamp(gear - 1, 0, gearRatios.Length - 1);
            return gearRatios[index] * finalDrive;
        }
    }

    public void UpdateAutomatic(float engineRpm)
    {
        if (gear < maxGear && engineRpm > 13800f) gear++;
        else if (gear > 1 && engineRpm < 9000f) gear--;
    }
}
