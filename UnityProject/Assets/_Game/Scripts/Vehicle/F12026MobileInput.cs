using UnityEngine;
using UnityEngine.EventSystems;

public class F12026MobileInput : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum ControlType { Steering, Throttle, Brake }

    public ControlType controlType;
    public F12026CarController car;

    [Header("Steering")]
    public float steeringSensitivity = 1.15f;
    public float steeringDeadZone = 0.06f;

    private bool pressed;
    private float steer;
    private float throttle;
    private float brake;

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        Apply(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;

        if (controlType == ControlType.Steering) steer = 0f;
        if (controlType == ControlType.Throttle) throttle = 0f;
        if (controlType == ControlType.Brake) brake = 0f;

        Push();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pressed)
            Apply(eventData);
    }

    private void Apply(PointerEventData e)
    {
        if (car == null) return;

        if (controlType == ControlType.Steering)
        {
            // Steering pad uses the full horizontal range, with a small dead zone.
            float normalized = e.position.x / Mathf.Max(Screen.width, 1f) * 2f - 1f;
            steer = Mathf.Abs(normalized) < steeringDeadZone
                ? 0f
                : Mathf.Clamp(normalized * steeringSensitivity, -1f, 1f);
        }
        else if (controlType == ControlType.Throttle)
        {
            throttle = 1f;
            brake = 0f;
        }
        else
        {
            brake = 1f;
            throttle = 0f;
        }

        Push();
    }

    private void Push()
    {
        if (car == null) return;
        car.SetMobileInput(steer, throttle, brake);
    }
}
