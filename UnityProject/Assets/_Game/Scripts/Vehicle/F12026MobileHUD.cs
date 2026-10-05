using UnityEngine;
using UnityEngine.UI;

public class F12026MobileHUD : MonoBehaviour
{
    public F12026CarController car;

    [Header("Optional UI")]
    public Text speedText;
    public Text gearText;
    public Text aeroText;
    public Slider batterySlider;

    private void Update()
    {
        if (car == null) return;

        if (speedText != null)
            speedText.text = Mathf.RoundToInt(car.SpeedKph) + " km/h";

        if (gearText != null)
        {
            int gear = car.gearbox != null ? car.gearbox.gear : 1;
            gearText.text = gear.ToString();
        }

        if (aeroText != null)
            aeroText.text = car.activeAeroXMode ? "X · LOW DRAG" : "Z · DOWNFORCE";

        if (batterySlider != null && car.powerUnit != null)
            batterySlider.value =
                Mathf.Clamp01(car.powerUnit.BatteryEnergyMJ / 
                               Mathf.Max(0.01f, car.powerUnit.batteryCapacityMJ));
    }
}
