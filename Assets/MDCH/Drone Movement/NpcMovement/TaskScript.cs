using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskScript : MonoBehaviour, WalkInteractable
{
   // variables needed to make the timer part of the script work
   public float TaskTime;
   private float currentTime; 
   private bool isTimerRunning = false;

   public float interactionDistance = 3f;

   // Reference to MoveToWaypoint script
    private MoveToWaypoint moveToWaypoint;


          void Start()
    {
        currentTime = TaskTime;
    }
         public void Interact(){
            // add interaction here
            Debug.Log(" interaction");
            isTimerRunning = true;
         }

         void Update()
    {
      // handles the time aspect of the task
        if(isTimerRunning){
            Debug.Log("Timer Has Started");
        currentTime -= Time.deltaTime;
        if (currentTime <= 0.0f)
        {
            currentTime = 0;
            isTimerRunning = false;
            ResetTimer();
        }
        }

    }

    void ResetTimer(){
      //resets the timer so it can be used more than once
      currentTime = TaskTime;

      // Find all MoveToWaypoint instances and interact with those within the specified distance
        MoveToWaypoint[] allMoveToWaypoints = FindObjectsOfType<MoveToWaypoint>();
        foreach (MoveToWaypoint moveToWaypoint in allMoveToWaypoints)
        {
            if (Vector3.Distance(transform.position, moveToWaypoint.transform.position) <= interactionDistance)
            {
                moveToWaypoint.nextWaypoint();
            }
        }
    }
}
