using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float fieldOfViewMax = 50f;
    [SerializeField] private float fieldOfViewMin = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 100f;
    [SerializeField] private float zoomSpeed = 10f;

    private float targetFieldOfView;

    void Start()
    {
        targetFieldOfView = cinemachineCamera.Lens.FieldOfView;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 inputDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) inputDirection.z += 1f;
        if (Keyboard.current.sKey.isPressed) inputDirection.z -= 1f;
        if (Keyboard.current.aKey.isPressed) inputDirection.x -= 1f;
        if (Keyboard.current.dKey.isPressed) inputDirection.x += 1f;

        Vector3 moveDirection =
            transform.forward * inputDirection.z +
            transform.right * inputDirection.x;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void HandleRotation()
    {
        float rotateDir = 0f;

        if (Keyboard.current.qKey.isPressed) rotateDir += 1f;
        if (Keyboard.current.eKey.isPressed) rotateDir -= 1f;

        transform.eulerAngles +=
            new Vector3(0f, rotateDir * rotateSpeed * Time.deltaTime, 0f);
    }

    void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0) targetFieldOfView -= 5f;
        if (scroll < 0) targetFieldOfView += 5f;

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, fieldOfViewMin, fieldOfViewMax);

        var lens = cinemachineCamera.Lens;

        lens.FieldOfView = Mathf.Lerp(
            lens.FieldOfView,
            targetFieldOfView,
            Time.deltaTime * zoomSpeed
        );

        cinemachineCamera.Lens = lens;
    }
}
