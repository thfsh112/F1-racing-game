using UnityEngine;

public class F12026_AeroModel : MonoBehaviour
{
    public Rigidbody rb;
    public float airDensity = 1.225f;
    public float frontalArea = 1.5f;
    public float downforceCoefficient = 3f;
    public float dragCoefficient = 0.32f;
    [Range(0.2f,1f)] public float xModeDownforce = 0.55f;
    [Range(0.2f,1f)] public float xModeDrag = 0.55f;
    public bool xMode;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!rb) return;
        float speed = rb.linearVelocity.magnitude;
        float dynamicPressure = 0.5f * airDensity * speed * speed * frontalArea;
        float downforce = dynamicPressure * downforceCoefficient *
                          (xMode ? xModeDownforce : 1f);
        float drag = dynamicPressure * dragCoefficient *
                     (xMode ? xModeDrag : 1f);

        rb.AddForce(Vector3.down * downforce);
        if (speed > 0.1f)
            rb.AddForce(-rb.linearVelocity.normalized * drag);
    }
}