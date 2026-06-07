using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float damege = 25f;

    public void Seek(Transform _target)
    {
        target = _target;
    }
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 aimPoint = target.position + new Vector3(0f, 0f, 0f);
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(aimPoint);
    }
    
    void HitTarget()
    {
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damege);
        }
        
        Destroy(gameObject);
    }
}
