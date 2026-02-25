using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface WalkInteractable
{
    void Interact();
}
public class MoveToWaypoint : MonoBehaviour
{
    private NavMeshAgent agent;
    public GameObject waypointPath; // Reference the parent object holding waypoints
    private Transform[] waypoints;

    public float minDistance = 8f;
    public float speed = 5.0f;

    private int currentWaypointIndex;
    private bool isPerformingAction = false;

    Animator m_Animator;

   void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponentInChildren<Animator>();
        m_Animator.SetBool("Walking", true);
        waypoints = new Transform[waypointPath.transform.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = waypointPath.transform.GetChild(i);
        }

        ChooseNewRandomWaypoint();
    }

    void Update()
    {
        if (!isPerformingAction)
        {
            MoveTowardsWaypoint();
        }
    }

    void ChooseNewRandomWaypoint()
    {
        currentWaypointIndex = Random.Range(0, waypoints.Length);
    }

    public void MoveTowardsWaypoint()
    {
        // Calculate the distance to the current waypoint
        float distance = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);

        // If within the minimum distance, interact with the WalkInteractable component
        if (distance < minDistance)
        {
            // Get the WalkInteractable component at the waypoint position
            WalkInteractable interactable = waypoints[currentWaypointIndex].GetComponent<WalkInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
                isPerformingAction = true; // Prevent further movement until action is complete
                m_Animator.SetBool("Walking", false);
            }
        }
        else
        {
            // Move towards the current waypoint
            agent.SetDestination(waypoints[currentWaypointIndex].position);
            agent.speed = speed;
        }
    }

    public void nextWaypoint()
    {
        ChooseNewRandomWaypoint();
        isPerformingAction = false;
        m_Animator.SetBool("Walking", true);
    }

}
