using UnityEngine;

namespace F12026
{
    public class F12026_Suspension : MonoBehaviour
    {
        public WheelCollider[] wheels = new WheelCollider[4];
        public float antiRoll = 9000f;
        public float frontSpring = 45000f;
        public float rearSpring = 50000f;
        public float damper = 6500f;

        private void Awake()
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                if (!wheels[i]) continue;
                var spring = wheels[i].suspensionSpring;
                spring.spring = i < 2 ? frontSpring : rearSpring;
                spring.damper = damper;
                spring.targetPosition = 0.5f;
                wheels[i].suspensionSpring = spring;
            }
        }

        private void FixedUpdate()
        {
            ApplyAntiRoll(0, 1);
            ApplyAntiRoll(2, 3);
        }

        private void ApplyAntiRoll(int left, int right)
        {
            if (!wheels[left] || !wheels[right]) return;

            float travelL = GetTravel(wheels[left]);
            float travelR = GetTravel(wheels[right]);
            float force = (travelL - travelR) * antiRoll;

            if (wheels[left].GetGroundHit(out var hitL))
                wheels[left].attachedRigidbody.AddForceAtPosition(
                    wheels[left].transform.up * -force, hitL.point);

            if (wheels[right].GetGroundHit(out var hitR))
                wheels[right].attachedRigidbody.AddForceAtPosition(
                    wheels[right].transform.up * force, hitR.point);
        }

        private static float GetTravel(WheelCollider wheel)
        {
            if (!wheel.GetGroundHit(out var hit)) return 1f;
            Vector3 local = wheel.transform.InverseTransformPoint(hit.point);
            return Mathf.Clamp01((-local.y - wheel.radius) / wheel.suspensionDistance);
        }
    }
}
