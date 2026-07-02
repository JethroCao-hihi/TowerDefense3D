using UnityEngine;

public class TowerStats : MonoBehaviour
{
    [Header("Bộ dữ liệu cấu hình sạch (ScriptableObject)")]
    public TowerData configData; // Kéo file Scriptable Object vào đây ngoài Inspector

    // Ẩn các biến này khỏi Inspector vì giờ game sẽ tự đọc từ file cấu hình sạch
    [HideInInspector] public string towerType = "Cannon";
    [HideInInspector] public float baseDamage = 25f;  
    [HideInInspector] public float baseFireRate = 1f; 

    [Header("Chi so nang cap")]
    public int towerLevel = 1;
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f; 
    public float rangeMultiplier = 1f; 

    private void Awake()
    {
        // Tự động nạp chỉ số gốc từ file cấu hình Scriptable Object ngoài cửa sổ Project
        if (configData != null)
        {
            towerType = configData.towerType;
            baseDamage = configData.baseDamage;
            baseFireRate = configData.baseFireRate;
        }
    }

    public void UpgradeTower()
    {
        towerLevel++;

        // Giữ nguyên chỉ số cân bằng xuất sắc bạn vừa thiết lập
        damageMultiplier += 1.2f;   // Tăng mạnh sát thương gốc mỗi cấp để không bị lỗ DPS khi ghép
        fireRateMultiplier += 0.5f; // Tăng mạnh tốc độ bắn gốc mỗi cấp
        rangeMultiplier += 0.2f;    // Tăng tầm xa mỗi cấp

        // Tăng kích thước tháp mỗi cấp thêm 20% để tạo hiệu ứng thị giác tiến hóa
        transform.localScale *= 1.2f;

        Debug.Log($"🎉 {towerType} tiến hóa CẤP {towerLevel}! Dame x{damageMultiplier}, Tốc x{fireRateMultiplier}, Tầm x{rangeMultiplier}");
    }
}