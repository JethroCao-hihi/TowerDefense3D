using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("Indicator Visuals")]
    public GameObject indicatorRoot;
    public Renderer indicatorRenderer;
    public Material validMaterial;
    public Material invalidMaterial;

    [Header("Path Map (Drag all path layers here)")]
    public Tilemap[] obstacleTilemaps;

    [Header("Shop Tower")]
    public GameObject[] availableTowers;
    private GameObject towerToBuild;

    private Camera mainCamera;
    private Plane groundPlane;
    private bool canPlaceAtCurrentPosition = false;

    private Vector3Int currentCellPosition;
    private System.Collections.Generic.HashSet<Vector3Int> occupiedCells = new System.Collections.Generic.HashSet<Vector3Int>();

    void Start()
    {
        mainCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        indicatorRoot.SetActive(true);

        if (availableTowers != null && availableTowers.Length > 0)
        {
            towerToBuild = availableTowers[0];
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (indicatorRoot.activeSelf) indicatorRoot.SetActive(false);
            return;
        }
        if (Mouse.current == null) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            indicatorRoot.SetActive(false);
            return;
        }
        else
        {
            indicatorRoot.SetActive(true);
        }

        MoveIndicatorAndValidatePosition();

        if (Input.GetMouseButtonDown(0))
        {
            if (canPlaceAtCurrentPosition)
            {
                BuildTower();
            }
            else
            {
                Debug.Log("<color=red>LỖI: Vị trí không hợp lệ (Đè lên đường hoặc đã có tháp)!</color>");
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

                bool isOverlappingRoad = false;

                foreach (Tilemap map in obstacleTilemaps)
                {
                    if (map == null) continue;

                    Vector3Int cellPos = map.WorldToCell(snappedPoint);
                    for (int i = -10; i <= 10; i++)
                    {
                        if (map.HasTile(new Vector3Int(cellPos.x, cellPos.y, i)) ||
                            map.HasTile(new Vector3Int(cellPos.x, i, cellPos.z)))
                        {
                            isOverlappingRoad = true;
                            break;
                        }
                    }

                    foreach (Transform child in map.transform)
                    {
                        Vector2 childPos2D = new Vector2(child.position.x, child.position.z);
                        Vector2 snapPos2D = new Vector2(snappedPoint.x, snappedPoint.z);

                        if (Vector2.Distance(childPos2D, snapPos2D) < 0.5f)
                        {
                            isOverlappingRoad = true;
                            break;
                        }
                    }

                    if (isOverlappingRoad) break;
                }

                bool isOccupied = occupiedCells.Contains(currentCellPosition);

                UpdateIndicatorStatus(!isOverlappingRoad && !isOccupied);
            }
        }
    }

    void UpdateIndicatorStatus(bool isValid)
    {
        canPlaceAtCurrentPosition = isValid;
        indicatorRenderer.material = isValid ? validMaterial : invalidMaterial;
    }

    void BuildTower()
    {
        if (towerToBuild != null)
        {
            Instantiate(towerToBuild, indicatorRoot.transform.position + Vector3.up * 0.15f, Quaternion.identity);
            occupiedCells.Add(currentCellPosition);
        }
    }

    public void SelectTowerToBuild(int towerIndex)
    {
        if (towerIndex >= 0 && towerIndex < availableTowers.Length)
        {
            towerToBuild = availableTowers[towerIndex];
        }
    }
}