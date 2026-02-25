using UnityEngine;

public class TakeoffController : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float targetAltitude = 10f;
    [SerializeField] private float thrustPower = 15f;
    [SerializeField] private float startDelay = 0.5f;
    
    [Header("Hover Behavior")]
    [SerializeField] private bool enableHoverBob = true;
    [SerializeField] private float bobAmount = 0.3f;
    [SerializeField] private float bobSpeed = 1f;
    
    private Rigidbody rb;
    private float startTime;
    private float targetHeight;
    private bool hasReachedTarget = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = true;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        
        startTime = Time.time;
        targetHeight = transform.position.y + targetAltitude;
    }
    
    void FixedUpdate()
    {
        if (Time.time < startTime + startDelay)
            return;
            
        ApplyLift();
        
        if (hasReachedTarget && enableHoverBob)
        {
            ApplyHoverBob();
        }
    }
    
    void ApplyLift()
    {
        float currentHeight = transform.position.y;
        float heightDifference = targetHeight - currentHeight;
        
        if (!hasReachedTarget && Mathf.Abs(heightDifference) < 0.5f)
        {
            hasReachedTarget = true;
        }
        
        float hoverForce = Physics.gravity.magnitude * rb.mass;
        
        float liftForce = hoverForce;
        if (heightDifference > 0.1f)
        {
            float thrustMultiplier = Mathf.Clamp01(heightDifference / 5f);
            liftForce += thrustPower * rb.mass * thrustMultiplier;
        }
        else if (heightDifference < -0.1f)
        {
            liftForce *= 0.8f;
        }
        
        rb.AddForce(Vector3.up * liftForce, ForceMode.Force);
        
        if (Mathf.Abs(heightDifference) < 1f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.95f, rb.linearVelocity.z);
        }
    }
    
    void ApplyHoverBob()
    {
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        float newTarget = targetHeight + bobOffset;
        
        float heightDiff = newTarget - transform.position.y;
        rb.AddForce(Vector3.up * heightDiff * rb.mass * 2f, ForceMode.Force);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 targetPos = new Vector3(transform.position.x, targetHeight, transform.position.z);
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        Gizmos.DrawLine(transform.position, targetPos);
    }
}