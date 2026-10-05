using UnityEngine;
namespace F12026 {
 public class F12026_PowerUnit:MonoBehaviour {
  public float maxIcePowerKw=560,rpm=4000,idleRpm=3500,maxRpm=15000,maxMguKPowerKw=120;[Range(0,1)]public float stateOfCharge=.82f;public float deployRate=.12f,regenRate=.08f;public bool overdrive;public float overdriveMultiplier=1.08f;
  public float AvailablePowerKw{get{float e=stateOfCharge>.02f?maxMguKPowerKw:0;float p=maxIcePowerKw+e;return overdrive?p*overdriveMultiplier:p;}}
  public void Simulate(float throttle,float brake,float dt){rpm=Mathf.Lerp(rpm,Mathf.Lerp(idleRpm,maxRpm,Mathf.Clamp01(throttle)),dt*5);if(overdrive&&throttle>.8f)stateOfCharge=Mathf.Clamp01(stateOfCharge-deployRate*dt);else if(brake>.1f)stateOfCharge=Mathf.Clamp01(stateOfCharge+regenRate*brake*dt);}
 }
}