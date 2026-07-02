using UnityEngine;

public class CatapultRock : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float areHeight = 3f; // Giữ nguyên tên biến gốc của bạn để tránh mất dữ liệu Inspector
    public float damege = 30f;   

    // --- BIẾN NHỚ HỆ SỐ FUSE ---
    private float currentDamageMultiplier = 1f;

    private Vector3 startPos;
    private float progress = 0f;
    private float totalDistance;

    public void Seek(Transform _target)
    {
        target = _target;
        startPos = transform.position;
        totalDistance = Vector3.Distance(startPos, target.position);

        // ==========================================
        // SỬA LỖI 1: BẮT BUỘC RESET PROGRESS VỀ 0
        // Vì đạn lấy từ Pool ra sẽ giữ nguyên trạng thái cũ (progress = 1). 
        // Nếu không reset, viên đá vừa đẻ ra sẽ tự kích hoạt HitTarget() ngay khung hình đầu tiên!
        // ==========================================
        progress = 0f; 
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
            // SỬA LỖI 2: THAY DESTROY THÀNH DESPAWN
            // Nếu con quái bị tháp khác bắn chết trước khi đá bay tới, 
            // viên đá phải tự trả mình về Pool chứ không được tự hủy hoàn toàn.
            // ==========================================
            SimplePool.Instance.Despawn(gameObject); 
            return;
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
        // Kiểm tra lại một lần nữa phòng trường hợp mục tiêu biến mất đúng khung hình chạm
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