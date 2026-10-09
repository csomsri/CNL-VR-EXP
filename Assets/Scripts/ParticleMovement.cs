using UnityEngine;

public class CyclotronParticle : MonoBehaviour
{
    [Header("Particle")]
    public float charge = 1.0f;
    public float mass = 1.0f;
    public Vector3 velocity;

    [Header("Magnetic Field")]
    public Vector3 magneticField = new Vector3(0f, 1f, 0f);

    [Header("Electric Field")]
    public float electricFieldStrength = 10f;
    public Vector3 electricFieldDirection = Vector3.right;

    [Header("Cyclotron Gap")]
    public Transform gapCenter;
    public Vector3 gapSize = new Vector3(1f, 1f, 5f);

    [Header("RF")]
    public float rfFrequency = 1.0f;

    [Header("Orbit Center")]
    public float orbitCenterBoxSize = 0.2f;

    private void FixedUpdate()
    {
        // Calculate the Lorentz force
        if (velocity != Vector3.zero)
        {
            Vector3 lorentzForce = Vector3.Cross(velocity, magneticField) * charge;
            Vector3 acceleration = lorentzForce / mass;
            // Update the velocity and position of the particle
            velocity += acceleration * Time.fixedDeltaTime;
            transform.position += velocity * Time.fixedDeltaTime;

        }
    }

    // On Entering the cyclotron gap, apply the electric field to accelerate the particle
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeeGap"))
        {
            // Apply the electric field to accelerate the particle
            velocity += electricFieldDirection * (electricFieldStrength * Time.fixedDeltaTime);        }

    }

    // Render a gizmo to visualize the cyclotron gap
    private void OnDrawGizmos()
    {
        if (gapCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(gapCenter.position, gapSize);
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