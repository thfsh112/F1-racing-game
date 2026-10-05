using UnityEngine;
namespace F12026 {
 public class F12026_RuntimeBootstrap:MonoBehaviour {
  public F12026_VehicleRig rig;public F12026_VehicleInput input;public F12026_PowerUnit power;public F12026_AeroModel aero;public F12026_Transmission transmission;
  void Awake(){if(!rig)rig=GetComponent<F12026_VehicleRig>();if(!input)input=GetComponent<F12026_VehicleInput>();if(!power)power=GetComponent<F12026_PowerUnit>();if(!aero)aero=GetComponent<F12026_AeroModel>();if(!transmission)transmission=GetComponent<F12026_Transmission>();}
  void FixedUpdate(){if(!input||!rig)return;rig.ApplySteering(input.Steering);rig.ApplyBrake(input.Brake*5000);rig.ApplyDrive(input.Throttle*900);if(power)power.Simulate(input.Throttle,input.Brake,Time.fixedDeltaTime);if(transmission&&power)transmission.AutomaticShift(power.rpm);if(aero)aero.SetLowDrag(input.AeroLowDrag);}
 }
}