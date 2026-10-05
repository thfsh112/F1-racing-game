using UnityEngine;

public class F12026_Telemetry : MonoBehaviour
{
    public F12026_VehiclePhysics vehicle;

    public float SpeedKmh { get; private set; }
    public float RPM { get; private set; }
    public int Gear { get; private set; }
    public float BatterySoC { get; private set; }

    private void Awake()
    {
        if (!vehicle) vehicle = GetComponent<F12026_VehiclePhysics>();
    }

    private void Update()
    {
        if (!vehicle) return;
        SpeedKmh = vehicle.GetSpeedKmh();
        RPM = vehicle.GetRPM();
        Gear = vehicle.GetGear();
        BatterySoC = vehicle.GetBatterySoC();
    }
}