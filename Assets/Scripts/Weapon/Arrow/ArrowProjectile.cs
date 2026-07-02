using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private Transform target;
    public float speed = 15f;
    public float damege = 20f; // Giữ nguyên chữ "damege" để tránh mất dữ liệu ngoài Inspector

    // --- BIẾN NHỚ HỆ SỐ FUSE ---
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
            // ==========================================
            // ĐÃ SỬA: Thay thế hoàn toàn Destroy thành Despawn
            // Trả mũi tên về Pool nếu quái bị tiêu diệt giữa đường bay
            // ==========================================
            SimplePool.Instance.Despawn(gameObject);
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
        // Bọc hàm kiểm tra an toàn phòng trường hợp mục tiêu biến mất ngay khung hình chạm
        if (target != null)
        {
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // --- NHÂN SÁT THƯƠNG KHI TRÚNG QUÁI ---
                enemyHealth.TakeDamage(damege * currentDamageMultiplier);
            }
        }

        SimplePool.Instance.Despawn(gameObject);
    }
}