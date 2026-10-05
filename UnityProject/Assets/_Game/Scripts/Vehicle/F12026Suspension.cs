using UnityEngine;

public class F12026Suspension : MonoBehaviour
{
    [Header("Geometry")]
    public Transform suspensionAnchor;
    [Min(0.05f)] public float restLength = 0.32f;
    [Min(0.01f)] public float maxCompression = 0.14f;
    [Min(0.01f)] public float maxDroop = 0.10f;
    [Min(0.05f)] public float wheelRadius = 0.36f;

    [Header("Spring")]
    [Min(1000f)] public float springRate = 105000f;
    [Min(100f)] public float bumpDamping = 9000f;
    [Min(100f)] public float reboundDamping = 12000f;
    [Min(0f)] public float bumpStopRate = 220000f;

    [Header("Contact")]
    public LayerMask trackMask = ~0;
    public float contactOffset = 0.015f;

    public bool Grounded { get; private set; }
    public Vector3 ContactPoint { get; private set; }
    public Vector3 ContactNormal { get; private set; } = Vector3.up;
    public float Compression01 { get; private set; }
    public float SuspensionForce { get; private set; }
    public float TravelFromRest { get; private set; }
    public float CurrentLength { get; private set; }

    private float previousCompression;
    private Rigidbody body;

    public void Initialize(Rigidbody rb)
    {
        body = rb;
        if (suspensionAnchor == null)
            suspensionAnchor = transform;
    }

    public void Simulate(float dt)
    {
        if (body == null)
            return;

        Vector3 origin = suspensionAnchor.position;
        Vector3 down = -suspensionAnchor.up;
        float rayLength = restLength + maxDroop + wheelRadius + contactOffset;

        Grounded = Physics.Raycast(
            origin,
            down,
            out RaycastHit hit,
            rayLength,
            trackMask,
            QueryTriggerInteraction.Ignore);

        float compression = 0f;
        float currentLength = restLength + maxDroop;

        if (Grounded)
        {
            currentLength = Mathf.Clamp(
                hit.distance - wheelRadius,
                restLength - maxCompression,
                restLength + maxDroop);

            compression = Mathf.Clamp01(
                (restLength - currentLength) /
                Mathf.Max(maxCompression, 0.001f));

            ContactPoint = hit.point;
            ContactNormal = hit.normal;

            float compressionVelocity =
                (compression - previousCompression) /
                Mathf.Max(dt, 0.0001f);

            float springForce =
                Mathf.Max(0f, compression * maxCompression * springRate);

            float damping =
                compressionVelocity >= 0f ? bumpDamping : reboundDamping;

            float damperForce =
                Mathf.Max(0f, compressionVelocity * damping);

            float bumpProgress =
                Mathf.InverseLerp(0.90f, 1f, compression);

            float bumpStop =
                bumpProgress * bumpProgress * bumpStopRate * maxCompression;

            SuspensionForce = springForce + damperForce + bumpStop;

            body.AddForceAtPosition(
                ContactNormal * SuspensionForce,
                ContactPoint,
                ForceMode.Force);
        }
        else
        {
            SuspensionForce = 0f;
            ContactPoint =
                origin + down * (currentLength + wheelRadius);
            ContactNormal = -down;
        }

        previousCompression = compression;
        Compression01 = compression;
        CurrentLength = currentLength;
        TravelFromRest = restLength - currentLength;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform a = suspensionAnchor != null ? suspensionAnchor : transform;
        Vector3 origin = a.position;
        Vector3 down = -a.up;

        Gizmos.DrawLine(
            origin,
            origin + down * (restLength + maxDroop + wheelRadius));

        Gizmos.DrawWireSphere(
            origin + down * (restLength - maxCompression + wheelRadius),
            wheelRadius);
    }
#endif
}
