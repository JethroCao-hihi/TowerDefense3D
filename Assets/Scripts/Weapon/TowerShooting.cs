using UnityEngine;

public class TowerShooting : MonoBehaviour
{
    [Header("Pillar Index")]
    public float range = 3f;
    public float fireRate = 0.15f; // Thời gian chờ giữa các lần bắn
    private float fireCountdown = 0f;

    [Header("Object Settings")]
    public Transform partToRotate;
    public GameObject bulletPrefab;

    [Tooltip("For single-barrel towers")]
    public Transform firePoint;

    [Tooltip("For multi-barrel towers")]
    public Transform[] dualFirePoints;
    private int currentFirePointIndex = 0;

    [Header("Targeting")]
    public string enemyTag = "Enemy";
    private Transform target;

    // --- BIẾN KẾT NỐI VỚI HỆ THỐNG NÂNG CẤP ---
    private TowerStats stats;

    void Start()
    {
        // Lấy component TowerStats gắn trên cùng tòa tháp
        stats = GetComponent<TowerStats>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        UpdateTarget();
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireCountdown <= 0f)
        {
            Shoot();
            
            // --- TÍNH TOÁN TỐC ĐỘ BẮN ĐÃ NÂNG CẤP ---
            float actualFireRate = fireRate;
            if (stats != null)
            {
                // Chia cho multiplier: Hệ số càng lớn -> thời gian chờ càng nhỏ -> Bắn càng nhanh
                actualFireRate = fireRate / stats.fireRateMultiplier; 
            }
            
            fireCountdown = actualFireRate;
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

        // --- TÍNH TOÁN TẦM XA ĐÃ NÂNG CẤP ---
        float actualRange = range;
        if (stats != null)
        {
            actualRange = range * stats.rangeMultiplier;
        }

        // Dùng tầm xa mới để dò quái
        if (nearestEnemy != null && shortestDistance <= actualRange)
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
        // --- PHÁT ÂM THANH BẮN (Tự động nhận diện tên tháp) ---
        if (AudioManager.Instance != null && stats != null)
        {
            AudioManager.Instance.PlayShoot(stats.towerType);
        }

        Transform spawnPoint = firePoint;
        if (dualFirePoints != null && dualFirePoints.Length > 0)
        {
            spawnPoint = dualFirePoints[currentFirePointIndex];
            currentFirePointIndex = (currentFirePointIndex + 1) % dualFirePoints.Length;
        }

        // ==========================================
        // ĐÃ THAY THẾ: Gọi Đạn từ SimplePool thay vì Instantiate
        // ==========================================
        GameObject bulletGO = SimplePool.Instance.Spawn(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Truyền mục tiêu cho đạn bay tới
        bulletGO.SendMessage("Seek", target, SendMessageOptions.DontRequireReceiver);
        
        // --- TRUYỀN SỨC MẠNH (DAME) CHO VIÊN ĐẠN KHI VỪA ĐẺ RA ---
        if (stats != null)
        {
            bulletGO.SendMessage("SetDamageMultiplier", stats.damageMultiplier, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Hiển thị vòng đỏ trong Scene để bạn dễ căn chỉnh thiết kế
        Gizmos.color = Color.red;
        float displayRange = range;
        
        TowerStats editorStats = GetComponent<TowerStats>();
        if (editorStats != null)
        {
            displayRange = range * editorStats.rangeMultiplier;
        }
        
        Gizmos.DrawWireSphere(transform.position, displayRange);
    }
}