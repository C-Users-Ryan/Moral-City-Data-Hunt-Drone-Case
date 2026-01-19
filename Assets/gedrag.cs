using UnityEngine;

public class drones_gedrag : MonoBehaviour
{
    [Header("Flight Area")]
    [SerializeField] private Transform plane;
    [SerializeField] private float flightRadius = 20f;
    [SerializeField] private float minHeight = 3f;
    [SerializeField] private float maxHeight = 8f;
    [Header("Movement")]
    [SerializeField] private float cruiseSpeed = 8f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float tiltAmount = 25f;
    [Header("Obstacle Avoidance")]
    [SerializeField] private float detectionDistance = 5f;
    [SerializeField] private float avoidanceForce = 8f;
    [SerializeField] private LayerMask obstacleLayer = ~0;
    [SerializeField] private int numRays = 8;
    [Header("Behavior")]
    [SerializeField] private float waypointReachDistance = 3f;
    [SerializeField] private float newWaypointDelay = 0.5f;
    [SerializeField] private bool aggressiveFlying = true;
    [SerializeField] private float heightChangeFrequency = 0.7f;
    private Rigidbody rb;
    private Vector3 currentWaypoint;
    private Vector3 centerPoint;
    private float nextWaypointTime;
    private float baseHeight;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 3f;
        if (plane != null)
        {
            centerPoint = plane.position + Vector3.up * minHeight;
        }
        else
        {
            centerPoint = transform.position;
        }
        baseHeight = centerPoint.y;
        GenerateNewWaypoint();
    }
    void FixedUpdate()
    {
        NavigateToWaypoint();
        AvoidObstacles();
        ApplyDroneTilt();
        CheckWaypointReached();
        KeepInBounds();
    }
    void NavigateToWaypoint()
    {
        Vector3 directionToWaypoint = (currentWaypoint - transform.position).normalized;
        Vector3 moveDirection = directionToWaypoint;
        float speedMultiplier = aggressiveFlying ? 1.5f : 1f;
        if (rb.linearVelocity.magnitude < cruiseSpeed * 0.5f)
        {
            speedMultiplier *= 2f;
        }
        rb.AddForce(moveDirection * acceleration * speedMultiplier * rb.mass, ForceMode.Force);
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            rb.MoveRotation(newRotation);
        }
    }
    void AvoidObstacles()
    {
        Vector3 avoidanceDirection = Vector3.zero;
        int obstaclesDetected = 0;
        bool closeObstacle = false;
        for (int i = 0; i < numRays; i++)
        {
            float angle = i * (360f / numRays);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, detectionDistance, obstacleLayer))
            {
                if (hit.collider.transform != plane)
                {
                    Vector3 awayFromObstacle = transform.position - hit.point;
                    float weight = 1f - (hit.distance / detectionDistance);


                    if (hit.distance < detectionDistance * 0.5f)
                    {
                        awayFromObstacle.y += 2f;
                        closeObstacle = true;
                    }
                    avoidanceDirection += awayFromObstacle.normalized * weight;
                    obstaclesDetected++;
                    Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
                }
            }
            else
            {
                Debug.DrawRay(transform.position, direction * detectionDistance, Color.green);
            }
        }
        RaycastHit upHit, downHit;
        if (Physics.Raycast(transform.position, Vector3.up, out upHit, detectionDistance * 0.5f, obstacleLayer))
        {
            avoidanceDirection += Vector3.down * 2f;
        }
        if (Physics.Raycast(transform.position, Vector3.down, out downHit, detectionDistance * 0.5f, obstacleLayer))
        {
            if (downHit.collider.transform != plane)
            {
                avoidanceDirection += Vector3.up * 2f;
            }
        }
        if (obstaclesDetected > 0)
        {
            avoidanceDirection = avoidanceDirection.normalized;
            float forceMultiplier = closeObstacle ? 2f : 1f;
            rb.AddForce(avoidanceDirection * avoidanceForce * forceMultiplier * rb.mass, ForceMode.Force);
            if (closeObstacle || obstaclesDetected > 3)
            {
                GenerateNewWaypoint();
            }
        }
    }
    void ApplyDroneTilt()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > 0.1f)
        {
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            float forwardTilt = Vector3.Dot(horizontalVelocity.normalized, forward) * tiltAmount;
            float rightTilt = Vector3.Dot(horizontalVelocity.normalized, right) * tiltAmount;
            Quaternion baseTilt = Quaternion.Euler(-forwardTilt, transform.eulerAngles.y, rightTilt);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, baseTilt, Time.fixedDeltaTime * rotationSpeed));
        }
        else
        {
            Quaternion levelRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, levelRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }
    void CheckWaypointReached()
    {
        float distance = Vector3.Distance(transform.position, currentWaypoint);
        bool reachedWaypoint = distance < waypointReachDistance && Time.time > nextWaypointTime;
        bool stuckOrSlow = rb.linearVelocity.magnitude < 1f && Time.time > nextWaypointTime + 2f;
        if (reachedWaypoint || stuckOrSlow)
        {
            GenerateNewWaypoint();
        }
        if (distance < waypointReachDistance * 0.5f)
        {
            GenerateNewWaypoint();
        }
    }
    void GenerateNewWaypoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * flightRadius;
        float randomHeight;
        if (Random.value < heightChangeFrequency)
        {
            randomHeight = Random.Range(minHeight, maxHeight);
        }
        else
        {
            float currentRelativeHeight = transform.position.y - baseHeight;
            randomHeight = Mathf.Clamp(currentRelativeHeight + Random.Range(-2f, 2f), minHeight, maxHeight);
        }
        currentWaypoint = centerPoint + new Vector3(randomCircle.x, randomHeight - baseHeight, randomCircle.y);
        nextWaypointTime = Time.time + newWaypointDelay;
    }
    void KeepInBounds()
    {
        float distanceFromCenter = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                     new Vector3(centerPoint.x, 0, centerPoint.z));
        if (distanceFromCenter > flightRadius)
        {
            Vector3 directionToCenter = (centerPoint - transform.position).normalized;
            directionToCenter.y = 0;
            rb.AddForce(directionToCenter * acceleration * rb.mass * 2f, ForceMode.Force);
        }
        if (transform.position.y < baseHeight + minHeight - 1f)
        {
            rb.AddForce(Vector3.up * rb.mass * 5f, ForceMode.Force);
        }
        else if (transform.position.y > baseHeight + maxHeight + 1f)
        {
            rb.AddForce(Vector3.down * rb.mass * 3f, ForceMode.Force);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = plane != null ? plane.position : centerPoint;
        Gizmos.DrawWireSphere(center + Vector3.up * minHeight, flightRadius);
        Gizmos.DrawWireSphere(center + Vector3.up * maxHeight, flightRadius);
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentWaypoint, waypointReachDistance);
            Gizmos.DrawLine(transform.position, currentWaypoint);
        }
    }
}