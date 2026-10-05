using UnityEngine;
namespace F12026 {
 public class F12026_Suspension:MonoBehaviour {
  public WheelCollider[] wheels=new WheelCollider[4];public float antiRoll=9000,frontSpring=45000,rearSpring=50000,damper=6500;
  void Awake(){for(int i=0;i<wheels.Length;i++){if(!wheels[i])continue;var s=wheels[i].suspensionSpring;s.spring=i<2?frontSpring:rearSpring;s.damper=damper;s.targetPosition=.5f;wheels[i].suspensionSpring=s;}}
  void FixedUpdate(){Apply(0,1);Apply(2,3);}
  void Apply(int l,int r){if(!wheels[l]||!wheels[r])return;float a=Travel(wheels[l]),b=Travel(wheels[r]),f=(a-b)*antiRoll;if(wheels[l].GetGroundHit(out var hl))wheels[l].attachedRigidbody.AddForceAtPosition(wheels[l].transform.up*-f,hl.point);if(wheels[r].GetGroundHit(out var hr))wheels[r].attachedRigidbody.AddForceAtPosition(wheels[r].transform.up*f,hr.point);}
  static float Travel(WheelCollider w){if(!w.GetGroundHit(out var h))return 1;Vector3 p=w.transform.InverseTransformPoint(h.point);return Mathf.Clamp01((-p.y-w.radius)/w.suspensionDistance);}
 }
}