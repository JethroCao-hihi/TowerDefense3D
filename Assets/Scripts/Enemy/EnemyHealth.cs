using UnityEngine;
using UnityEngine.UI; // BẮT BUỘC để dùng Image cho thanh máu
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHealth = 100f;
    private float currentHealth;
    public int killReward = 100; // Tiền thưởng khi giết con quái này

    [Header("Health Bar")]
    public Image healthBarFill;

    [Header("Hiệu ứng Animation Động cơ")]
    public MeshFilter ufoMeshFilter;
    public Mesh normalMesh;
    public Mesh hitReactionMesh;
    public float changeDuration = 0.15f;

    [Header("Hiệu ứng VFX chớp màu (Nếu có)")]
    public Renderer enemyRenderer;
    private Color originalColor;
    private Coroutine currentHitAnimation;

    public GameObject floatingTextPrefab; // Prefab cho chữ bay lên khi giết quái

    [Header("Hiệu ứng Nổ khi chết (VFX)")]
    public GameObject deathExplosionVFX; // <<=== 1. KHAI BÁO CỤC NỔ Ở ĐÂY

    void Start()
    {
        currentHealth = maxHealth;

        // Setup mặc định cho động cơ lúc đẻ ra
        if (ufoMeshFilter != null && normalMesh != null)
        {
            ufoMeshFilter.mesh = normalMesh;
        }

        // Lưu màu gốc
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void TakeDamage(float damageAmount)
    {
        // 1. Trừ máu
        currentHealth -= damageAmount;

        // 2. Cập nhật giao diện Thanh Máu
        if (healthBarFill)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }

        // 3. Chạy hiệu ứng giật động cơ
        if (currentHitAnimation != null) StopCoroutine(currentHitAnimation);
        currentHitAnimation = StartCoroutine(HitReactionRoutine());

        // 4. Kiểm tra chết
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Hàm xử lý việc tráo Mesh trong 0.15 giây
    IEnumerator HitReactionRoutine()
    {
        if (ufoMeshFilter && hitReactionMesh != null)
        {
            ufoMeshFilter.mesh = hitReactionMesh;
        }

        if (enemyRenderer)
        {
            enemyRenderer.material.color = Color.red;
        }

        yield return new WaitForSeconds(changeDuration);

        if (ufoMeshFilter && normalMesh != null)
        {
            ufoMeshFilter.mesh = normalMesh;
        }

        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor;
        }

        currentHitAnimation = null;
    }

    public void Die()
    {
        Debug.Log("💥 Enemy died!");

        // <<=== 2. GỌI VỤ NỔ HOÀNH TRÁNG RA ĐÚNG CHỖ QUÁI CHẾT
        if (deathExplosionVFX != null)
        {
            GameObject fx = Instantiate(deathExplosionVFX, transform.position, Quaternion.identity);
            Destroy(fx, 2f); // Hủy vụ nổ sau 2 giây để không làm nặng game
        }

        // GỌI NGÂN HÀNG CỘNG TIỀN
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(killReward);
        }
        
        // Khởi tạo chữ bay lên tại vị trí quái chết
        if (floatingTextPrefab != null)
        {
            GameObject floatText = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            floatText.GetComponent<FloatingText>().Setup("+" + killReward + "$", Color.yellow);
        }

        Destroy(gameObject);
    }
}