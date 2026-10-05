using UnityEngine;
namespace F12026 {
 public class F12026_TireModel : MonoBehaviour {
  public enum Compound{Soft,Medium,Hard,Intermediate,Wet}
  public Compound compound=Compound.Medium;
  public float peakGrip=1.55f,longitudinalStiffness=10f,lateralStiffness=12f;
  public float slipAngle,slipRatio; [Range(0,1)] public float normalizedGrip=1;
  public float EvaluateGrip(float speedKph,float temperatureC){float t=Mathf.Clamp01(1f-Mathf.Abs(temperatureC-92f)/65f);float s=Mathf.Clamp01(.72f+speedKph/400f);return peakGrip*(normalizedGrip=Mathf.Clamp01(t*s));}
  public float EvaluateLateralForce(float slip){slipAngle=slip;return Mathf.Clamp(slip*lateralStiffness,-peakGrip,peakGrip)*normalizedGrip;}
  public float EvaluateLongitudinalForce(float slip){slipRatio=slip;return Mathf.Clamp(slip*longitudinalStiffness,-peakGrip,peakGrip)*normalizedGrip;}
 }
}