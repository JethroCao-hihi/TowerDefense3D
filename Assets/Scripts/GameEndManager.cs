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
        Time.timeScale = 0f; // Dừng game
        gameEndPanel.SetActive(true);
        winPanel.SetActive(true);
        losePanel.SetActive(false);
    }

    public void ShowLoseScreen()
    {
        Time.timeScale = 0f; // Dừng game
        gameEndPanel.SetActive(true);
        winPanel.SetActive(false);
        losePanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu"); // Nhớ để tên scene Menu chính xác
    }
}