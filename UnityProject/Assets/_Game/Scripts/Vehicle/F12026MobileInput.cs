using UnityEngine;
using UnityEngine.EventSystems;

public class F12026MobileInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum ControlType { Steering, Throttle, Brake }
    public ControlType controlType;
    public F12026CarController car;
    public float steeringSensitivity = 1.2f;

    public void OnPointerDown(PointerEventData eventData) => Apply(eventData);
    public void OnPointerUp(PointerEventData eventData) => Release();
    public void OnDrag(PointerEventData eventData) => Apply(eventData);

    private void Apply(PointerEventData e)
    {
        if (car == null) return;

        if (controlType == ControlType.Steering)
            car.steerInput = Mathf.Clamp(
                e.position.x / Mathf.Max(Screen.width,1f) * 2f - 1f,
                -1f, 1f) * steeringSensitivity;
        else if (controlType == ControlType.Throttle)
            car.throttleInput = 1f;
        else
            car.brakeInput = 1f;
    }

    private void Release()
    {
        if (car == null) return;
        if (controlType == ControlType.Steering) car.steerInput = 0f;
        if (controlType == ControlType.Throttle) car.throttleInput = 0f;
        if (controlType == ControlType.Brake) car.brakeInput = 0f;
    }
}
