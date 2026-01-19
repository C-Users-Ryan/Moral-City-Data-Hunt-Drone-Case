using UnityEngine;

public class AerialEvasionController : MonoBehaviour
{
    [Header("Flight Zone")]
    [SerializeField] private Transform flightAnchor;
    [SerializeField] private float zoneRadius = 20f;
    [SerializeField] private float minAltitude = 3f;
    [SerializeField] private float maxAltitude = 8f;

    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float speedCap = 15f;
    [SerializeField] private float thrustPower = 8f;
    [SerializeField] private float turnRate = 3f;
    [SerializeField] private float bankingAngle = 25f;

    [Header("Avoidance System")]
    [SerializeField] private float scanDistance = 5f;
    [SerializeField] private float evadeStrength = 8f;
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private int rayCount = 8;

    [Header("AI Behaviour")]
    [SerializeField] private float waypointRadius = 3f;
    [SerializeField] private float waypointCooldown = 0.5f;
    [SerializeField] private bool isAggressive = true;
    [SerializeField] private float altitudeVariationChance = 0.7f;

    private Rigidbody droneBody;
    private Vector3 targetPoint;
    private Vector3 anchorPoint;
    private float nextTargetTime;
    private float baseAltitude;

    void Start()
    {
        droneBody = GetComponent<Rigidbody>();
        if (droneBody == null)
            droneBody = gameObject.AddComponent<Rigidbody>();

        droneBody.useGravity = true;
        droneBody.linearDamping = 0.5f;
        droneBody.angularDamping = 3f;

        anchorPoint = flightAnchor != null ?
            flightAnchor.position + Vector3.up * minAltitude : transform.position;

        baseAltitude = anchorPoint.y;
        CreateNewTarget();
    }

    void FixedUpdate()
    {
        FlyTowardsTarget();
        RunAvoidanceScan();
        ApplyBanking();
        CheckTargetReached();
        EnforceFlightBounds();
    }

    void FlyTowardsTarget()
    {
        Vector3 dir = (targetPoint - transform.position).normalized;
        float speedBoost = isAggressive ? 1.5f : 1f;

        if (droneBody.linearVelocity.magnitude < baseSpeed * 0.5f)
            speedBoost *= 2f;

        droneBody.AddForce(dir * thrustPower * speedBoost * droneBody.mass, ForceMode.Force);

        if (droneBody.linearVelocity.magnitude > speedCap)
            droneBody.linearVelocity = droneBody.linearVelocity.normalized * speedCap;

        Vector3 flatVel = new Vector3(droneBody.linearVelocity.x, 0, droneBody.linearVelocity.z);
        if (flatVel.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVel);
            droneBody.MoveRotation(
                Quaternion.Slerp(droneBody.rotation, targetRot, Time.fixedDeltaTime * turnRate));
        }
    }

    void RunAvoidanceScan()
    {
        Vector3 evadeDir = Vector3.zero;
        int hits = 0;
        bool dangerClose = false;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (360f / rayCount);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, scanDistance, collisionMask))
            {
                if (hit.collider.transform != flightAnchor)
                {
                    Vector3 away = transform.position - hit.point;
                    float weight = 1f - (hit.distance / scanDistance);

                    if (hit.distance < scanDistance * 0.5f)
                    {
                        away.y += 2f;
                        dangerClose = true;
                    }

                    evadeDir += away.normalized * weight;
                    hits++;
                }
            }
        }

        if (hits > 0)
        {
            evadeDir.Normalize();
            float multiplier = dangerClose ? 2f : 1f;

            droneBody.AddForce(
                evadeDir * evadeStrength * multiplier * droneBody.mass,
                ForceMode.Force);

            if (dangerClose || hits > 3)
                CreateNewTarget();
        }
    }

    void ApplyBanking()
    {
        Vector3 flatVel = new Vector3(droneBody.linearVelocity.x, 0, droneBody.linearVelocity.z);

        if (flatVel.magnitude > 0.1f)
        {
            float forwardTilt = Vector3.Dot(flatVel.normalized, transform.forward) * bankingAngle;
            float sideTilt = Vector3.Dot(flatVel.normalized, transform.right) * bankingAngle;

            Quaternion tilt = Quaternion.Euler(-forwardTilt, transform.eulerAngles.y, sideTilt);
            droneBody.MoveRotation(
                Quaternion.Slerp(droneBody.rotation, tilt, Time.fixedDeltaTime * turnRate));
        }
    }

    void CheckTargetReached()
    {
        float dist = Vector3.Distance(transform.position, targetPoint);
        bool arrived = dist < waypointRadius && Time.time > nextTargetTime;
        bool stalled = droneBody.linearVelocity.magnitude < 1f &&
                       Time.time > nextTargetTime + 2f;

        if (arrived || stalled || dist < waypointRadius * 0.5f)
            CreateNewTarget();
    }

    void CreateNewTarget()
    {
        Vector2 circle = Random.insideUnitCircle * zoneRadius;

        float newHeight = Random.value < altitudeVariationChance ?
            Random.Range(minAltitude, maxAltitude) :
            Mathf.Clamp(transform.position.y - baseAltitude + Random.Range(-2f, 2f),
                        minAltitude, maxAltitude);

        targetPoint = anchorPoint +
                      new Vector3(circle.x, newHeight - baseAltitude, circle.y);

        nextTargetTime = Time.time + waypointCooldown;
    }

    void EnforceFlightBounds()
    {
        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(anchorPoint.x, 0, anchorPoint.z));

        if (dist > zoneRadius)
        {
            Vector3 toCenter = (anchorPoint - transform.position).normalized;
            toCenter.y = 0;
            droneBody.AddForce(toCenter * thrustPower * droneBody.mass * 2f);
        }

        if (transform.position.y < baseAltitude + minAltitude - 1f)
            droneBody.AddForce(Vector3.up * droneBody.mass * 5f);
        else if (transform.position.y > baseAltitude + maxAltitude + 1f)
            droneBody.AddForce(Vector3.down * droneBody.mass * 3f);
    }
}