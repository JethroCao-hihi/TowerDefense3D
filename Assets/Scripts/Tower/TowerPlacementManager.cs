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

    // Từ điển lưu Trạng thái Tháp
    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();

    // TỪ ĐIỂN MỚI: Nhớ giá tiền của tháp nằm trên ô đất đó để tính tiền hoàn trả
    private Dictionary<Vector3Int, int> placedTowerCosts = new Dictionary<Vector3Int, int>();

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

        if (EventSystem.current.IsPointerOverGameObject())
        {
            indicatorRoot.SetActive(false);
            return;
        }
        else indicatorRoot.SetActive(true);

        MoveIndicatorAndValidatePosition();

        if (Input.GetMouseButtonDown(0) && isActionValid)
        {
            if (currentMode == PlacementMode.Build) BuildTower();
            else if (currentMode == PlacementMode.Delete) OpenConfirmationDialog();
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
                    int currentPrice = towerPrices[selectedTowerIndex];
                    bool hasEnoughMoney = EconomyManager.Instance.currentMoney >= currentPrice;

                    isActionValid = !isOverlappingRoad && !hasTower && hasEnoughMoney;
                    UpdateIndicatorStatus(isActionValid ? originalMaterial : invalidMaterial);
                }
                else if (currentMode == PlacementMode.Delete)
                {
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
            int currentPrice = towerPrices[selectedTowerIndex];
            EconomyManager.Instance.SpendMoney(currentPrice); // Trừ tiền

            GameObject newTower = Instantiate(towerToBuild, indicatorRoot.transform.position + Vector3.up * 0.15f, Quaternion.identity);

            if (buildSuccessPrefab != null)
            {
                Vector3 effectPos = indicatorRoot.transform.position + Vector3.up * 0.1f;
                GameObject successEffect = Instantiate(buildSuccessPrefab, effectPos, Quaternion.identity);
                Destroy(successEffect, effectDuration);
            }

            placedTowers.Add(currentCellPosition, newTower);

            // Ghi nhớ giá trị của cái tháp này vào từ điển
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
            // === LOGIC HOÀN TIỀN TẠI ĐÂY ===
            if (placedTowerCosts.ContainsKey(cellToTargetDelete))
            {
                // Chia đôi giá trị gốc để ra số tiền hoàn trả (50%)
                int refundAmount = placedTowerCosts[cellToTargetDelete] / 2;

                // Gửi tiền về ngân hàng
                EconomyManager.Instance.AddMoney(refundAmount);
                Debug.Log($"<color=green><b>Đã bán tháp! Thu hồi vốn {refundAmount} Vàng.</b></color>");

                // Xóa sổ ghi nợ
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

            // Xóa luôn dữ liệu giá tiền nếu tháp bị quái bắn nổ (Không hoàn tiền khi tháp chết)
            if (placedTowerCosts.ContainsKey(keyToRemove))
            {
                placedTowerCosts.Remove(keyToRemove);
            }
        }
    }

    public void SelectTowerToBuild(int towerIndex)
    {
        SetPlacementMode(false);
        if (towerIndex >= 0 && towerIndex < availableTowers.Length)
        {
            towerToBuild = availableTowers[towerIndex];
            selectedTowerIndex = towerIndex;
        }
    }
}