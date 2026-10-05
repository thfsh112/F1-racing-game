using UnityEngine;

public class F12026ChaseCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Rigidbody targetBody;

    [Header("Position")]
    public Vector3 followOffset = new Vector3(0f, 2.9f, -6.8f);
    public float positionSmooth = 7f;
    public float rotationSmooth = 8f;

    [Header("Look Ahead")]
    public float lookHeight = 0.75f;
    public float lookAhead = 5f;
    public float maxLookAhead = 14f;

    [Header("Speed FOV")]
    public Camera targetCamera;
    public float minFov = 62f;
    public float maxFov = 74f;
    public float fovAtKph = 260f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetBody == null && target != null)
            targetBody = target.GetComponentInParent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition =
            target.TransformPoint(followOffset);

        float speed = targetBody != null
            ? targetBody.linearVelocity.magnitude * 3.6f
            : 0f;

        float speed01 = Mathf.InverseLerp(0f, fovAtKph, speed);
        float dynamicLookAhead =
            Mathf.Lerp(lookAhead, maxLookAhead, speed01);

        Vector3 velocity =
            targetBody != null ? targetBody.linearVelocity : target.forward;

        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(velocity, Vector3.up);

        Vector3 forward =
            horizontalVelocity.sqrMagnitude > 1f
                ? horizontalVelocity.normalized
                : target.forward;

        Vector3 lookPoint =
            target.position +
            Vector3.up * lookHeight +
            forward * dynamicLookAhead;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

        Quaternion desiredRotation =
            Quaternion.LookRotation(
                lookPoint - transform.position,
                Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));

        if (targetCamera != null)
        {
            float desiredFov = Mathf.Lerp(minFov, maxFov, speed01);
            targetCamera.fieldOfView = Mathf.Lerp(
                targetCamera.fieldOfView,
                desiredFov,
                1f - Mathf.Exp(-5f * Time.deltaTime));
        }
    }
}
