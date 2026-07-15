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
    public Material fuseMaterial;      

    [Header("Selection B (Hiệu ứng khi đặt xong)")]
    public GameObject buildSuccessPrefab;
    public float effectDuration = 0.5f;

    [Header("Giao diện UI Xác nhận xóa")]
    public GameObject confirmationPanel;

    [Header("Bản đồ Đường đi")]
    public Tilemap[] obstacleTilemaps;

    private GameObject towerToBuild;
    private int currentSelectedTowerCost = 0; 

    private Camera mainCamera;
    private Plane groundPlane;
    private bool isActionValid = false;
    private Vector3Int currentCellPosition;
    
    private GameObject draggedTowerModel;
    private Vector3 originalTowerPos;

    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();

    private Vector3Int firstFuseCell;
    public bool isDraggingForFuse = false;

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
        
        towerToBuild = null; 
        currentSelectedTowerCost = 0;

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

        // Kiểm tra xem người chơi có đang bấm vào UI (nút bấm, bảng điều khiển) không
        if (!isDraggingForFuse && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Fix lỗi trên mobile: Nếu có touch thì kiểm tra touch đó có dính vào UI không
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                if (EventSystem.current.IsPointerOverGameObject(Touchscreen.current.touches[0].touchId.ReadValue()))
                {
                    indicatorRoot.SetActive(false);
                    return;
                }
            }
            else
            {
                indicatorRoot.SetActive(false);
                return;
            }
        }

        MoveIndicatorAndValidatePosition();

        // ==========================================
        // THAY THẾ TOÀN BỘ LỆNH INPUT CŨ SANG CHUẨN MỚI
        // ==========================================
        bool isPointerDown = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        bool isPointerUp = Pointer.current != null && Pointer.current.press.wasReleasedThisFrame;
        bool isRightClick = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        if (isPointerDown)
        {
            if (currentMode == PlacementMode.Build)
            {
                if (placedTowers.ContainsKey(currentCellPosition))
                {
                    isDraggingForFuse = true;
                    firstFuseCell = currentCellPosition;
                    isActionValid = false; 
                    UpdateIndicatorStatus(fuseMaterial);

                    draggedTowerModel = placedTowers[firstFuseCell];
                    originalTowerPos = draggedTowerModel.transform.position; 
                }
                else if (isActionValid && towerToBuild != null)
                {
                    BuildTower();
                }
            }
            else if (currentMode == PlacementMode.Delete && placedTowers.ContainsKey(currentCellPosition))
            {
                OpenConfirmationDialog();
            }
        }

        if (isPointerUp && isDraggingForFuse)
        {
            isDraggingForFuse = false;
            
            if (isActionValid) 
            {
                ExecuteDragAndDropFuse();
            }
            else if (draggedTowerModel != null)
            {
                draggedTowerModel.transform.position = originalTowerPos;
            }

            draggedTowerModel = null; 
            MoveIndicatorAndValidatePosition();
        }

        // Hủy thao tác khi bấm chuột phải (Trên mobile thường dùng nút tắt/UI thay vì nút này)
        if (isRightClick)
        {
            towerToBuild = null; 
            indicatorRoot.SetActive(false); 
        }
    }

    void MoveIndicatorAndValidatePosition()
    {
        // Chặn lỗi NullReferenceException trên điện thoại khi không có chuột
        if (Pointer.current == null) return;

        // Lấy tọa độ bằng Pointer thay vì Mouse để hỗ trợ Cảm ứng
        Vector2 screenPosition = Pointer.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

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

                if (isDraggingForFuse)
                {
                    indicatorRoot.SetActive(true);
                    if (hasTower && currentCellPosition != firstFuseCell)
                    {
                        GameObject tower1 = placedTowers[firstFuseCell];
                        GameObject tower2 = placedTowers[currentCellPosition];
                        if (tower1 != null && tower2 != null)
                        {
                            TowerStats stats1 = tower1.GetComponent<TowerStats>();
                            TowerStats stats2 = tower2.GetComponent<TowerStats>();
                            if (stats1 != null && stats2 != null && stats1.towerType == stats2.towerType && stats1.towerLevel == stats2.towerLevel)
                            {
                                isActionValid = true;
                                UpdateIndicatorStatus(fuseMaterial);
                            }
                            else
                            {
                                isActionValid = false;
                                UpdateIndicatorStatus(invalidMaterial);
                            }
                        }
                    }
                    else
                    {
                        isActionValid = false;
                        UpdateIndicatorStatus(invalidMaterial);
                    }
                }
                else if (currentMode == PlacementMode.Build)
                {
                    if (towerToBuild == null)
                    {
                        isActionValid = false;
                        indicatorRoot.SetActive(hasTower);
                        UpdateIndicatorStatus(originalMaterial); 
                    }
                    else 
                    {
                        indicatorRoot.SetActive(true);
                        bool hasEnoughMoney = EconomyManager.Instance.currentMoney >= currentSelectedTowerCost;
                        isActionValid = !isOverlappingRoad && !hasTower && hasEnoughMoney;
                        UpdateIndicatorStatus(isActionValid ? originalMaterial : invalidMaterial);
                    }
                }
                else if (currentMode == PlacementMode.Delete)
                {
                    indicatorRoot.SetActive(true);
                    isActionValid = hasTower;
                    UpdateIndicatorStatus(isActionValid ? deleteMaterial : invalidMaterial);
                }
            }
        }

        if (isDraggingForFuse && draggedTowerModel != null)
        {
            Vector3 liftedPos = indicatorRoot.transform.position;
            liftedPos.y += 1.2f; 
            draggedTowerModel.transform.position = Vector3.Lerp(draggedTowerModel.transform.position, liftedPos, Time.deltaTime * 15f);
        }
    }

    void ExecuteDragAndDropFuse()
    {
        GameObject tower1 = placedTowers[firstFuseCell]; 
        GameObject tower2 = placedTowers[currentCellPosition]; 

        TowerStats stats1 = tower1.GetComponent<TowerStats>();
        TowerStats stats2 = tower2.GetComponent<TowerStats>();
        TowerHealth health1 = tower1.GetComponent<TowerHealth>();
        TowerHealth health2 = tower2.GetComponent<TowerHealth>();

        if (stats2 != null)
        {
            if (health1 != null && health2 != null)
            {
                if (health1.currentHealth > health2.currentHealth)
                {
                    if (health1.currentHealth >= health1.maxHealth)
                    {
                        float targetHpPercent = (health2.currentHealth / health2.maxHealth) * 100f;

                        if (targetHpPercent >= 100f)
                        {
                            health2.currentHealth = health2.maxHealth;
                        }
                        else if (targetHpPercent >= 75f)
                        {
                            health2.currentHealth = health2.maxHealth;
                        }
                        else if (targetHpPercent >= 50f)
                        {
                            health2.currentHealth += (health2.maxHealth * 0.25f);
                        }
                        else
                        {
                            health2.currentHealth += (health2.maxHealth * 0.75f);
                        }
                    }
                    else
                    {
                        health2.currentHealth += (health1.currentHealth * 0.75f);
                    }
                }

                health2.currentHealth = Mathf.Clamp(health2.currentHealth, 0f, health2.maxHealth);
            }

            stats2.UpgradeTower();

            if (health2 != null)
            {
                health2.SendMessage("UpdateTowerHealthBar", SendMessageOptions.DontRequireReceiver);
            }

            if (buildSuccessPrefab != null)
            {
                GameObject successEffect = Instantiate(buildSuccessPrefab, tower2.transform.position, Quaternion.identity);
                Destroy(successEffect, effectDuration);
            }

            Destroy(tower1);
            placedTowers.Remove(firstFuseCell);
        }
    }

    void BuildTower()
    {
        if (towerToBuild != null)
        {
            if (!EconomyManager.Instance.SpendMoney(currentSelectedTowerCost)) return;

            GameObject newTower = Instantiate(towerToBuild, indicatorRoot.transform.position + Vector3.up * 0.15f, Quaternion.identity);

            if (SoundManager.Instance != null) {
                SoundManager.Instance.PlayUI("Build"); 
            }

            if (buildSuccessPrefab != null)
            {
                Vector3 effectPos = indicatorRoot.transform.position + Vector3.up * 0.1f;
                GameObject successEffect = Instantiate(buildSuccessPrefab, effectPos, Quaternion.identity);
                Destroy(successEffect, effectDuration);
            }

            placedTowers.Add(currentCellPosition, newTower);

            StartCoroutine(SelectionToggleRoutine());
        }
    }

    public void SetSelectedTower(GameObject prefab, int cost)
    {
        currentMode = PlacementMode.Build;
        towerToBuild = prefab;
        currentSelectedTowerCost = cost; 
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
            GameObject towerToDelete = placedTowers[cellToTargetDelete];
            TowerStats stats = towerToDelete.GetComponent<TowerStats>();

            if (stats != null && stats.configData != null)
            {
                int refundAmount = (stats.configData.baseCost * stats.towerLevel) / 2;
                EconomyManager.Instance.AddMoney(refundAmount);
            }

            Destroy(towerToDelete);
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
        }
    }
}