using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps; // Bắt buộc phải có dòng này để gọi Tilemap

public class TowerPlacementManager : MonoBehaviour
{
    [Header("Indicator Visuals")]
    public GameObject indicatorRoot;
    public Renderer indicatorRenderer;
    public Material validMaterial;
    public Material invalidMaterial;

    [Header("Tower Prefab")]
    public GameObject towerPrefab;

    [Header("Bản đồ Đường đi")]
    [Tooltip("Kéo GameObject 'Way' từ Hierarchy thả vào đây")]
    public Tilemap roadTilemap;

    private Camera mainCamera;
    private Plane groundPlane;
    private bool canPlaceAtCurrentPosition = false;

    void Start()
    {
        mainCamera = Camera.main;

        // Tạo mặt phẳng ảo nằm ngang ở độ cao Y=0 để hứng con chuột (Không cần Collider)
        groundPlane = new Plane(Vector3.up, Vector3.zero);

        indicatorRoot.SetActive(true);
    }

    void Update()
    {
        if (Mouse.current == null || !indicatorRoot.activeSelf) return;

        MoveIndicatorAndValidatePosition();

        if (Input.GetMouseButtonDown(0))
        {
            if (canPlaceAtCurrentPosition)
            {
                BuildTower();
            }
            else
            {
                Debug.Log("<color=red>LỖI: Không thể đặt đè lên đường đi của quái!</color>");
            }
        }
    }

    void MoveIndicatorAndValidatePosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (groundPlane.Raycast(ray, out float enterDistance))
        {
            // 1. Lấy tọa độ chuột trên mặt phẳng
            Vector3 rawPoint = ray.GetPoint(enterDistance);

            // 2. Chuyển tọa độ thế giới thành tọa độ Ô LƯỚI (Cell) của Tilemap
            Vector3Int cellPosition = roadTilemap.WorldToCell(rawPoint);

            // 3. Lấy chính xác TÂM của ô lưới đó để đặt Khung ngắm (Khỏi cần tính toán Snap)
            Vector3 snappedPoint = roadTilemap.GetCellCenterWorld(cellPosition);
            snappedPoint.y = 0.05f; // Nhích nhẹ lên để không chìm dưới đất

            indicatorRoot.transform.position = snappedPoint;

            // 4. HỎI TILEMAP: "Ô này có chứa viên gạch đường đi nào không?"
            bool isOverlappingRoad = roadTilemap.HasTile(cellPosition);

            // Nếu KHÔNG có gạch đường đi -> Hợp lệ (True)
            UpdateIndicatorStatus(!isOverlappingRoad);
        }
    }

    void UpdateIndicatorStatus(bool isValid)
    {
        canPlaceAtCurrentPosition = isValid;
        indicatorRenderer.material = isValid ? validMaterial : invalidMaterial;
    }

    void BuildTower()
    {
        Instantiate(towerPrefab, indicatorRoot.transform.position + Vector3.up * 0.15f, Quaternion.identity);
        Debug.Log("🎯 Đã xây tháp chuẩn xác vào tâm ô cỏ!");
    }
}