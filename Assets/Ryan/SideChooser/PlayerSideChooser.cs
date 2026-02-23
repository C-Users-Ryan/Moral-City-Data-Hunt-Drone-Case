using System.Collections;
using UnityEngine;

public class PlayerSideChooser : MonoBehaviour
{
    [Header("Case Controller")]
    public InspectorCaseController caseController;

    [Header("Voting Zones (Trigger Colliders)")]
    public Collider voteZoneA;
    public Collider voteZoneB;

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Voting Settings")]
    public float countdownTime = 5f;

    private bool playerInZoneA = false;
    private bool playerInZoneB = false;
    private bool votingInProgress = false;

    // ------------------------------------------------
    // START VOTING (CALL THIS FROM BUTTON / EVENT)
    // ------------------------------------------------
    public void StartVote()
    {
        if (votingInProgress)
            return;

        StartCoroutine(VotingRoutine());
    }

    // ------------------------------------------------
    // COUNTDOWN
    // ------------------------------------------------
    IEnumerator VotingRoutine()
    {
        votingInProgress = true;

        float timer = countdownTime;

        Debug.Log("Voting started!");

        while (timer > 0f)
        {
            Debug.Log("Voting ends in: " + Mathf.Ceil(timer));
            timer -= Time.deltaTime;
            yield return null;
        }

        DecideVote();

        votingInProgress = false;
    }

    // ------------------------------------------------
    // DECIDE RESULT
    // ------------------------------------------------
    void DecideVote()
    {
        if (caseController == null)
        {
            Debug.LogWarning("No InspectorCaseController assigned!");
            return;
        }

        if (playerInZoneA && !playerInZoneB)
        {
            Debug.Log("Player voted OPTION A");
            caseController.SelectOption(0);
        }
        else if (playerInZoneB && !playerInZoneA)
        {
            Debug.Log("Player voted OPTION B");
            caseController.SelectOption(1);
        }
        else
        {
            Debug.Log("No valid vote (player in none or both zones)");
        }
    }

    // ------------------------------------------------
    // TRIGGER DETECTION
    // ------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (other.bounds.Intersects(voteZoneA.bounds))
            playerInZoneA = true;

        if (other.bounds.Intersects(voteZoneB.bounds))
            playerInZoneB = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (!other.bounds.Intersects(voteZoneA.bounds))
            playerInZoneA = false;

        if (!other.bounds.Intersects(voteZoneB.bounds))
            playerInZoneB = false;
    }
}