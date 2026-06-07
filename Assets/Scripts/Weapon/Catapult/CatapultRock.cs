using UnityEngine;

public class CatapultRock : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float areHeight = 3f;
    public float damege = 30f;

    private Vector3 startPos;
    private float progress = 0f;
    private float totalDistance;

    public void Seek(Transform _target)
    {
        target = _target;
        startPos = transform.position;
        totalDistance = Vector3.Distance(startPos, target.position);
    }
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject); return;
        }

        float step = speed * Time.deltaTime;
        progress += step / totalDistance;

        Vector3 currentPos = Vector3.Lerp(startPos, target.position, progress);

        float height = Mathf.Sin(progress * Mathf.PI) * areHeight;
        currentPos.y += height;

        transform.position = currentPos;
        transform.Rotate(Vector3.right * 500f * Time.deltaTime);

        if (progress >= 1f)
        {
            HitTarget();
        }
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
