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
    // BUTTON API
    // -------------------------------

    public void AdvanceCase()
    {
        if (currentCaseIndex >= cases.Count - 1)
            return;

        currentCaseIndex++;
        EnterCase(cases[currentCaseIndex]);
    }

    public void SelectOptionA()
    {
        ApplyOption(cases[currentCaseIndex], true);
    }

    public void SelectOptionB()
    {
        ApplyOption(cases[currentCaseIndex], false);
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
        if (c.wasEntered) return;

        foreach (var obj in c.startEnableObjects)
            if (obj) obj.SetActive(true);

        c.wasEntered = true;
    }

    void ApplyOption(Case c, bool optionA)
    {
        CaseOption chosen = optionA ? c.optionA : c.optionB;
        CaseOption other = optionA ? c.optionB : c.optionA;

        // rigid: only once
        if (chosen.wasApplied)
            return;

        // enforce single option
        other.active = false;
        chosen.active = true;

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
}
