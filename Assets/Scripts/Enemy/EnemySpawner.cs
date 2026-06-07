using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Target")]
    [Tooltip("Drag and drop the Enemy Prefab from the Project window here")]
    public GameObject enemyPrefab;

    [Tooltip("Drag and drop the End Point from the Hierarchy here")]
    public Transform endPoint;

    [Header("Spawn Locations")]
    [Tooltip("Change the Size to 3 and drag and drop 3 Spawn Points here")]
    public Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [Tooltip("Time interval between each enemy spawn (seconds)")]
    public float spawnInterval = 3f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
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

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        if (chosenSpawnPoint == null)
        {
            Debug.LogWarning($"Ô Spawn ở vị trí thứ {randomIndex} đang bị trống (None)!");
            return;
        }

        GameObject spawnedEnemy = Instantiate(enemyPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

        EnemyMovement movement = spawnedEnemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.SetTarget(endPoint);
        }
    }
}