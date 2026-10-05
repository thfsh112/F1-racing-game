using UnityEngine;

public class F12026_VehicleInput : MonoBehaviour
{
    public F12026_VehiclePhysics vehicle;
    public KeyCode overtakeKey = KeyCode.X;
    public KeyCode xModeKey = KeyCode.Z;

    private void Awake()
    {
        if (!vehicle) vehicle = GetComponent<F12026_VehiclePhysics>();
    }

    private void Update()
    {
        if (!vehicle) return;

        vehicle.SetXMode(Input.GetKey(xModeKey));
        vehicle.SetOverdrive(Input.GetKey(overtakeKey));
    }
}
