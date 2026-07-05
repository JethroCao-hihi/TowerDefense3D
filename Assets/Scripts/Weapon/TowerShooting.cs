using UnityEngine;

public class TowerShooting : MonoBehaviour
{
    [Header("Chỉ số Tấn công")]
    public float range = 3f;
    public float fireRate = 0.15f; // Thời gian chờ giữa các lần bắn
    private float fireCountdown = 0f;

    [Header("Thiết lập Vật thể")]
    public Transform partToRotate; // Phần xoay của tháp
    public GameObject bulletPrefab;

    [Header("Muzzle Flash Settings")]
    public GameObject muzzleFlashPrefab; // <--- KHÓI SÚNG / LÓE SÁNG

    [Tooltip("Dành cho tháp 1 nòng (VỊ TRÍ ĐẦU SÚNG/FIRE POINT)")]
    public Transform firePoint; // <--- KÉO VẬT THỂ ĐẦU SÚNG VÀO ĐÂY

    [Tooltip("Dành cho tháp nhiều nòng")]
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

        // Xoay tháp hướng về mục tiêu
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        // Bắn đạn theo thời gian đếm ngược
        if (fireCountdown <= 0f)
        {
            Shoot();
            
            // --- TÍNH TOÁN TỐC ĐỘ BẮN ĐÃ NÂNG CẤP ---
            float actualFireRate = fireRate;
            if (stats != null)
            {
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
        // --- PHÁT ÂM THANH BẮN (nếu có) ---
        if (AudioManager.Instance != null && stats != null)
        {
            AudioManager.Instance.PlayShoot(stats.towerType);
        }

        // Xác định vị trí nòng súng sẽ bắn
        Transform spawnPoint = firePoint;
        if (spawnPoint == null) spawnPoint = transform; // Đề phòng lỗi chưa kéo FirePoint

        if (dualFirePoints != null && dualFirePoints.Length > 0)
        {
            spawnPoint = dualFirePoints[currentFirePointIndex];
            currentFirePointIndex = (currentFirePointIndex + 1) % dualFirePoints.Length;
        }

        // ==========================================
        // 1. SINH HIỆU ỨNG VFX TẠI ĐÚNG VỊ TRÍ ĐẦU SÚNG
        // ==========================================
        if (muzzleFlashPrefab != null)
        {
            // --- ĐÃ CHỈNH SỬA ĐỂ KHÓA TỌA ĐỘ VÀO ĐẦU SÚNG ---

            // Spawn hiệu ứng làm con của spawnPoint (để đi theo nòng súng)
            GameObject flash = Instantiate(muzzleFlashPrefab, spawnPoint);
            
            // ÉP BUỘC vị trí và góc quay địa phương (tương đối) khớp hoàn toàn với đầu nòng súng (tâm 0,0,0)
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;
            
            // Tự hủy sau một thời gian ngắn
            Destroy(flash, 0.15f); 
        }

        // ==========================================
        // 2. SINH ĐẠN TỪ POOL VÀ BẮN ĐI (đã tối ưu trước đó)
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
        // Hiển thị vòng đỏ trong Scene để dễ căn chỉnh
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