using UnityEngine;
using UnityEngine.UI; // BẮT BUỘC để dùng Image cho thanh máu
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Bộ dữ liệu cấu hình sạch (ScriptableObject)")]
    public EnemyData configData; // Kéo file dữ liệu quái (Data_Goblin, Data_Orc...) vào đây

    // Ẩn các biến này khỏi Inspector vì bây giờ game sẽ tự động đọc dữ liệu từ file ScriptableObject
    [HideInInspector] public float maxHealth = 100f;
    [HideInInspector] public int killReward = 100; 
    [HideInInspector] public int scoreReward = 500; // Thêm biến ẩn lưu điểm thưởng đọc từ ScriptableObject
    
    private float currentHealth;

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
    public GameObject deathExplosionVFX; 

    void Start()
    {
        // --- TỰ ĐỘNG NẠP CHỈ SỐ SẠCH TỪ SCRIPTABLE OBJECT ---
        if (configData != null)
        {
            maxHealth = configData.maxHealth;
            killReward = configData.goldReward;
            
            // 💡 LƯU Ý ĐỒ ÁN: Nếu file EnemyData của bạn chưa có biến scoreReward, 
            // dòng dưới đây tạm thời sẽ lấy mặc định là 500 điểm cho quái thường, 2000 điểm cho Boss.
            // Sau này bạn có thể vào script EnemyData thêm: public int scoreReward; rồi sài: scoreReward = configData.scoreReward;
            if (gameObject.CompareTag("Boss")) scoreReward = 2000;
            else scoreReward = 500;

            // 💡 MẸO MỞ RỘNG: Nếu bạn có script quản lý di chuyển (VD: EnemyMovement hoặc AIPath),
            // bạn có thể nạp luôn tốc độ chạy của quái tại đây ngoài hàm Start để quái đi chuẩn chỉ:
            // var movement = GetComponent<EnemyMovement>();
            // if (movement != null) movement.speed = configData.moveSpeed;
        }

        currentHealth = maxHealth;

        // Đảm bảo thanh máu đầy 100% khi quái vừa được sinh ra
        if (healthBarFill)
        {
            healthBarFill.fillAmount = 1f;
        }

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
        // Hiển thị tên quái chính xác trong Console
        string deadEnemyName = configData != null ? configData.enemyName : "Enemy";
        Debug.Log($"💥 {deadEnemyName} died!");

        // 1. GỌI VỤ NỔ HOÀNH TRÁNG
        if (deathExplosionVFX != null)
        {
            GameObject fx = Instantiate(deathExplosionVFX, transform.position, Quaternion.identity);
            Destroy(fx, 2f); 
        }

        // 2. RUNG MÀN HÌNH (CHỈ KHI LÀ BOSS)
        // Đảm bảo con Boss ngoài Unity đã được gán Tag là "Boss"
        /*if (gameObject.CompareTag("Boss"))
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.3f, 0.15f);
            }
        }*/

        // 3. GỌI NGÂN HÀNG CỘNG TIỀN
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(killReward);
        }

        // =========================================================
        // TRANG BỊ MỚI: CỘNG ĐIỂM VÀ TIỀN VÀO HỆ THỐNG MENU MANAGER
        // =========================================================
        if (MenuManager.Instance != null)
        {
            // Tự động đẩy điểm (scoreReward) và tiền (killReward) thực tế của con quái này vào MenuManager
            MenuManager.Instance.AddScoreAndCoins(scoreReward, killReward);
        }
    
        // 4. Khởi tạo chữ bay lên
        if (floatingTextPrefab != null)
        {
            GameObject floatText = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            floatText.GetComponent<FloatingText>().Setup("+" + killReward + "$", Color.yellow);
        }

        Destroy(gameObject);
    }
}