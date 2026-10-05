using UnityEngine;

namespace F12026
{
    public class F12026_AeroModel : MonoBehaviour
    {
        public float downforceCoefficient = 2.4f;
        public float dragCoefficient = 0.95f;
        public float frontalArea = 1.55f;
        public float airDensity = 1.225f;

        [Header("Active Aero")]
        [Range(0f, 1f)] public float frontAero = 1f;
        [Range(0f, 1f)] public float rearAero = 1f;
        public bool lowDragMode;

        private Rigidbody rb;

        private void Awake() => rb = GetComponent<Rigidbody>();

        private void FixedUpdate()
        {
            if (!rb) return;

            float speed = rb.linearVelocity.magnitude;
            float q = 0.5f * airDensity * speed * speed;

            float downforce = q * downforceCoefficient * Mathf.Lerp(0.75f, 1.1f, (frontAero + rearAero) * 0.5f);
            float dragCd = lowDragMode ? dragCoefficient * 0.62f : dragCoefficient;
            float drag = q * dragCd * frontalArea;

            rb.AddForce(-transform.up * downforce);
            if (speed > 0.01f)
                rb.AddForce(-rb.linearVelocity.normalized * drag);
        }

        public void SetLowDrag(bool enabled) => lowDragMode = enabled;
    }
}
