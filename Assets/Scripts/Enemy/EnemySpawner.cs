using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Target")]
    [Tooltip("Kéo thả Prefab Enemy từ ô Project vào đây")]
    public GameObject enemyPrefab;

    [Tooltip("Kéo thả điểm Đích (End Point) từ Hierarchy vào đây")]
    public Transform endPoint;

    [Header("Spawn Locations")]
    [Tooltip("Thay đổi Size thành 3 và kéo thả 3 ô Spawn vào đây")]
    public Transform[] spawnPoints; // Mảng chứa danh sách các điểm spawn

    [Header("Spawn Settings")]
    [Tooltip("Thời gian giãn cách giữa mỗi lần sinh quái (giây)")]
    public float spawnInterval = 3f;

    void Start()
    {
        // Bắt đầu lặp lại việc sinh quái
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        // Kiểm tra an toàn xem bạn đã gán đủ đồ trong Inspector chưa
        if (enemyPrefab == null || endPoint == null)
        {
            Debug.LogWarning("Spawner bị thiếu Enemy Prefab hoặc End Point!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Bạn chưa gán bất kỳ ô Spawn Point nào vào danh sách!");
            return;
        }

        // 1. Lấy ngẫu nhiên một chỉ số (Index) trong mảng spawnPoints
        // Random.Range(int, int) trong Unity sẽ lấy từ min đến max-1, vừa khít với độ dài mảng
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        // Phòng trường hợp bạn gán thiếu phần tử trong mảng
        if (chosenSpawnPoint == null)
        {
            Debug.LogWarning($"Ô Spawn ở vị trí thứ {randomIndex} đang bị trống (None)!");
            return;
        }

        // 2. Sinh con quái ra ngay tại vị trí và góc xoay của ô Spawn ngẫu nhiên vừa chọn
        GameObject spawnedEnemy = Instantiate(enemyPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

        // 3. Tìm script EnemyMovement trên con quái để đưa điểm đích cho nó chạy
        EnemyMovement movement = spawnedEnemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.SetTarget(endPoint);
        }
    }
}