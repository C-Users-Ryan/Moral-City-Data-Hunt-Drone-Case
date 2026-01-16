using UnityEngine;

public class Navigation : MonoBehaviour
{
    [Header("Target Plane")]
    [SerializeField] private GameObject floorPlane;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float rotationSpeed = 300f;
    
    [Header("Detection Settings")]
    [SerializeField] private float edgeCheckDistance = 0.6f; // Shortened for precision
    [SerializeField] private float obstacleRayDist = 1.0f;
    
    [Header("Direction Change")]
    [SerializeField] private float minWalkTime = 2f;
    [SerializeField] private float maxWalkTime = 5f;
    
    private Vector3 currentDirection;
    private float directionTimer;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // IMPORTANT: Set this to true to stop physics from "sliding" the character
        rb.isKinematic = true; 

        if (floorPlane == null)
            Debug.LogError("Assign the Plane to the Floor Plane slot!");

        ChooseNewDirection();
    }
    
    void Update()
    {
        // 1. Calculate the next position before we actually move there
        Vector3 nextMove = transform.forward * walkSpeed * Time.deltaTime;
        Vector3 potentialPosition = transform.position + nextMove;

        // 2. Check if that potential position is still on the plane
        if (IsPositionSafe(potentialPosition) && !IsObstacleInWay())
        {
            // Only move if it's safe
            transform.position += nextMove;
        }
        else
        {
            // If unsafe, stop and find a new way immediately
            ChooseNewDirection();
        }

        if (currentDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0) ChooseNewDirection();
    }

    bool IsPositionSafe(Vector3 pos)
    {
        RaycastHit hit;
        Vector3 rayOrigin = pos + (transform.forward * edgeCheckDistance) + Vector3.up * 1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2f))
        {
            if (hit.collider.gameObject == floorPlane) return true;
        }
        return false;
    }

    bool IsObstacleInWay()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, obstacleRayDist))
        {
            if (hit.collider.gameObject != floorPlane) return true;
        }
        return false;
    }
    
    void ChooseNewDirection()
    {
        int attempts = 0;
        while (attempts < 50)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            
            if (Physics.Raycast(transform.position + (dir * 1.5f) + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 2f))
            {
                if (hit.collider.gameObject == floorPlane)
                {
                    currentDirection = dir;
                    directionTimer = Random.Range(minWalkTime, maxWalkTime);
                    return;
                }
            }
            attempts++;
        }
        currentDirection = -transform.forward;
    }
}