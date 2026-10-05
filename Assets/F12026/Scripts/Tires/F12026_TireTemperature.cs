using UnityEngine;
namespace F12026 {
 public class F12026_TireTemperature:MonoBehaviour {
  public float temperatureC=82,coreTemperatureC=82,ambientTemperatureC=25,heatingRate=.35f,coolingRate=.08f;
  public void Simulate(float slipEnergy,float brakeEnergy,float dt){float h=(Mathf.Abs(slipEnergy)+Mathf.Abs(brakeEnergy))*heatingRate;float c=(temperatureC-ambientTemperatureC)*coolingRate;temperatureC=Mathf.Clamp(temperatureC+(h-c)*dt,20,160);coreTemperatureC=Mathf.Lerp(coreTemperatureC,temperatureC,dt*.4f);}
 }
}