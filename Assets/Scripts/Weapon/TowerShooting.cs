using UnityEngine;

public class TowerShooting : MonoBehaviour
{
    [Header("Chỉ số Trụ")]
    public float range = 3f;
    public float fireRate = 0.15f;  // Súng máy thì nạp đạn cực nhanh (0.15 giây/viên)
    private float fireCountdown = 0f;

    [Header("Cài đặt vật thể")]
    public Transform partToRotate;
    public GameObject bulletPrefab;

    [Tooltip("Dùng cho tháp 1 nòng (Đại bác, Nỏ, Đá)")]
    public Transform firePoint;

    [Tooltip("Dùng cho tháp nhiều nòng (Súng máy)")]
    public Transform[] dualFirePoints;
    private int currentFirePointIndex = 0; // Bộ đếm đổi nòng

    [Header("Mục tiêu")]
    public string enemyTag = "Enemy";
    private Transform target;

    void Update()
    {
        UpdateTarget();

        if (target == null) return;

        // 1. Xoay nòng súng
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        // 2. Kích hoạt bắn
        if (fireCountdown <= 0f)
        {
            Shoot(); // Cứ hết thời gian nạp đạn là bắn 1 viên
            fireCountdown = fireRate;
        }
        fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Shoot()
    {
        Transform spawnPoint = firePoint;

        // Nếu là tháp súng máy (có 2 nòng trở lên)
        if (dualFirePoints != null && dualFirePoints.Length > 0)
        {
            // Lấy nòng súng hiện tại
            spawnPoint = dualFirePoints[currentFirePointIndex];

            // Đảo nòng súng cho lần bắn kế tiếp (0 -> 1 -> 0 -> 1)
            currentFirePointIndex = (currentFirePointIndex + 1) % dualFirePoints.Length;
        }

        // Khạc đạn
        GameObject bulletGO = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bulletGO.SendMessage("Seek", target, SendMessageOptions.DontRequireReceiver);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}