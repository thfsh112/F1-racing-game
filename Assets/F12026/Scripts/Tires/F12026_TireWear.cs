using UnityEngine;
namespace F12026 {
 public class F12026_TireWear:MonoBehaviour {
  [Range(0,1)] public float wear; public float baseWearRate=.000015f;
  public float GripMultiplier=>Mathf.Lerp(1f,.72f,wear);
  public void Simulate(float load,float slip,float temperature,float dt){float p=Mathf.Max(0,temperature-105)/80f;wear=Mathf.Clamp01(wear+baseWearRate*(1+load*.0005f)*(1+Mathf.Abs(slip))*(1+p)*dt);}
 }
}