using UnityEngine;

public class TowerShooting : MonoBehaviour
{
    [Header("Chỉ số Tấn công")]
    public float range = 3f;
    public float fireRate = 0.15f; 
    private float fireCountdown = 0f;

    [Header("Thiết lập Vật thể")]
    public Transform partToRotate; 
    public GameObject bulletPrefab;

    [Header("Muzzle Flash Settings")]
    public GameObject muzzleFlashPrefab; 

    [Tooltip("Dành cho tháp 1 nòng (VỊ TRÍ ĐẦU SÚNG/FIRE POINT)")]
    public Transform firePoint; 

    [Tooltip("Dành cho tháp nhiều nòng")]
    public Transform[] dualFirePoints;
    private int currentFirePointIndex = 0;

    [Header("Targeting")]
    public string enemyTag = "Enemy";
    private Transform target;

    private TowerStats stats;

    void Start()
    {
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

        float actualRange = range;
        if (stats != null)
        {
            actualRange = range * stats.rangeMultiplier;
        }

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
        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlayUI(stats.towerType); 
        }

        Transform spawnPoint = firePoint;
        if (spawnPoint == null) spawnPoint = transform; 

        if (dualFirePoints != null && dualFirePoints.Length > 0)
        {
            spawnPoint = dualFirePoints[currentFirePointIndex];
            currentFirePointIndex = (currentFirePointIndex + 1) % dualFirePoints.Length;
        }

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, spawnPoint);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;
            Destroy(flash, 0.15f); 
        }

        GameObject bulletGO = SimplePool.Instance.Spawn(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bulletGO.SendMessage("Seek", target, SendMessageOptions.DontRequireReceiver);
        
        if (stats != null)
        {
            bulletGO.SendMessage("SetDamageMultiplier", stats.damageMultiplier, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmosSelected()
    {
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