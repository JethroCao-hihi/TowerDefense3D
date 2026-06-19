using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("Chi so ban")]
    public float range = 4f;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Cai dat nong sung")]
    public Transform partToRotate;
    public Transform firePoint;
    public GameObject enemyBulletPrefab;


    private string towerTag = "Tower";
    private Transform target;   

    void Update()
    {
        if (Time.timeScale == 0f) return;

        UpdateTarget();

        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;

        if (partToRotate != null)
        {
            partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }
        fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        GameObject[] towers = GameObject.FindGameObjectsWithTag(towerTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestTower = null;

        foreach (GameObject tower in towers)
        {
            float distanceToTower = Vector3.Distance(transform.position, tower.transform.position);
            if (distanceToTower < shortestDistance)
            {
                shortestDistance = distanceToTower;
                nearestTower = tower;
            }
        }
        if (nearestTower != null && shortestDistance <= range)
        {
            target = nearestTower.transform;
        }
        else
        {
            target = null;
        }
    }

    void Shoot()
    {
        GameObject bulletGo = Instantiate (enemyBulletPrefab, firePoint.position, firePoint.rotation);
        EnemyBullet bullet = bulletGo.GetComponent<EnemyBullet>();

        if (bullet != null)
        {
            bullet.Seek(target);
        }
    }

    private void OnDrawGizmosSellected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
