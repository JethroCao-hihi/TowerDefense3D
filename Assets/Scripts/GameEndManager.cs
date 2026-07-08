using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance;
    
    [Header("UI Panels")]
    public GameObject gameEndPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    void Awake() { Instance = this; }

    void Start() { gameEndPanel.SetActive(false); }

    public void ShowWinScreen()
    {
        // 1. Bật khung bọc bên ngoài lên (nếu có)
        if (gameEndPanel != null) gameEndPanel.SetActive(true);

        // 2. Gọi MenuManager để nó vừa hiện bảng Win, vừa nạp Điểm và Tiền thực tế lên UI kẹo dẻo
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.TriggerGameWin();
        }
        else
        {
            // Phương án dự phòng nếu không tìm thấy MenuManager
            Time.timeScale = 0f;
            if (winPanel != null) winPanel.SetActive(true);
            if (losePanel != null) losePanel.SetActive(false);
        }
    }

    public void ShowLoseScreen()
    {
        // 1. Bật khung bọc bên ngoài lên (nếu có)
        if (gameEndPanel != null) gameEndPanel.SetActive(true);

        // 2. Gọi MenuManager để nó vừa hiện bảng Lose, vừa nạp Điểm thực tế lên UI ô "000"
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.TriggerGameOver();
        }
        else
        {
            // Phương án dự phòng nếu không tìm thấy MenuManager
            Time.timeScale = 0f;
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu"); 
    }
}