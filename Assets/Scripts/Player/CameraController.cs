using UnityEngine;
using UnityEngine.InputSystem; // Sử dụng hệ thống Input mới

public class CameraController : MonoBehaviour
{
    [Header("--- PC Settings ---")]
    public float panSpeed = 20f;
    public float zoomSpeed = 20f;

    [Header("--- Mobile Settings ---")]
    public float touchPanSpeed = 0.5f;   // Tốc độ vuốt ngón tay (Nên để nhỏ vì delta đo bằng pixel)
    public float touchZoomSpeed = 0.1f;  // Tốc độ nhúm 2 ngón tay để zoom

    [Header("Camera Rotation Bounds")]
    public float minX = -10f;
    public float maxX = 50f;
    public float minZ = -20f;
    public float maxZ = 20f;

    [Header("Camera FOV Bounds")]
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
        Vector3 pos = transform.position;

        // ==========================================
        // 1. ĐIỀU KHIỂN TRÊN PC (Phím WASD / Mũi tên)
        // ==========================================
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) pos.z += panSpeed * Time.deltaTime;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) pos.z -= panSpeed * Time.deltaTime;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) pos.x += panSpeed * Time.deltaTime;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) pos.x -= panSpeed * Time.deltaTime;
        }

        // ==========================================
        // 2. ĐIỀU KHIỂN TRÊN MOBILE (Vuốt 1 ngón tay)
        // ==========================================
        if (Touchscreen.current != null)
        {
            // KỂM TRA ĐÈN ĐỎ: Nếu đang kéo tháp để Fuse thì KHÔNG CHO phép di chuyển bản đồ
            bool isDraggingTower = TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.isDraggingForFuse;

            // Nếu màn hình nhận 1 ngón tay, đang di chuyển, không Zoom và KHÔNG KÉO THÁP
            if (!isDraggingTower && Touchscreen.current.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && !IsPinching())
            {
                Vector2 delta = Touchscreen.current.touches[0].delta.ReadValue();
                
                pos.x -= delta.x * touchPanSpeed * Time.deltaTime;
                pos.z -= delta.y * touchPanSpeed * Time.deltaTime;
            }
        }

        // Khóa giới hạn bản đồ
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        transform.position = pos;
    }

    void HandleZoom()
    {
        // ==========================================
        // 1. ZOOM TRÊN PC (Con lăn chuột)
        // ==========================================
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0)
            {
                float zoomAmount = scroll * zoomSpeed * Time.deltaTime;
                float targetFOV = cam.fieldOfView - zoomAmount;
                cam.fieldOfView = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            }
        }

        // ==========================================
        // 2. ZOOM TRÊN MOBILE (Pinch bằng 2 ngón tay)
        // ==========================================
        if (IsPinching())
        {
            var touchZero = Touchscreen.current.touches[0];
            var touchOne = Touchscreen.current.touches[1];

            // Lấy vị trí hiện tại của 2 ngón tay
            Vector2 touchZeroPos = touchZero.position.ReadValue();
            Vector2 touchOnePos = touchOne.position.ReadValue();

            // Tính vị trí cũ của 2 ngón tay (ở frame trước)
            Vector2 touchZeroPrevPos = touchZeroPos - touchZero.delta.ReadValue();
            Vector2 touchOnePrevPos = touchOnePos - touchOne.delta.ReadValue();

            // Tính khoảng cách giữa 2 ngón tay: Cũ và Mới
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZeroPos - touchOnePos).magnitude;

            // Tính sự chênh lệch (nếu số Dương là đang bung tay ra, số Âm là đang kẹp tay lại)
            float difference = currentMagnitude - prevMagnitude;

            // Áp dụng vào FOV của Camera
            float targetFOV = cam.fieldOfView - (difference * touchZoomSpeed);
            cam.fieldOfView = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }
    }

    // Hàm bổ trợ kiểm tra xem có đúng 2 ngón tay đang thao tác không
    private bool IsPinching()
    {
        if (Touchscreen.current == null) return false;

        // Nếu ngón số 1 và ngón số 2 đều đang chạm trên màn hình
        var touch0 = Touchscreen.current.touches[0].phase.ReadValue();
        var touch1 = Touchscreen.current.touches[1].phase.ReadValue();

        bool isTouch0Active = touch0 == UnityEngine.InputSystem.TouchPhase.Began || touch0 == UnityEngine.InputSystem.TouchPhase.Moved || touch0 == UnityEngine.InputSystem.TouchPhase.Stationary;
        bool isTouch1Active = touch1 == UnityEngine.InputSystem.TouchPhase.Began || touch1 == UnityEngine.InputSystem.TouchPhase.Moved || touch1 == UnityEngine.InputSystem.TouchPhase.Stationary;

        return isTouch0Active && isTouch1Active;
    }
}