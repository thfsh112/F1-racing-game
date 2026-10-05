using UnityEngine;

namespace F12026
{
    public class F12026_Telemetry : MonoBehaviour
    {
        public float speedKph;
        public float rpm;
        public int gear = 1;
        [Range(0f, 1f)] public float stateOfCharge;
        public bool overdrive;
        public bool activeAero;

        private Rigidbody rb;
        private F12026_PowerUnit power;
        private F12026_AeroModel aero;
        private F12026_Transmission transmission;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            power = GetComponent<F12026_PowerUnit>();
            aero = GetComponent<F12026_AeroModel>();
            transmission = GetComponent<F12026_Transmission>();
        }

        private void Update()
        {
            if (rb) speedKph = rb.linearVelocity.magnitude * 3.6f;
            if (power)
            {
                rpm = power.rpm;
                stateOfCharge = power.stateOfCharge;
                overdrive = power.overdrive;
            }
            if (aero) activeAero = !aero.lowDragMode;
            if (transmission) gear = transmission.Gear;
        }
    }
}
