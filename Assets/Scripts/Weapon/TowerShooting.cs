using UnityEngine;

public class TowerShooting : MonoBehaviour
{
    [Header("Pillar Index")]
    public float range = 3f;
    public float fireRate = 0.15f;
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
        if (dualFirePoints != null && dualFirePoints.Length > 0)
        {
            spawnPoint = dualFirePoints[currentFirePointIndex];
            currentFirePointIndex = (currentFirePointIndex + 1) % dualFirePoints.Length;
        }

        GameObject bulletGO = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bulletGO.SendMessage("Seek", target, SendMessageOptions.DontRequireReceiver);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}