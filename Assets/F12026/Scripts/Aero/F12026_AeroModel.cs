using UnityEngine;
namespace F12026 {
 public class F12026_AeroModel:MonoBehaviour {
  public float downforceCoefficient=2.4f,dragCoefficient=.95f,frontalArea=1.55f,airDensity=1.225f; [Range(0,1)]public float frontAero=1,rearAero=1;public bool lowDragMode; Rigidbody rb;
  void Awake(){rb=GetComponent<Rigidbody>();}
  void FixedUpdate(){if(!rb)return;float v=rb.linearVelocity.magnitude,q=.5f*airDensity*v*v;float down=q*downforceCoefficient*Mathf.Lerp(.75f,1.1f,(frontAero+rearAero)*.5f);float drag=q*(lowDragMode?dragCoefficient*.62f:dragCoefficient)*frontalArea;rb.AddForce(-transform.up*down);if(v>.01f)rb.AddForce(-rb.linearVelocity.normalized*drag);}
  public void SetLowDrag(bool enabled){lowDragMode=enabled;}
 }
}