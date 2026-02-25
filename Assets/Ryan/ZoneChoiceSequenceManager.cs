using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneBasedChoiceSequence : MonoBehaviour
{
    // -------------------------------
    // PLAYER & START
    // -------------------------------

    [Header("Player")]
    public Transform playerHead;

    [Header("Start Trigger")]
    public Collider startTrigger;

    [Header("Object To Disable On Start")]
    public GameObject startingObject;

    // -------------------------------
    // ZONES
    // -------------------------------

    [Header("Zones (Local Space)")]
    public Bounds zoneALocal;
    public Bounds zoneBLocal;

    public enum ZoneResult { None, ZoneA, ZoneB }
    public ZoneResult CurrentZone { get; private set; }

    private Vector3 headsetStartWorld;

    // -------------------------------
    // SEQUENCE DATA
    // -------------------------------

    [System.Serializable]
    public class ChoiceObject
    {
        [Header("Voting Platform")]
        public GameObject choiceVisual;

        [Tooltip("BoxCollider that defines the voting area")]
        public BoxCollider votingBox;

        [Header("Materials")]
        public Material zoneAMaterial;
        public Material zoneBMaterial;

        [HideInInspector] public bool selected;
        [HideInInspector] public float stayTimer;
    }

    [System.Serializable]
    public class SequenceStep
    {
        [Header("Activation")]
        public List<GameObject> activateOnStart;

        [Header("Voting")]
        public float requiredStandTime = 3f;
        public float postVoteLockout = 2f;

        [Header("Choices")]
        public List<ChoiceObject> choices;

        [Header("After Choice")]
        public List<GameObject> revealAfterChoice;
        public List<GameObject> hideAfterChoice;
        public float waitAfterChoice = 2f;
    }

    [Header("Sequence")]
    public List<SequenceStep> steps = new List<SequenceStep>();

    // -------------------------------
    // STATE
    // -------------------------------

    private bool sequenceRunning = false;
    private bool waitingForChoice = false;
    private bool votingLocked = false;
    private SequenceStep currentStep;

    // -------------------------------
    // UNITY
    // -------------------------------

    void Update()
    {
        if (!sequenceRunning || votingLocked) return;

        UpdatePlayerZone();

        if (waitingForChoice)
            UpdateVotingPlatforms();
    }

    void OnTriggerEnter(Collider other)
    {
        if (sequenceRunning) return;
        if (other != startTrigger) return;

        if (startingObject)
            startingObject.SetActive(false);

        headsetStartWorld = playerHead.position;
        sequenceRunning = true;

        StartCoroutine(RunSequence());
    }

    // -------------------------------
    // SEQUENCE FLOW
    // -------------------------------

    IEnumerator RunSequence()
    {
        foreach (var step in steps)
            yield return RunStep(step);

        sequenceRunning = false;
    }

    IEnumerator RunStep(SequenceStep step)
    {
        currentStep = step;
        ResetChoices(step);

        foreach (var obj in step.activateOnStart)
            if (obj) obj.SetActive(true);

        waitingForChoice = true;
        yield return new WaitUntil(() => AnyChoiceSelected(step));
        waitingForChoice = false;

        yield return StartCoroutine(PostVoteLockout(step));

        foreach (var obj in step.revealAfterChoice)
            if (obj) obj.SetActive(true);

        foreach (var obj in step.hideAfterChoice)
            if (obj) obj.SetActive(false);

        yield return new WaitForSeconds(step.waitAfterChoice);
    }

    IEnumerator PostVoteLockout(SequenceStep step)
    {
        votingLocked = true;

        foreach (var c in step.choices)
        {
            c.stayTimer = 0f;
            if (c.votingBox)
                c.votingBox.enabled = false;
        }

        yield return new WaitForSeconds(step.postVoteLockout);

        foreach (var c in step.choices)
            if (c.votingBox)
                c.votingBox.enabled = true;

        votingLocked = false;
    }

    void ResetChoices(SequenceStep step)
    {
        foreach (var c in step.choices)
        {
            c.selected = false;
            c.stayTimer = 0f;
        }
    }

    bool AnyChoiceSelected(SequenceStep step)
    {
        foreach (var c in step.choices)
            if (c.selected)
                return true;
        return false;
    }

    // -------------------------------
    // BOX-BASED VOTING LOGIC
    // -------------------------------

    void UpdateVotingPlatforms()
    {
        foreach (var choice in currentStep.choices)
        {
            if (choice.selected) continue;
            if (!choice.votingBox || !choice.votingBox.enabled) continue;

            if (IsPlayerInsideBox(choice.votingBox))
            {
                choice.stayTimer += Time.deltaTime;

                if (choice.stayTimer >= currentStep.requiredStandTime)
                {
                    FinalizeChoice(choice);
                    break;
                }
            }
            else
            {
                choice.stayTimer = 0f;
            }
        }
    }

    bool IsPlayerInsideBox(BoxCollider box)
    {
        // Convert player position into box local space
        Vector3 localPos = box.transform.InverseTransformPoint(playerHead.position);

        Vector3 halfSize = box.size * 0.5f;
        Vector3 center = box.center;

        return
            localPos.x > center.x - halfSize.x &&
            localPos.x < center.x + halfSize.x &&
            localPos.y > center.y - halfSize.y &&
            localPos.y < center.y + halfSize.y &&
            localPos.z > center.z - halfSize.z &&
            localPos.z < center.z + halfSize.z;
    }

    void FinalizeChoice(ChoiceObject choice)
    {
        ApplyZoneMaterial(choice);
        choice.selected = true;
    }

    void ApplyZoneMaterial(ChoiceObject choice)
    {
        if (!choice.choiceVisual) return;

        Renderer r = choice.choiceVisual.GetComponentInChildren<Renderer>();
        if (!r) return;

        switch (CurrentZone)
        {
            case ZoneResult.ZoneA:
                if (choice.zoneAMaterial)
                    r.material = choice.zoneAMaterial;
                break;

            case ZoneResult.ZoneB:
                if (choice.zoneBMaterial)
                    r.material = choice.zoneBMaterial;
                break;
        }
    }

    // -------------------------------
    // ZONE TRACKING
    // -------------------------------

    void UpdatePlayerZone()
    {
        Vector3 playerLocal = playerHead.position - headsetStartWorld;

        if (zoneALocal.Contains(playerLocal))
            CurrentZone = ZoneResult.ZoneA;
        else if (zoneBLocal.Contains(playerLocal))
            CurrentZone = ZoneResult.ZoneB;
        else
            CurrentZone = ZoneResult.None;
    }
}
