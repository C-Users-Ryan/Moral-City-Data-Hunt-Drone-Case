using UnityEngine;

public class TextHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    void Start()
    {
        // If no camera is assigned, try to find the main camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // Ensure the canvas is always facing the camera
        if (mainCamera != null)
        {
            // Look at the camera's position
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
