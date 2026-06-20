using UnityEngine;

public class TowerStats : MonoBehaviour
{
    [Header("Dinh danh loai thap")]
    public string towerType = "Cannon";

    [Header("Chi so nang cap")]
    public int towerLevel = 1;
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f; // Biến mới: Hệ số nhân tốc độ bắn

    [Header("Chỉ số Gốc")]
    public float baseDamage = 25f;  // Sát thương mặc định (Bạn có thể sửa số này ngoài Inspector cho từng tháp)
    public float baseFireRate = 1f; // Tốc độ bắn gốc (1 viên/giây)

    private TowerStats stats;       // Biến để đọc dữ liệu nâng cấp

    public void UpgradeTower()
    {
        towerLevel++;

        // Cập nhật chỉ số sức mạnh
        damageMultiplier += 0.5f;   // Tăng 50% sát thương gốc mỗi cấp
        fireRateMultiplier += 0.2f; // Tăng 20% tốc độ bắn gốc mỗi cấp

        // Tăng kích thước tháp mỗi cấp thêm 20%
        transform.localScale *= 1.2f;

        Debug.Log($"🎉 {towerType} đã tiến hóa lên CẤP {towerLevel}! Sát thương x{damageMultiplier}, Tốc bắn x{fireRateMultiplier}");
    }
}