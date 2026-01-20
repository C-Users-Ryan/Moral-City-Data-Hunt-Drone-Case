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

        [Header("Case Start")]
        public bool startCase;
        public List<GameObject> startEnableObjects;

        [Header("Options")]
        public CaseOption optionA;
        public CaseOption optionB;

        [HideInInspector] public bool startApplied;
    }

    [Header("Cases")]
    public List<Case> cases = new List<Case>();

    // -------------------------------
    // UNITY
    // -------------------------------

    void Update()
    {
        HandleGlobalStart();

        foreach (var c in cases)
        {
            HandleCase(c);
        }
    }

    // -------------------------------
    // GLOBAL START LOGIC
    // -------------------------------

    void HandleGlobalStart()
    {
        if (!start || startApplied) return;

        foreach (var obj in startEnableObjects)
            if (obj) obj.SetActive(true);

        startApplied = true;
    }

    // -------------------------------
    // CASE LOGIC
    // -------------------------------

    void HandleCase(Case c)
    {
        if (!c.startCase) return;

        // Apply case start objects ONCE
        if (!c.startApplied)
        {
            foreach (var obj in c.startEnableObjects)
                if (obj) obj.SetActive(true);

            c.startApplied = true;
        }

        // Enforce single option active
        if (c.optionA.active && c.optionB.active)
            c.optionB.active = false;

        // Reset apply state for switching
        if (c.optionA.active)
            c.optionB.wasApplied = false;

        if (c.optionB.active)
            c.optionA.wasApplied = false;

        // Apply option logic
        ApplyOption(c.optionA);
        ApplyOption(c.optionB);
    }

    void ApplyOption(CaseOption option)
    {
        // Only apply once when option becomes active
        if (!option.active || option.wasApplied)
            return;

        foreach (var obj in option.enableObjects)
            if (obj) obj.SetActive(true);

        foreach (var obj in option.disableObjects)
            if (obj) obj.SetActive(false);

        option.wasApplied = true;
    }
}
