using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float damege = 25f; // Giữ nguyên chữ "damege" để bạn không bị mất dữ liệu ngoài Inspector

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
            // SỬA LỖI CHÍ MẠNG: THAY DESTROY THÀNH DESPAWN
            // Khi mục tiêu biến mất giữa đường, viên đạn phải tự thu hồi về Pool
            // ==========================================
            SimplePool.Instance.Despawn(gameObject);
            return;
        }

        Vector3 aimPoint = target.position; // Rút gọn đoạn cộng Vector3(0,0,0) thừa cho nhẹ code
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
        // Kiểm tra an toàn đề phòng quái biến mất ngay đúng khung hình chạm vào
        if (target != null)
        {
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // --- CHỈ NHÂN SÁT THƯƠNG VÀ TRỪ MÁU ---
                enemyHealth.TakeDamage(damege * currentDamageMultiplier);
            }
        }

        SimplePool.Instance.Despawn(gameObject);
    }
}