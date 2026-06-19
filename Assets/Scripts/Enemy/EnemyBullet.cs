using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Transform target;


    [Header("Chi so dan")]
    public float speed = 10f;   
    public float damage = 20f;   

    public void Seek(Transform _target)
    {
        target = _target;
    }
    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    void HitTarget()
    {
        TowerHealth health = target.GetComponent<TowerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
