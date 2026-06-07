using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;
public class CameraController : MonoBehaviour
{
    [Header("Camera Movement Speed")]
    public float panSpeed = 20f;

    [Header("Camera Rotation Speed")]
    public float minX = -10f;
    public float maxX = 50f;
    public float minZ = -20f;
    public float maxZ = 20f;

    [Header("Camera Zoom Speed")]
    public float zoomSpeed = 20f;
    public float minFOV = 20f;
    public float maxFOV = 70f;

    private Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        if (Keyboard.current == null) return;

        Vector3 pos = transform.position;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) pos.z += panSpeed * Time.deltaTime;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) pos.z -= panSpeed * Time.deltaTime;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) pos.x += panSpeed * Time.deltaTime;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) pos.x -= panSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        transform.position = pos;
    }

    void HandleZoom()
    {
        if (Mouse.current == null) return;
        float scroll = Mouse.current.scroll.ReadValue().y;
        
        if (scroll != 0)
        {
            float zoomAmount = scroll * zoomSpeed * Time.deltaTime;
            
            float targetFOV = cam.fieldOfView - zoomAmount;

            cam.fieldOfView = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }
    }
}
