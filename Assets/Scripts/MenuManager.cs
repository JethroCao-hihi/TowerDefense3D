using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [Tooltip("Kéo cục OptionMenu vào đây")]
    public GameObject optionMenuPanel;

    [Tooltip("Kéo cục PauseMenu vào đây")]
    public GameObject pauseMenuPanel;

    private bool isPaused = false;

    void Start()
    {
        // Khi vừa vào Menu, đảm bảo các bảng phụ phải được ẩn đi
        if (optionMenuPanel != null) optionMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Đảm bảo thời gian game trôi bình thường
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Tính năng mở rộng: Bấm phím ESC để bật/tắt Pause Menu khi đang chơi
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ==========================================
    // CÁC HÀM CHO MAIN MENU CHÍNH
    // ==========================================

    public void PlayGame()
    {
        // LƯU Ý: Đổi chữ "GameScene" thành ĐÚNG TÊN scene chơi game của bạn
        SceneManager.LoadScene("Map1");
    }

    public void OpenSettings()
    {
        optionMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Đã thoát game! (Sẽ chỉ hoạt động khi build ra file .exe hoặc .apk)");
        Application.Quit();
    }

    // ==========================================
    // CÁC HÀM CHO OPTION MENU (CÀI ĐẶT)
    // ==========================================

    public void CloseSettings() // Hàm này gắn vào nút QuitMenu
    {
        optionMenuPanel.SetActive(false);
    }

    // Bạn có thể tự code thêm hàm chỉnh Âm thanh ở đây cho thanh Slider Music/SFX

    // ==========================================
    // CÁC HÀM CHO PAUSE MENU (KHI ĐANG CHƠI)
    // ==========================================

    public void PauseGame()
    {
        if (pauseMenuPanel == null) return;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Đóng băng mọi hoạt động trong game
        isPaused = true;
    }

    public void ResumeGame() // Hàm này gắn vào nút Play trong PauseMenu
    {
        if (pauseMenuPanel == null) return;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Rã đông game
        isPaused = false;
    }

    public void RestartGame() // Hàm này gắn vào nút Again trong PauseMenu
    {
        Time.timeScale = 1f;
        // Tự động load lại scene hiện tại đang chơi
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu() // Hàm này gắn vào nút Quit trong PauseMenu
    {
        Time.timeScale = 1f;
        // LƯU Ý: Đổi chữ "Menu" thành ĐÚNG TÊN scene Menu của bạn
        SceneManager.LoadScene("Menu");
    }
}