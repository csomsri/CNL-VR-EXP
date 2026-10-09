using UnityEngine;

public class CyclotronParticle : MonoBehaviour
{
    [Header("Particle")]
    public float charge = 1.0f;
    [Min(0.000001f)] public float mass = 1.0f;
    public Vector3 velocity = new Vector3(0f, 0f, 0.25f);

    [Header("Magnetic Field")]
    public Vector3 magneticField = new Vector3(0f, 1f, 0f);

    [Header("Electric Field")]
    public float electricFieldStrength = 0.05f;
    public Vector3 electricFieldDirection = Vector3.forward;

    [Header("Cyclotron Gap")]
    [Tooltip("Gap object with a BoxCollider. Its collider size, center, rotation and scale define the acceleration region.")]
    public Transform gapCenter;

    [Header("RF")]
    [Tooltip("RF frequency in Hz. Zero automatically matches the cyclotron frequency.")]
    [Min(0f)] public float rfFrequency = 0f;
    [Tooltip("Phase of E = strength * sin(2*pi*f*t + phase). 90 degrees starts at peak positive field.")]
    public float rfPhaseDegrees = 90f;

    [Header("Injection")]
    [Tooltip("Spawn at the assigned gap center when the simulation begins.")]
    public bool startAtGapCenter = true;

    [Header("Orbit Center")]
    public float orbitCenterBoxSize = 0.2f;

    private float simulationTime;

    private void Start()
    {
        if (gapCenter == null || !gapCenter.TryGetComponent<BoxCollider>(out var gap))
        {
            Debug.LogWarning("Assign a gap object with a BoxCollider to enable RF acceleration.", this);
            return;
        }

        if (startAtGapCenter)
            transform.position = gap.transform.TransformPoint(gap.center);
    }

    private void FixedUpdate()
    {
        if (mass <= 0f) return;

        float dt = Time.fixedDeltaTime;
        float chargeToMass = charge / mass;
        Vector3 oldVelocity = velocity;
        Vector3 electricAcceleration = Vector3.zero;
        if (gapCenter != null && gapCenter.TryGetComponent<BoxCollider>(out var gap))
        {
            // Test in collider-local space so object and parent scaling,
            // rotation, and collider center offsets are all respected.
            Vector3 offset = gap.transform.InverseTransformPoint(transform.position) - gap.center;
            Vector3 halfSize = gap.size * 0.5f;
            if (Mathf.Abs(offset.x) <= halfSize.x &&
                Mathf.Abs(offset.y) <= halfSize.y &&
                Mathf.Abs(offset.z) <= halfSize.z)
            {
                float frequency = rfFrequency > 0f ? rfFrequency :
                    Mathf.Abs(chargeToMass) * magneticField.magnitude / (2f * Mathf.PI);
                float phase = 2f * Mathf.PI * frequency * (simulationTime + dt * 0.5f)
                    + rfPhaseDegrees * Mathf.Deg2Rad;
                electricAcceleration = electricFieldDirection.normalized *
                    (chargeToMass * electricFieldStrength * Mathf.Sin(phase));
            }
        }

        // Boris rotation preserves speed in a magnetic field; Euler integration
        // artificially increased the orbit radius on every physics step.
        Vector3 halfKick = electricAcceleration * (dt * 0.5f);
        Vector3 vMinus = velocity + halfKick;
        Vector3 t = magneticField * (chargeToMass * dt * 0.5f);
        Vector3 s = 2f * t / (1f + t.sqrMagnitude);
        Vector3 vPrime = vMinus + Vector3.Cross(vMinus, t);
        velocity = vMinus + Vector3.Cross(vPrime, s) + halfKick;
        transform.position += (oldVelocity + velocity) * (dt * 0.5f);
        simulationTime += dt;
    }

    // Render a gizmo to visualize the cyclotron gap
    private void OnDrawGizmos()
    {
        if (gapCenter != null && gapCenter.TryGetComponent<BoxCollider>(out var gap))
        {
            Gizmos.color = Color.yellow;
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = gap.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(gap.center, gap.size);
            Gizmos.matrix = previousMatrix;
        }
        // Draw the orbit center box
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * orbitCenterBoxSize);
    }

    // Render a gizmo for the center of orbit
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

}
