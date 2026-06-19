using UnityEngine;
using System.Collections;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [System.Serializable]
    public class EnemyGroup
    {
        public string groupLabel;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnRate;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public EnemyGroup[] enemySubGroups;
    }

    [Header("Cài đặt Đợt Quái (Waves)")]
    public Wave[] waves;
    public float preparationTime = 30f;

    [Header("Cài đặt Bỏ Qua (Skip)")]
    [Tooltip("Số lượng quái còn sống ít nhất để nút Skip hiện ra (VD: 5)")]
    public int skipThreshold = 5; // <<== ĐIỂM NÂNG CẤP LÀ ĐÂY: BẠN CÓ THỂ ĐỔI SỐ NÀY Ở NGOÀI INSPECTOR
    public int skipBonusMoney = 300; // Tiền thưởng khi Skip

    [Header("Hệ thống Cổng Không gian")]
    public Transform[] spawnPoints;
    public Transform endPoint;

    [Header("Giao diện UI Trên Cùng")]
    public TextMeshProUGUI waveTopText;
    public TextMeshProUGUI countdownText;

    [Header("Giao diện UI Giữa Màn Hình")]
    public GameObject largeWaveNoticeParent;
    public TextMeshProUGUI largeWaveText;

    [Header("Nút Bỏ Qua (Skip Button)")]
    public GameObject nextWaveButton;

    private int currentWaveIndex = 0;
    private bool isSkipPressed = false;
    private bool isSpawning = false;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(false);
        if (nextWaveButton != null) nextWaveButton.SetActive(false);

        StartCoroutine(GameMasterRoutine());
    }

    IEnumerator GameMasterRoutine()
    {
        while (currentWaveIndex < waves.Length)
        {
            if (isGameOver) yield break;

            // === GIAI ĐOẠN 1: CHUẨN BỊ ===
            isSpawning = false;
            isSkipPressed = false;

            if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(true);
            if (largeWaveText != null) largeWaveText.text = "WAVE " + (currentWaveIndex + 1);
            if (waveTopText != null) waveTopText.text = $"WAVE: {currentWaveIndex + 1} / {waves.Length}";
            if (nextWaveButton != null) nextWaveButton.SetActive(true);

            float bigTextTimer = 2f;
            float timer = preparationTime;

            while (timer > 0 && !isSkipPressed)
            {
                if (isGameOver) yield break;

                if (Time.timeScale > 0f)
                {
                    timer -= Time.deltaTime;
                    if (countdownText != null) countdownText.text = "Chuẩn bị: " + Mathf.Ceil(timer).ToString() + "s";

                    if (bigTextTimer > 0)
                    {
                        bigTextTimer -= Time.deltaTime;
                        if (bigTextTimer <= 0 && largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(false);
                    }
                }
                yield return null;
            }

            // === GIAI ĐOẠN 2: QUÁI TRÀN RA ===
            isSpawning = true;
            isSkipPressed = false;
            if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(false);
            if (nextWaveButton != null) nextWaveButton.SetActive(false);
            if (countdownText != null) countdownText.text = "QUÁI ĐANG TRÀN RA!";

            Wave currentWave = waves[currentWaveIndex];
            foreach (EnemyGroup group in currentWave.enemySubGroups)
            {
                if (group.enemyPrefab == null) continue;
                for (int i = 0; i < group.enemyCount; i++)
                {
                    if (isGameOver) yield break;
                    while (Time.timeScale == 0f) yield return null;

                    SpawnEnemy(group.enemyPrefab);
                    yield return new WaitForSeconds(1f / group.spawnRate);
                }
            }

            // === GIAI ĐOẠN 3: CHIẾN ĐẤU ===
            isSpawning = false;
            isSkipPressed = false;

            bool isLastWave = (currentWaveIndex == waves.Length - 1);

            while (true)
            {
                if (isGameOver) yield break;

                int aliveCount = CountAliveEnemies();

                if (aliveCount == 0) break;

                // ÁP DỤNG BIẾN SKIP THRESHOLD Ở ĐÂY
                if (!isLastWave && aliveCount <= skipThreshold)
                {
                    if (nextWaveButton != null && !nextWaveButton.activeSelf) nextWaveButton.SetActive(true);
                }
                else
                {
                    if (nextWaveButton != null && nextWaveButton.activeSelf) nextWaveButton.SetActive(false);
                }

                if (isSkipPressed) break;

                if (countdownText != null) countdownText.text = $"Còn lại: {aliveCount} quái";
                yield return null;
            }

            if (isGameOver) yield break;

            // === GIAI ĐOẠN 4: THẮNG WAVE ===
            if (nextWaveButton != null) nextWaveButton.SetActive(false);

            if (!isSkipPressed)
            {
                if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(true);
                if (largeWaveText != null) largeWaveText.text = "WAVE CLEARED!";
                if (countdownText != null) countdownText.text = "";
                yield return new WaitForSeconds(3f);
            }

            currentWaveIndex++;
        }

        // === KẾT THÚC GAME ===
        if (!isGameOver)
        {
            if (largeWaveText != null) largeWaveText.text = "VICTORY!";
            if (countdownText != null) countdownText.text = "BẠN ĐÃ PHÁ ĐẢO!";
            if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(true);
        }
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Length == 0 || endPoint == null) return;
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];
        GameObject enemy = Instantiate(enemyPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

        EnemyMovement moveScript = enemy.GetComponent<EnemyMovement>();
        if (moveScript != null) moveScript.SetTarget(endPoint);
    }

    int CountAliveEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length;
    }

    public void ClickSkipToNextWave()
    {
        if (!isSpawning && nextWaveButton.activeSelf)
        {
            isSkipPressed = true;
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddMoney(skipBonusMoney); // Cộng tiền bằng biến mới
            }
        }
    }

    public void GameLost()
    {
        isGameOver = true;
        if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(true);
        if (largeWaveText != null) largeWaveText.text = "DEFEAT!";
        if (countdownText != null) countdownText.text = "CỨ ĐIỂM ĐÃ BỊ PHÁ!";
        if (nextWaveButton != null) nextWaveButton.SetActive(false);
    }
}