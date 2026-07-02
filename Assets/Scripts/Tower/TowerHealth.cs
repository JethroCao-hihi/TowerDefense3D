using UnityEngine;
using UnityEngine.UI;

public class TowerHealth : MonoBehaviour
{
    [Header("Dữ liệu cấu hình tháp")]
    public TowerStats towerStats; // Script đang chứa file Data của tháp

    [Header("Hiệu ứng nổ tháp (VFX")] 
    public GameObject destructionVFX;
    
    private float currentHealth;
    public Image healthBarFill;

    void Start()
    {
        // Đọc máu từ file Data thông qua TowerStats
        if (towerStats != null && towerStats.configData != null)
        {
            currentHealth = towerStats.configData.maxHealth;
            
            // Nếu tháp đã được ghép lên cấp, máu cũng phải trâu hơn! (Cấp 2 x 1.5 máu, Cấp 3 x 2.0 máu...)
            currentHealth *= (1f + (towerStats.towerLevel - 1) * 0.5f);
        }

        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null && towerStats != null && towerStats.configData != null)
        {
            float maxHp = towerStats.configData.maxHealth * (1f + (towerStats.towerLevel - 1) * 0.5f);
            healthBarFill.fillAmount = currentHealth / maxHp;
        }
    }

    void Die()
    {
        Debug.Log($"💥 Tháp {gameObject.name} đã bị phá hủy!");
        
        // <<== 2. GỌI HIỆU ỨNG NỔ RA TRƯỚC KHI THÁP BIẾN MẤT
        if (destructionVFX != null)
        {
            // Đẻ cục VFX ra ngay tại vị trí của tháp
            GameObject fx = Instantiate(destructionVFX, transform.position, Quaternion.identity);
            
            // Hủy cục VFX sau 2 giây để dọn rác bộ nhớ
            Destroy(fx, 2f); 
        }

        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.RemoveTowerFromGrid(gameObject);
        }

        Destroy(gameObject);
    }
}