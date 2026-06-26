using UnityEngine;
using UnityEngine.UI;

public class TowerShopButton : MonoBehaviour
{
    [Header("Cài đặt Tháp cho nút này")]
    public GameObject towerPrefab;
    public int towerCost = 100;

    private Button myButton;
    private Image buttonImage;

    void Start()
    {
        myButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    void Update()
    {
        // Liên tục kiểm tra tiền để bật/tắt nút
        if (EconomyManager.Instance != null)
        {
            if (EconomyManager.Instance.currentMoney >= towerCost)
            {
                myButton.interactable = true;
                buttonImage.color = Color.white; 
            }
            else
            {
                // Thiếu tiền -> Khóa nút và làm đen đi
                myButton.interactable = false;
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); 
            }
        }
    }

    // ĐÂY LÀ HÀM BẠN ĐANG TÌM KIẾM ĐÂY!
    public void SelectThisTower()
    {
        if (TowerPlacementManager.Instance != null)
        {
            // Truyền tháp và giá tiền sang cho ông thợ xây
            TowerPlacementManager.Instance.SetSelectedTower(towerPrefab, towerCost);
            Debug.Log("🎯 Đã bấm chọn mua tháp: " + towerPrefab.name);
        }
    }
}