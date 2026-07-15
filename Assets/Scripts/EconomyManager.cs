using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance; // Tổng đài để các script khác gọi đến

    [Header("Ví Tiền")]
    public int startingMoney = 300;
    public int currentMoney;

    [Header("Giao diện UI")]
    public TextMeshProUGUI moneyText; // Kéo chữ hiển thị số tiền vào đây

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // ==========================================
        // 1. ĐỒNG BỘ TIỀN TỪ KÉT SẮT (DataManager)
        // ==========================================
        if (DataManager.Instance != null)
        {
            currentMoney = DataManager.Instance.totalMoney;
        }
        else
        {
            // Chạy dự phòng bằng số tiền mặc định nếu bạn đang test trực tiếp scene Map1
            currentMoney = startingMoney; 
        }
        
        UpdateUI();
    }

    // Hàm cộng tiền khi giết quái
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
        
        // ==========================================
        // 2. LƯU TIỀN VÀO BỘ NHỚ KHI NHẬN ĐƯỢC
        // ==========================================
        if (DataManager.Instance != null) 
        {
            DataManager.Instance.SaveMoney(currentMoney);
        }
    }

    // Hàm trừ tiền khi mua tháp (Trả về true nếu đủ tiền)
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            
            // ==========================================
            // 3. LƯU TIỀN VÀO BỘ NHỚ KHI TIÊU ĐI
            // ==========================================
            if (DataManager.Instance != null) 
            {
                DataManager.Instance.SaveMoney(currentMoney);
            }
            
            return true;
        }
        return false; // Không đủ tiền
    }

    void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Vàng: " + currentMoney;
        }
    }
}