using System.Collections;
using UnityEngine;

public class PlayerSideChooser : MonoBehaviour
{
    [Header("Case Controller")]
    public InspectorCaseController caseController;

    [Header("Voting Zones")]
    public Collider voteZoneA;
    public Collider voteZoneB;

    [Header("XR Player (Quest Camera Rig)")]
    public Transform xrRig; // XR Origin OR Main Camera

    [Header("Voting Settings")]
    public float countdownTime = 5f;

    // -----------------------------
    // DEBUG (VISIBLE IN INSPECTOR)
    // -----------------------------
    [Header("DEBUG STATUS")]
    public bool debugStartVote;
    public bool votingInProgress;
    public bool playerInZoneA;
    public bool playerInZoneB;

    private bool lastDebugToggle;

    // ------------------------------------------------
    void Update()
    {
        // --- LIVE POSITION CHECK (VR SAFE) ---
        CheckPlayerZone();

        // Inspector button toggle
        if (debugStartVote && !lastDebugToggle)
        {
            debugStartVote = false;
            StartVote();
        }

        lastDebugToggle = debugStartVote;
    }

    // ------------------------------------------------
    // CHECK WHICH ZONE PLAYER IS IN
    // ------------------------------------------------
    void CheckPlayerZone()
    {
        if (xrRig == null)
            return;

        Vector3 playerPosition = xrRig.position;

        playerInZoneA = voteZoneA.bounds.Contains(playerPosition);
        playerInZoneB = voteZoneB.bounds.Contains(playerPosition);
    }

    // ------------------------------------------------
    // START VOTE
    // ------------------------------------------------
    public void StartVote()
    {
        if (votingInProgress)
        {
            Debug.Log("Vote already running.");
            return;
        }

        Debug.Log("START VOTE CALLED");
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
        Debug.Log($"Decision Check | A:{playerInZoneA}  B:{playerInZoneB}");

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
            Debug.LogWarning("No valid vote (none or both zones)");
        }
    }
}