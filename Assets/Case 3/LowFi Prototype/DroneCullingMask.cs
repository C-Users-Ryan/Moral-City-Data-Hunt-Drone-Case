using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

public class DroneCullingMask : MonoBehaviour
{
    [Header("Cameras & Layers")]
    public Camera xrCamera;  // assign CenterEyeAnchor
    public LayerMask hiddenWorldLayer;
    public LayerMask voidObjectsLayer;

    [Header("Quad overlay (fallback)")]
    public Renderer overlayRenderer;

    [Header("Timing")]
    public float fallbackFadeDuration = 1.5f;

    [Header("Inspector test toggles (Play mode)")]
    public bool triggerReveal = false;
    public bool triggerHide = false;

    private bool isFading = false;
    private bool isRevealed = false;
    public bool audiostarted = false;
    public AudioSource audioSource;
    public GameObject target;

    void Awake()
    {
        if (xrCamera == null) xrCamera = Camera.main;

        // Start with world hidden, only show void objects
        SetOnlyLayersOnAllCameras(voidObjectsLayer);

        // Fully opaque overlay at start
        if (overlayRenderer != null)
        {
            Color c = overlayRenderer.material.color;
            overlayRenderer.material.color = new Color(c.r, c.g, c.b, 1f);
            overlayRenderer.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (triggerReveal)
        {
            triggerReveal = false;
            RevealWorld();
        }

        if (triggerHide)
        {
            triggerHide = false;
            HideWorld();
        }
    }

    public void RevealWorld()
    {
        if (!isFading && !isRevealed)
        {
            StartCoroutine(RevealCoroutine());
            if (audiostarted == false)
            {
                audiostarted = true;
                audioSource.Play();
                Destroy(target);
            }
        }

    }

    public void HideWorld()
    {
        if (!isFading && isRevealed)
            StartCoroutine(HideCoroutine());
    }

    IEnumerator RevealCoroutine()
    {
        isFading = true;

        // Enable hidden world layers immediately
        AddLayerToAllCameras(hiddenWorldLayer);

        // Fade overlay from alpha=1 -> 0
        if (overlayRenderer != null)
            yield return StartCoroutine(FadeOverlayAlpha(1f, 0f, fallbackFadeDuration));

        isFading = false;
        isRevealed = true;
    }

    IEnumerator HideCoroutine()
    {
        isFading = true;

        // Fade overlay from alpha=0 -> 1
        if (overlayRenderer != null)
            yield return StartCoroutine(FadeOverlayAlpha(0f, 1f, fallbackFadeDuration));

        // After fade to black, hide the world layers again
        SetOnlyLayersOnAllCameras(voidObjectsLayer);

        isFading = false;
        isRevealed = false;
    }

    IEnumerator FadeOverlayAlpha(float startA, float endA, float duration)
    {
        if (overlayRenderer == null) yield break;

        overlayRenderer.gameObject.SetActive(true);
        Material mat = overlayRenderer.material;
        Color col = mat.color;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startA, endA, t / duration);
            mat.color = new Color(col.r, col.g, col.b, alpha);
            yield return null;
        }

        mat.color = new Color(col.r, col.g, col.b, endA);

        // disable quad if fully transparent
        if (Mathf.Approximately(endA, 0f))
            overlayRenderer.gameObject.SetActive(false);
    }

    // Camera layer helpers
    void SetOnlyLayersOnAllCameras(LayerMask mask)
    {
        int m = mask.value;
        Camera[] cams = Camera.allCameras;
        foreach (Camera cam in cams)
            cam.cullingMask = m;
    }

    void AddLayerToAllCameras(LayerMask maskToAdd)
    {
        int add = maskToAdd.value;
        Camera[] cams = Camera.allCameras;
        foreach (Camera cam in cams)
            cam.cullingMask |= add;
    }
}
