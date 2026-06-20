using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance;

    public enum PlacementMode { Build, Delete }
    private PlacementMode currentMode = PlacementMode.Build;

    [Header("Selection A (Khung ngắm di chuyển)")]
    public GameObject indicatorRoot;
    public Renderer indicatorRenderer;
    public Material invalidMaterial;
    public Material deleteMaterial;
    public Material fuseMaterial;      // Vật liệu màu Tím khi kéo tháp đi ghép

    [Header("Selection B (Hiệu ứng khi đặt xong)")]
    public GameObject buildSuccessPrefab;
    public float effectDuration = 0.5f;

    [Header("Giao diện UI Xác nhận xóa")]
    public GameObject confirmationPanel;

    [Header("Bản đồ Đường đi")]
    public Tilemap[] obstacleTilemaps;

    [Header("Shop Tower")]
    public GameObject[] availableTowers;
    public int[] towerPrices;
    private GameObject towerToBuild;
    private int selectedTowerIndex = 0;

    private Camera mainCamera;
    private Plane groundPlane;
    private bool isActionValid = false;
    private Vector3Int currentCellPosition;

    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, int> placedTowerCosts = new Dictionary<Vector3Int, int>();

    // CÁC BIẾN QUẢN LÝ KÉO - THẢ ĐỂ GHÉP (DRAG & DROP FUSE)
    private Vector3Int firstFuseCell;
    private bool isDraggingForFuse = false;

    private Material originalMaterial;
    private bool isDisplayingSuccessEffect = false;
    private bool isWaitingForConfirmation = false;
    private Vector3Int cellToTargetDelete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (availableTowers != null && availableTowers.Length > 0) towerToBuild = availableTowers[0];

        if (indicatorRenderer != null) originalMaterial = indicatorRenderer.material;
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f || isDisplayingSuccessEffect) return;

        if (isWaitingForConfirmation)
        {
            if (indicatorRoot.activeSelf) indicatorRoot.SetActive(false);
            return;
        }

        // Khi đang kéo tháp thì không cho UI chặn chuột để trải nghiệm mượt nhất
        if (!isDraggingForFuse && EventSystem.current.IsPointerOverGameObject())
        {
            indicatorRoot.SetActive(false);
            return;
        }
        else indicatorRoot.SetActive(true);

        // 1. Luôn cập nhật vị trí ô đất theo thời gian thực
        MoveIndicatorAndValidatePosition();

        // 2. ĐÈ CHUỘT XUỐNG: Ăn ngay lập tức không thông qua bộ lọc kiểm tra vị trí nữa
        if (Input.GetMouseButtonDown(0))
        {
            if (currentMode == PlacementMode.Build)
            {
                // Nếu ô này ĐÃ CÓ THÁP -> Nhấc đi kéo ngay lập tức!
                if (placedTowers.ContainsKey(currentCellPosition))
                {
                    isDraggingForFuse = true;
                    firstFuseCell = currentCellPosition;
                    isActionValid = false; // Tạm thời khóa để tính toán lại khi di chuyển
                    UpdateIndicatorStatus(fuseMaterial);
                }
                // Nếu ô TRỐNG và vị trí hợp lệ -> Tiến hành xây tháp
                else if (isActionValid)
                {
                    BuildTower();
                }
            }
            else if (currentMode == PlacementMode.Delete && placedTowers.ContainsKey(currentCellPosition))
            {
                OpenConfirmationDialog();
            }
        }

        // 3. THẢ CHUỘT TRÁI RA: Tiến hành dung hợp
        if (Input.GetMouseButtonUp(0) && isDraggingForFuse)
        {
            isDraggingForFuse = false;

            if (isActionValid)
            {
                ExecuteDragAndDropFuse();
            }

            // Ép cập nhật lại khung ngắm về trạng thái bình thường ngay sau khi thả
            MoveIndicatorAndValidatePosition();
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

                // TỐI ƯU HÓA LOGIC KHI ĐANG RÊ CHUỘT GHÉP THÁP
                if (isDraggingForFuse)
                {
                    // Chỉ hợp lệ khi trỏ vào ô CÓ THÁP và KHÔNG PHẢI chính nó
                    if (hasTower && currentCellPosition != firstFuseCell)
                    {
                        GameObject tower1 = placedTowers[firstFuseCell];
                        GameObject tower2 = placedTowers[currentCellPosition];

                        if (tower1 != null && tower2 != null)
                        {
                            TowerStats stats1 = tower1.GetComponent<TowerStats>();
                            TowerStats stats2 = tower2.GetComponent<TowerStats>();

                            // Phải trùng khớp danh tính và cấp độ
                            if (stats1 != null && stats2 != null && stats1.towerType == stats2.towerType && stats1.towerLevel == stats2.towerLevel)
                            {
                                isActionValid = true;
                                UpdateIndicatorStatus(fuseMaterial);
                                return;
                            }
                        }
                    }

                    // Thả bậy bạ ra đất trống hoặc tháp khác loại -> Khóa hành động + Báo đỏ
                    isActionValid = false;
                    UpdateIndicatorStatus(invalidMaterial);
                }
                // LOGIC KHI Ở CHẾ ĐỘ XÂY DỰNG BÌNH THƯỜNG
                else if (currentMode == PlacementMode.Build)
                {
                    int currentPrice = towerPrices[selectedTowerIndex];
                    bool hasEnoughMoney = EconomyManager.Instance.currentMoney >= currentPrice;

                    isActionValid = !isOverlappingRoad && !hasTower && hasEnoughMoney;
                    UpdateIndicatorStatus(isActionValid ? originalMaterial : invalidMaterial);
                }
                // LOGIC KHI Ở CHẾ ĐỘ XÓA THÁP
                else if (currentMode == PlacementMode.Delete)
                {
                    isActionValid = hasTower;
                    UpdateIndicatorStatus(isActionValid ? deleteMaterial : invalidMaterial);
                }
            }
        }
    }

    void ExecuteDragAndDropFuse()
    {
        GameObject tower1 = placedTowers[firstFuseCell];
        GameObject tower2 = placedTowers[currentCellPosition];

        TowerStats stats2 = tower2.GetComponent<TowerStats>();
        if (stats2 != null)
        {
            stats2.UpgradeTower();

            placedTowerCosts[currentCellPosition] += placedTowerCosts[firstFuseCell];

            if (buildSuccessPrefab != null)
            {
                // ĐÃ SỬA LỖI: Bắt hệ thống phải xóa cái vòng/hiệu ứng này đi sau 0.5 giây
                GameObject successEffect = Instantiate(buildSuccessPrefab, tower2.transform.position, Quaternion.identity);
                Destroy(successEffect, effectDuration);
            }

            Destroy(tower1);
            placedTowers.Remove(firstFuseCell);
            placedTowerCosts.Remove(firstFuseCell);

            Debug.Log("<color=green><b>[Fuse] Ghép tháp Kéo - Thả thành công tốt đẹp!</b></color>");
        }
    }

    void BuildTower()
    {
        if (towerToBuild != null)
        {
            int currentPrice = towerPrices[selectedTowerIndex];
            if (!EconomyManager.Instance.SpendMoney(currentPrice)) return;

            GameObject newTower = Instantiate(towerToBuild, indicatorRoot.transform.position + Vector3.up * 0.15f, Quaternion.identity);

            if (buildSuccessPrefab != null)
            {
                Vector3 effectPos = indicatorRoot.transform.position + Vector3.up * 0.1f;
                GameObject successEffect = Instantiate(buildSuccessPrefab, effectPos, Quaternion.identity);
                Destroy(successEffect, effectDuration);
            }

            placedTowers.Add(currentCellPosition, newTower);
            placedTowerCosts.Add(currentCellPosition, currentPrice);

            StartCoroutine(SelectionToggleRoutine());
        }
    }

    void OpenConfirmationDialog()
    {
        cellToTargetDelete = currentCellPosition;
        isWaitingForConfirmation = true;
        indicatorRoot.SetActive(false);

        if (confirmationPanel != null) confirmationPanel.SetActive(true);
    }

    public void ConfirmDeletion()
    {
        if (placedTowers.ContainsKey(cellToTargetDelete))
        {
            if (placedTowerCosts.ContainsKey(cellToTargetDelete))
            {
                int refundAmount = placedTowerCosts[cellToTargetDelete] / 2;
                EconomyManager.Instance.AddMoney(refundAmount);
                placedTowerCosts.Remove(cellToTargetDelete);
            }

            Destroy(placedTowers[cellToTargetDelete]);
            placedTowers.Remove(cellToTargetDelete);
        }
        CloseConfirmationDialog();
    }

    public void CancelDeletion()
    {
        CloseConfirmationDialog();
    }

    void CloseConfirmationDialog()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        isWaitingForConfirmation = false;
    }

    public void SetPlacementMode(bool isDeleteMode)
    {
        currentMode = isDeleteMode ? PlacementMode.Delete : PlacementMode.Build;
    }

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
        if (indicatorRenderer != null) indicatorRenderer.material = mat;
    }

    public void RemoveTowerFromGrid(GameObject towerobj)
    {
        Vector3Int keyToRemove = Vector3Int.zero;
        bool found = false;

        foreach (var kvp in placedTowers)
        {
            if (kvp.Value == towerobj)
            {
                keyToRemove = kvp.Key;
                found = true;
                break;
            }
        }
        if (found)
        {
            placedTowers.Remove(keyToRemove);
            if (placedTowerCosts.ContainsKey(keyToRemove)) placedTowerCosts.Remove(keyToRemove);
        }
    }

    public void SelectTowerToBuild(int towerIndex)
    {
        currentMode = PlacementMode.Build;
        if (towerIndex >= 0 && towerIndex < availableTowers.Length)
        {
            towerToBuild = availableTowers[towerIndex];
            selectedTowerIndex = towerIndex;
        }
    }
}