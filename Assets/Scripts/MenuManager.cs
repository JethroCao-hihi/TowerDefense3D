using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // BẮT BUỘC để điều khiển TextMeshPro

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("--- Menu Panels ---")] 
    public GameObject optionMenuPanel;
    public GameObject pauseMenuPanel;

    [Header("--- New Demo Panels (Nộp Đồ Án) ---")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("--- Victory Text Setup ---")] 
    public TMP_Text scoreNumberText;
    public TMP_Text coinNumberText;

    [Header("--- Game Over Text Setup ---")]
    public TMP_Text loseScoreNumberText;

    [Header("--- Game Stats ---")] 
    public int currentScore = 0;
    public int currentCoins = 0;

    [Header("--- Settings Game (Hệ thống Button ON/OFF) ---")] 
    [Tooltip("Kéo Object nút SOUND_ON vào đây")]
    public GameObject soundOnButton;

    [Tooltip("Kéo Object nút SOUND_OFF vào đây")]
    public GameObject soundOffButton;

    [Tooltip("Kéo Object nút MUSIC_ON vào đây")]
    public GameObject musicOnButton;

    [Tooltip("Kéo Object nút MUSIC_OFF vào đây")]
    public GameObject musicOffButton;

    private bool isSoundOn = true;
    private bool isMusicOn = true;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (optionMenuPanel != null) optionMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Khởi tạo trạng thái hiển thị ban đầu cho các nút ON/OFF
        UpdateSettingsUI();

        Time.timeScale = 1f;

        // =========================================================================
        // TỰ ĐỘNG ĐỔI BÀI NHẠC THÔNG MINH CHO TỪNG SCENE
        // =========================================================================
        if (SoundManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            // Nếu đang ở màn hình chính ngoài sảnh
            if (currentSceneName == "Menu")
            {
                SoundManager.Instance.PlayMusic("Menu"); 
            }
            // Nếu ở bất kỳ màn nào khác (màn chơi, map test, đồ án...), tự động bật nhạc chiến đấu
            else
            {
                SoundManager.Instance.PlayMusic("InGame"); 
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void AddScoreAndCoins(int scoreToAdd, int coinsToAdd)
    {
        currentScore += scoreToAdd;
        currentCoins += coinsToAdd;
    }

    // ==========================================
    // LOGIC ĐIỀU KHIỂN NÚT BUTTON ON/OFF ĐỘC LẬP
    // ==========================================

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn; 

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleSFX();
        }

        UpdateSettingsUI(); 
        Debug.Log("Sound SFX: " + (isSoundOn ? "BẬT" : "TẮT"));
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn; 

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleMusic();
        }

        UpdateSettingsUI(); 
        Debug.Log("Nhạc nền Music: " + (isMusicOn ? "BẬT" : "TẮT"));
    }

    private void UpdateSettingsUI()
    {
        if (soundOnButton != null) soundOnButton.SetActive(isSoundOn);
        if (soundOffButton != null) soundOffButton.SetActive(!isSoundOn);

        if (musicOnButton != null) musicOnButton.SetActive(isMusicOn);
        if (musicOffButton != null) musicOffButton.SetActive(!isMusicOn);
    }

    // ==========================================
    // CÁC HÀM ĐIỀU HƯỚNG VÀ KẾT THÚC GAME
    // ==========================================
    public void TriggerGameOver()
    {
        if (losePanel == null) return;
        losePanel.SetActive(true);
        Time.timeScale = 0f;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayUI("Lose");

        if (loseScoreNumberText != null) loseScoreNumberText.text = currentScore.ToString("N0");
    }

    public void TriggerGameWin()
    {
        if (winPanel == null) return;
        winPanel.SetActive(true);
        Time.timeScale = 0f;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayUI("Win");

        if (scoreNumberText != null) scoreNumberText.text = currentScore.ToString("N0");
        if (coinNumberText != null) coinNumberText.text = currentCoins.ToString("N0");
    }

    public void PauseGame()
    {
        if (pauseMenuPanel == null) return;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel == null) return;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenSettings()
    {
        if (optionMenuPanel == null) return;
        optionMenuPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (optionMenuPanel == null) return;
        optionMenuPanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Map1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}