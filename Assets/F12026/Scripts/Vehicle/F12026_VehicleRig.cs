using UnityEngine;
namespace F12026 {
 public class F12026_VehicleRig : MonoBehaviour {
  public WheelCollider frontLeft,frontRight,rearLeft,rearRight;
  public Transform frontLeftMesh,frontRightMesh,rearLeftMesh,rearRightMesh;
  [Range(0f,45f)] public float maxSteerAngle=28f;
  public void ApplySteering(float s){float a=s*maxSteerAngle;if(frontLeft)frontLeft.steerAngle=a;if(frontRight)frontRight.steerAngle=a;}
  public void ApplyDrive(float t){if(rearLeft)rearLeft.motorTorque=t;if(rearRight)rearRight.motorTorque=t;}
  public void ApplyBrake(float t){if(frontLeft)frontLeft.brakeTorque=t;if(frontRight)frontRight.brakeTorque=t;if(rearLeft)rearLeft.brakeTorque=t;if(rearRight)rearRight.brakeTorque=t;}
  void FixedUpdate(){Sync(frontLeft,frontLeftMesh);Sync(frontRight,frontRightMesh);Sync(rearLeft,rearLeftMesh);Sync(rearRight,rearRightMesh);}
  static void Sync(WheelCollider c,Transform m){if(!c||!m)return;c.GetWorldPose(out var p,out var r);m.SetPositionAndRotation(p,r);}
 }
}