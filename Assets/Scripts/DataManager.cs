using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [Header("--- Dữ Liệu Người Chơi ---")]
    public int totalMoney = 0;

    private void Awake()
    {
        // Đảm bảo chỉ có duy nhất 1 DataManager tồn tại xuyên suốt game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Không bị hủy khi chuyển Scene
            LoadGameData(); // Tự động load dữ liệu ngay khi vừa mở app
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // CÁC HÀM LƯU & TẢI DỮ LIỆU
    // ==========================================
    
    public void LoadGameData()
    {
        // Lấy dữ liệu từ bộ nhớ. Nếu người chơi mới tải game (chưa có data), mặc định cho họ 500 vàng làm vốn khởi nghiệp.
        totalMoney = PlayerPrefs.GetInt("PlayerMoney", 500); 
        Debug.Log("Tải thành công dữ liệu. Tiền hiện có: " + totalMoney);
    }

    public void SaveMoney(int amount)
    {
        totalMoney = amount;
        PlayerPrefs.SetInt("PlayerMoney", totalMoney);
        PlayerPrefs.Save(); // Ép Unity ghi ngay lập tức vào ổ cứng điện thoại
    }

    // ==========================================
    // CHUẨN BỊ CHO HỆ THỐNG GACHA (Sắp tới)
    // ==========================================
    
    public void SaveUnlockedCard(string cardName)
    {
        // Sau này khi roll thẻ, chúng ta sẽ dùng hàm này để lưu thẻ mới vào danh sách
        PlayerPrefs.SetInt("Card_" + cardName, 1); // 1 nghĩa là đã sở hữu
        PlayerPrefs.Save();
    }

    public bool HasCard(string cardName)
    {
        return PlayerPrefs.GetInt("Card_" + cardName, 0) == 1;
    }
}