using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Player Stats")]
    public int startLives = 20;
    private int currentLives;

    private bool isGameOver = false;

    [Header("UI")]
    public TextMeshProUGUI livesText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentLives = startLives;
        UpdateLivesUI();
    }

  
    public void LoseLife(int amount)
    {
        if (isGameOver) return;

        currentLives -= amount;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            currentLives = 0;
            isGameOver = true;
            GameOver();
        }
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "LIVES: " + currentLives;
        }
    }

    void GameOver()
    {
        isGameOver = true;

        if (GameEndManager.Instance != null)
        {
            GameEndManager.Instance.ShowLoseScreen();
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.GameLost();
        }

        // Khóa đứng thời gian toàn game (Tháp ngừng bắn, quái ngừng đi)
        Time.timeScale = 0f;
    }
}