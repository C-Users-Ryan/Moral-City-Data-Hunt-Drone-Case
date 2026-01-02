using UnityEngine;
using System.Collections.Generic;

public class DroneMaterialChange : MonoBehaviour
{
    [Header("Objects to Update")]
    [SerializeField] private List<Renderer> targets = new List<Renderer>();

    [Header("Materials")]
    [SerializeField] private Material material1;
    [SerializeField] private Material material2;

    [Header("Debug Toggles")]
    [SerializeField] private bool useMaterial1;
    [SerializeField] private bool useMaterial2;

    void Update()
    {
        if (useMaterial1)
        {
            SetMaterial1();
            useMaterial1 = false;
        }

        if (useMaterial2)
        {
            SetMaterial2();
            useMaterial2 = false;
        }
    }

    public void SetMaterial1()
    {
        if (material1 == null)
        {
            Debug.LogWarning("Material 1 is not assigned.");
            return;
        }

        ApplyMaterial(material1);
    }

    public void SetMaterial2()
    {
        if (material2 == null)
        {
            Debug.LogWarning("Material 2 is not assigned.");
            return;
        }

        ApplyMaterial(material2);
    }

    private void ApplyMaterial(Material mat)
    {
        foreach (var r in targets)
        {
            if (r != null)
                r.material = mat;
        }
    }
}
