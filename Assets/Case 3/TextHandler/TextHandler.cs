using UnityEngine;

public class TextHandler : MonoBehaviour
{
    [SerializeField] private Transform centerEye;

    void Start()
    {
        // Auto-find Quest headset camera
        if (centerEye == null)
        {
            var eye = GameObject.Find("CenterEyeAnchor");

            if (eye != null)
                centerEye = eye.transform;
            else
                Debug.LogWarning("CenterEyeAnchor not found!");
        }
    }

    void LateUpdate()
    {
        if (centerEye == null) return;

        // Direction from text → headset
        Vector3 direction = transform.position - centerEye.position;

        // Face the player
        transform.rotation = Quaternion.LookRotation(direction);
    }
}