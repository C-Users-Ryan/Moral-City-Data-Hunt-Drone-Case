using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 1.5f;

    private float t = 0f;

    void Update()
    {
        if (pointA == null || pointB == null)
            return;

        // Increase t over time
        t += Time.deltaTime * speed;

        // PingPong makes t go: 0 → 1 → 0 → 1 → ...
        float lerpValue = Mathf.PingPong(t, 1f);

        // Move object between A and B
        transform.position = Vector3.Lerp(pointA.position, pointB.position, lerpValue);
    }
}
