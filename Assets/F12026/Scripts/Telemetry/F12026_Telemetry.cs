using UnityEngine;
namespace F12026 {
 public class F12026_Telemetry:MonoBehaviour {
  public float speedKph,rpm;public int gear=1;[Range(0,1)]public float stateOfCharge;public bool overdrive,activeAero;Rigidbody rb;F12026_PowerUnit power;F12026_AeroModel aero;F12026_Transmission transmission;
  void Awake(){rb=GetComponent<Rigidbody>();power=GetComponent<F12026_PowerUnit>();aero=GetComponent<F12026_AeroModel>();transmission=GetComponent<F12026_Transmission>();}
  void Update(){if(rb)speedKph=rb.linearVelocity.magnitude*3.6f;if(power){rpm=power.rpm;stateOfCharge=power.stateOfCharge;overdrive=power.overdrive;}if(aero)activeAero=!aero.lowDragMode;if(transmission)gear=transmission.Gear;}
 }
}