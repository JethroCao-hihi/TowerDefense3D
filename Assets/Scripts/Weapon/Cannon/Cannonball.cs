using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float damege = 25f;

    private float currentDamageMultiplier = 1f;

    public void Seek(Transform _target)
    {
        target = _target;
    }

    // --- LỖ TAI LẮNG NGHE THÁP TRUYỀN SỨC MẠNH ---
    public void SetDamageMultiplier(float mult)
    {
        currentDamageMultiplier = mult;
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
            // --- CHỈ NHÂN SÁT THƯƠNG VÀ TRỪ MÁU, KHÔNG CÓ HIỆU ỨNG NỔ Ở ĐÂY NỮA ---
            enemyHealth.TakeDamage(damege * currentDamageMultiplier);
        }

        Destroy(gameObject);
    }
}