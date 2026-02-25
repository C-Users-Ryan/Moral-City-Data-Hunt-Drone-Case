using UnityEngine;

public class StableDrone : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform targetSphere;
    
    [Header("Flight Settings")]
    [SerializeField] private float followDistance = 1.5f; // Closer to the sphere
    [SerializeField] private float hoverHeight = 1.2f;
    [SerializeField] private float followSpeed = 0.5f;   // Lower is smoother/slower
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Stability")]
    [SerializeField] private float maxTiltAngle = 10f;    // Keeps it from looking like a stunt drone
    [SerializeField] private float leanAmount = 2f;

    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;

    void LateUpdate() // LateUpdate is smoother for following moving objects
    {
        if (targetSphere == null) return;

        // 1. Calculate the Target Position (Stay close to the sphere)
        Vector3 offset = (transform.position - targetSphere.position).normalized * followDistance;
        Vector3 targetPos = targetSphere.position + offset + (Vector3.up * hoverHeight);

        // 2. Move using SmoothDamp (This provides the stable, weighted feel)
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPos, 
            ref smoothVelocity, 
            followSpeed
        );

        // 3. Obstacle Avoidance (Gently nudges away from walls)
        ApplyGentleAvoidance();

        // 4. Realistic Drone Rotation & Tilt
        ApplyStableRotation();
    }

    void ApplyGentleAvoidance()
    {
        RaycastHit hit;
        Vector3[] checkDirections = { transform.forward, transform.right, -transform.right, transform.up, -transform.up };

        foreach (Vector3 dir in checkDirections)
        {
            if (Physics.SphereCast(transform.position, 0.4f, dir, out hit, 1.0f))
            {
                if (hit.transform != targetSphere && hit.transform != transform)
                {
                    // Gently push away from the obstacle
                    transform.position += hit.normal * Time.deltaTime * 1.5f;
                }
            }
        }
    }

    void ApplyStableRotation()
    {
        // Always look toward the sphere
        Vector3 lookDir = targetSphere.position - transform.position;
        lookDir.y = 0; // Keep the rotation horizontal
        
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            
            // Calculate Tilt based on horizontal velocity
            float tiltZ = Mathf.Clamp(-smoothVelocity.x * leanAmount, -maxTiltAngle, maxTiltAngle);
            float tiltX = Mathf.Clamp(smoothVelocity.z * leanAmount, -maxTiltAngle, maxTiltAngle);
            
            Quaternion finalRot = targetRot * Quaternion.Euler(tiltX, 0, tiltZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * rotationSpeed);
        }
    }
}