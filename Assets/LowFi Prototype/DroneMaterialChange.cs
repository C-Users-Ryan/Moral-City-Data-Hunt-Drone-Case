using UnityEngine;
using System.Collections.Generic;

public class DroneMaterialChange : MonoBehaviour
{
    [Header("Objects to Update")]
    [SerializeField] private List<Renderer> targets = new List<Renderer>();

    [Header("Materials")]
    [SerializeField] private Material material1;
    [SerializeField] private Material material2;

    /// <summary>
    /// Applies Material 1 to all target objects.
    /// </summary>
    public void SetMaterial1()
    {
        if (material1 == null)
        {
            Debug.LogWarning("Material 1 is not assigned.");
            return;
        }

        ApplyMaterial(material1);
    }

    /// <summary>
    /// Applies Material 2 to all target objects.
    /// </summary>
    public void SetMaterial2()
    {
        if (material2 == null)
        {
            Debug.LogWarning("Material 2 is not assigned.");
            return;
        }

        ApplyMaterial(material2);
    }

    /// <summary>
    /// Helper function to swap materials on all renderers.
    /// </summary>
    private void ApplyMaterial(Material mat)
    {
        foreach (var r in targets)
        {
            if (r != null)
                r.material = mat;
        }
    }
}
