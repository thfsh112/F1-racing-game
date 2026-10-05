using UnityEngine;

namespace F12026
{
    public class F12026_VehicleRig : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        public WheelCollider frontLeft;
        public WheelCollider frontRight;
        public WheelCollider rearLeft;
        public WheelCollider rearRight;

        [Header("Wheel Meshes")]
        public Transform frontLeftMesh;
        public Transform frontRightMesh;
        public Transform rearLeftMesh;
        public Transform rearRightMesh;

        [Header("Steering")]
        [Range(0f, 45f)] public float maxSteerAngle = 28f;

        [Header("Geometry")]
        public float wheelbase = 3.6f;
        public float trackWidth = 1.55f;

        public void ApplySteering(float steering)
        {
            float angle = steering * maxSteerAngle;
            if (frontLeft) frontLeft.steerAngle = angle;
            if (frontRight) frontRight.steerAngle = angle;
        }

        public void ApplyDrive(float torque)
        {
            if (rearLeft) rearLeft.motorTorque = torque;
            if (rearRight) rearRight.motorTorque = torque;
        }

        public void ApplyBrake(float torque)
        {
            if (frontLeft) frontLeft.brakeTorque = torque;
            if (frontRight) frontRight.brakeTorque = torque;
            if (rearLeft) rearLeft.brakeTorque = torque;
            if (rearRight) rearRight.brakeTorque = torque;
        }

        private void FixedUpdate()
        {
            SyncWheel(frontLeft, frontLeftMesh);
            SyncWheel(frontRight, frontRightMesh);
            SyncWheel(rearLeft, rearLeftMesh);
            SyncWheel(rearRight, rearRightMesh);
        }

        private static void SyncWheel(WheelCollider collider, Transform mesh)
        {
            if (!collider || !mesh) return;
            collider.GetWorldPose(out var position, out var rotation);
            mesh.SetPositionAndRotation(position, rotation);
        }
    }
}
