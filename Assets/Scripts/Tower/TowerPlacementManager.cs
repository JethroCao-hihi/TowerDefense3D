using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic; // BẮT BUỘC: Để sử dụng cấu trúc Dictionary

public class TowerPlacementManager : MonoBehaviour
{
    // Hệ thống trạng thái (Build: Xây tháp, Delete: Xóa tháp)
    public enum PlacementMode { Build, Delete }
    private PlacementMode currentMode = PlacementMode.Build;

    [Header("Selection A (Khung ngắm di chuyển)")]
    public GameObject indicatorRoot;
    public Renderer indicatorRenderer;
    public Material invalidMaterial;   // Vật liệu Đỏ (Sai vị trí)
    public Material deleteMaterial;    // Vật liệu Cam/Vàng (Chỉ vào tháp cần xóa)

    [Header("Selection B (Hiệu ứng khi đặt xong)")]
    public GameObject buildSuccessPrefab;
    public float effectDuration = 0.5f;

    [Header("Giao diện UI Xác nhận xóa")]
    public GameObject confirmationPanel; // Kéo bảng Panel UI Yes/No vào đây

    [Header("Bản đồ Đường đi")]
    public Tilemap[] obstacleTilemaps;

    [Header("Shop Tower")]
    public GameObject[] availableTowers;
    private GameObject towerToBuild;

    private Camera mainCamera;
    private Plane groundPlane;
    private bool isActionValid = false; // Thay thế cho canPlace cũ, dùng chung cho cả 2 chế độ
    private Vector3Int currentCellPosition;

    // ĐỔI MỚI: Dùng Dictionary để quản lý chính xác GameObject của từng tháp theo tọa độ ô lưới
    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();

    private Material originalMaterial;
    private bool isDisplayingSuccessEffect = false;
    private bool isWaitingForConfirmation = false; // Công tắc đóng băng khi hiện bảng Hỏi xóa
    private Vector3Int cellToTargetDelete;        // Lưu tạm ô đất đang được chọn để xóa

    void Start()
    {
        mainCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (availableTowers != null && availableTowers.Length > 0) towerToBuild = availableTowers[0];

        if (indicatorRenderer != null)
        {
            originalMaterial = indicatorRenderer.material;
        }

        // Ẩn bảng xác nhận xóa khi vừa vào game
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f || isDisplayingSuccessEffect) return;

        // Nếu đang hiện bảng UI hỏi xóa, ẩn khung ngắm đi và khóa chặt Update
        if (isWaitingForConfirmation)
        {
            if (indicatorRoot.activeSelf) indicatorRoot.SetActive(false);
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            indicatorRoot.SetActive(false);
            return;
        }
        else indicatorRoot.SetActive(true);

        MoveIndicatorAndValidatePosition();

        // Nhấp chuột trái để hành động
        if (Input.GetMouseButtonDown(0) && isActionValid)
        {
            if (currentMode == PlacementMode.Build)
            {
                BuildTower();
            }
            else if (currentMode == PlacementMode.Delete)
            {
                OpenConfirmationDialog();
            }
        }
    }

    void MoveIndicatorAndValidatePosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (groundPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 rawPoint = ray.GetPoint(enterDistance);
            if (obstacleTilemaps.Length > 0 && obstacleTilemaps[0] != null)
            {
                currentCellPosition = obstacleTilemaps[0].WorldToCell(rawPoint);
                currentCellPosition.z = 0;

                Vector3 snappedPoint = obstacleTilemaps[0].GetCellCenterWorld(currentCellPosition);
                snappedPoint.y = 0.05f;
                indicatorRoot.transform.position = snappedPoint;

                bool isOverlappingRoad = CheckOverlappingRoad(snappedPoint, rawPoint);
                bool hasTower = placedTowers.ContainsKey(currentCellPosition);

                if (currentMode == PlacementMode.Build)
                {
                    // Chế độ xây: Chỉ hợp lệ khi KHÔNG dính đường VÀ CHƯA có tháp
                    isActionValid = !isOverlappingRoad && !hasTower;
                    UpdateIndicatorStatus(isActionValid ? originalMaterial : invalidMaterial);
                }
                else if (currentMode == PlacementMode.Delete)
                {
                    // Chế độ xóa: Chỉ hợp lệ khi CHỈ ĐÚNG vào ô ĐANG CÓ THÁP
                    isActionValid = hasTower;
                    UpdateIndicatorStatus(isActionValid ? deleteMaterial : invalidMaterial);
                }
            }
        }
    }

    void BuildTower()
    {
        if (towerToBuild != null)
        {
            GameObject newTower = Instantiate(towerToBuild, indicatorRoot.transform.position + Vector3.up * 0.15f, Quaternion.identity);

            if (buildSuccessPrefab != null)
            {
                Vector3 effectPos = indicatorRoot.transform.position + Vector3.up * 0.1f;
                GameObject successEffect = Instantiate(buildSuccessPrefab, effectPos, Quaternion.identity);
                Destroy(successEffect, effectDuration);
            }

            // Lưu tháp mới vào từ điển theo tọa độ ô đất của nó
            placedTowers.Add(currentCellPosition, newTower);

            StartCoroutine(SelectionToggleRoutine());
        }
    }

    // --- LOGIC XỬ LÝ XÓA THÁP ---

    void OpenConfirmationDialog()
    {
        cellToTargetDelete = currentCellPosition; // Ghi nhớ ô đất muốn xóa
        isWaitingForConfirmation = true;          // Bật công tắc đóng băng
        indicatorRoot.SetActive(false);           // Ẩn khung ngắm A đi

        if (confirmationPanel != null) confirmationPanel.SetActive(true); // Hiện bảng UI hỏi Yes/No
    }

    // Hàm này sẽ gắn vào nút "YES" trên giao diện UI
    public void ConfirmDeletion()
    {
        if (placedTowers.ContainsKey(cellToTargetDelete))
        {
            // 1. Cho bốc hơi cục tháp thực tế trên Scene
            Destroy(placedTowers[cellToTargetDelete]);

            // 2. Xóa dữ liệu lưu trữ trong từ điển để ô đất đó trống trải trở lại
            placedTowers.Remove(cellToTargetDelete);

            Debug.Log("<color=yellow><b>Đã xóa tháp thành công!</b></color>");
        }

        CloseConfirmationDialog();
    }

    // Hàm này sẽ gắn vào nút "NO" trên giao diện UI
    public void CancelDeletion()
    {
        Debug.Log("Đã hủy lệnh xóa tháp.");
        CloseConfirmationDialog();
    }

    void CloseConfirmationDialog()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false); // Ẩn bảng hỏi đi
        isWaitingForConfirmation = false; // Mở băng hệ thống
    }

    // Hàm để các nút bấm UI chuyển đổi chế độ chơi (Xây dựng <-> Thùng rác)
    public void SetPlacementMode(bool isDeleteMode)
    {
        currentMode = isDeleteMode ? PlacementMode.Delete : PlacementMode.Build;
        Debug.Log($"Chuyển sang chế độ: {currentMode}");
    }

    // --------------------------------------------------

    IEnumerator SelectionToggleRoutine()
    {
        isDisplayingSuccessEffect = true;
        indicatorRoot.SetActive(false);
        yield return new WaitForSeconds(effectDuration);
        isDisplayingSuccessEffect = false;
    }

    bool CheckOverlappingRoad(Vector3 snappedPoint, Vector3 rawPoint)
    {
        foreach (Tilemap map in obstacleTilemaps)
        {
            if (map == null) continue;
            Vector3Int cellPos = map.WorldToCell(snappedPoint);
            for (int i = -5; i <= 5; i++)
            {
                if (map.HasTile(new Vector3Int(cellPos.x, cellPos.y, i)) || map.HasTile(new Vector3Int(cellPos.x, i, cellPos.z))) return true;
            }
            foreach (Transform child in map.transform)
            {
                if (Vector2.Distance(new Vector2(child.position.x, child.position.z), new Vector2(snappedPoint.x, snappedPoint.z)) < 0.5f) return true;
            }
        }
        return false;
    }

    void UpdateIndicatorStatus(Material mat)
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material = mat;
        }
    }

    public void SelectTowerToBuild(int towerIndex)
    {
        // Khi chọn tháp mới để xây, tự động trả chế độ về Build Mode
        SetPlacementMode(false);
        if (towerIndex >= 0 && towerIndex < availableTowers.Length) towerToBuild = availableTowers[towerIndex];
    }
}