using System.Collections.Generic;
using UnityEngine;

public class InspectorCaseController : MonoBehaviour
{
    // -------------------------------
    // GLOBAL START
    // -------------------------------

    [Header("Global Start")]
    public bool start;
    public List<GameObject> startEnableObjects;

    [Header("Restart Settings")]
    public List<GameObject> objectsToDisableOnRestart; // objects to deactivate on restart
    public bool loopCases = true; // automatically loop back to case 1 after last case

    private bool startApplied = false;

    // -------------------------------
    // CASE DATA
    // -------------------------------

    [System.Serializable]
    public class CaseOption
    {
        public string optionName;
        public bool active;

        [Header("Enable These Objects")]
        public List<GameObject> enableObjects;

        [Header("Disable These Objects")]
        public List<GameObject> disableObjects;

        [HideInInspector] public bool wasApplied;
    }

    [System.Serializable]
    public class Case
    {
        [Header("Case Info")]
        public string caseName;

        [Header("Case Start Objects")]
        public List<GameObject> startEnableObjects;

        [Header("Options")]
        public CaseOption optionA;
        public CaseOption optionB;

        [HideInInspector] public bool wasEntered;
        [HideInInspector] public bool optionChosen; // prevents choosing twice
    }

    [Header("Cases (Stages)")]
    public List<Case> cases = new List<Case>();

    // -------------------------------
    // STAGE CONTROL
    // -------------------------------

    private int currentCaseIndex = -1;

    void Start()
    {
        ApplyGlobalStart();
    }

    // -------------------------------
    // BUTTON / API FUNCTIONS
    // -------------------------------

    /// <summary>
    /// Move to next case (hook to Next button)
    /// </summary>
    public void AdvanceCase()
    {
        if (cases.Count == 0) return;

        currentCaseIndex++;

        // Check if we exceeded the last case
        if (currentCaseIndex >= cases.Count)
        {
            if (loopCases)
            {
                Debug.Log("Looping cases: restarting all cases");
                RestartAllCases();
            }
            else
            {
                Debug.Log("No more cases available.");
                return;
            }
        }

        EnterCase(cases[currentCaseIndex]);
    }

    /// <summary>
    /// UI Button function
    /// 0 = Option A
    /// 1 = Option B
    /// </summary>
    public void SelectOption(int optionIndex)
    {
        if (currentCaseIndex < 0 || currentCaseIndex >= cases.Count)
        {
            Debug.LogWarning("No active case.");
            return;
        }

        Case currentCase = cases[currentCaseIndex];

        if (currentCase.optionChosen)
        {
            Debug.Log("Option already chosen for this case.");
            return;
        }

        switch (optionIndex)
        {
            case 0:
                ApplyOption(currentCase, true);
                break;

            case 1:
                ApplyOption(currentCase, false);
                break;

            default:
                Debug.LogWarning("Invalid option index.");
                return;
        }

        currentCase.optionChosen = true;
    }

    // -------------------------------
    // GLOBAL START
    // -------------------------------

    void ApplyGlobalStart()
    {
        if (!start || startApplied) return;

        foreach (var obj in startEnableObjects)
            if (obj) obj.SetActive(true);

        startApplied = true;
    }

    // -------------------------------
    // CASE FLOW
    // -------------------------------

    void EnterCase(Case c)
    {
        if (c == null || c.wasEntered) return;

        foreach (var obj in c.startEnableObjects)
            if (obj) obj.SetActive(true);

        c.wasEntered = true;
    }

    void ApplyOption(Case c, bool optionA)
    {
        if (c == null) return;

        CaseOption chosen = optionA ? c.optionA : c.optionB;
        CaseOption other = optionA ? c.optionB : c.optionA;

        if (chosen == null)
        {
            Debug.LogWarning("Chosen option missing.");
            return;
        }

        if (chosen.wasApplied)
            return;

        // enforce single option state
        chosen.active = true;
        if (other != null)
            other.active = false;

        ApplyOptionEffects(chosen);
    }

    void ApplyOptionEffects(CaseOption option)
    {
        foreach (var obj in option.enableObjects)
            if (obj) obj.SetActive(true);

        foreach (var obj in option.disableObjects)
            if (obj) obj.SetActive(false);

        option.wasApplied = true;
    }

    // -------------------------------
    // RESTART / LOOP FUNCTIONS
    // -------------------------------

    /// <summary>
    /// Fully restart all cases and options
    /// </summary>
    public void RestartAllCases()
    {
        Debug.Log("Restarting all cases...");

        // Reset each case and option
        foreach (var c in cases)
        {
            c.wasEntered = false;
            c.optionChosen = false;

            if (c.optionA != null)
                c.optionA.wasApplied = false;

            if (c.optionB != null)
                c.optionB.wasApplied = false;
        }

        // Deactivate objects that were enabled during gameplay
        foreach (var obj in objectsToDisableOnRestart)
            if (obj) obj.SetActive(false);

        // Reset current case index
        currentCaseIndex = 0;

        // Enter first case
        EnterCase(cases[currentCaseIndex]);
    }
}