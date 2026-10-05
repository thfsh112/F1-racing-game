using UnityEngine;
namespace F12026 {
 public class F12026_Transmission : MonoBehaviour {
  [Range(1,8)] public int gear=1; public bool reverse; public float shiftUpRpm=11500,shiftDownRpm=5000,finalDrive=3.2f;
  public float[] gearRatios={3.1f,2.4f,1.9f,1.55f,1.3f,1.1f,.95f,.82f};
  public int Gear=>reverse?-1:gear;
  public float Ratio=>reverse?-2.8f:gearRatios[Mathf.Clamp(gear-1,0,7)]*finalDrive;
  public void AutomaticShift(float rpm){if(rpm>shiftUpRpm&&gear<8)gear++;else if(rpm<shiftDownRpm&&gear>1)gear--;}
 }
}