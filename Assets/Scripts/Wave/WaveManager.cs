using UnityEngine;
using System.Collections;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnRate;
    }

    [Header("Cài đặt Đợt Quái (Waves)")]
    public Wave[] waves;
    public float preparationTime = 30f; // 30 giây chuẩn bị theo ý bạn

    [Header("Hệ thống Cổng Không gian")]
    public Transform[] spawnPoints;
    public Transform endPoint;

    [Header("Giao diện UI Trên Cùng")]
    public TextMeshProUGUI waveTopText;      // Chữ nhỏ góc màn hình (WAVE: 1/5)
    public TextMeshProUGUI countdownText;    // Đồng hồ đếm ngược nhỏ

    [Header("Giao diện UI Giữa Màn Hình")]
    public GameObject largeWaveNoticeParent;  // Cục Object chứa chữ WAVE lớn để ẩn/hiện
    public TextMeshProUGUI largeWaveText;    // Chữ WAVE 1 to đùng giữa màn hình

    [Header("Nút Bỏ Qua (Skip Button)")]
    public GameObject nextWaveButton;        // Kéo nút Skip UI vào đây

    private int currentWaveIndex = 0;
    private bool isSkipPressed = false;      // Công tắc nhận biết người chơi bấm nút Skip
    private bool isSpawning = false;         // Đang trong quá trình đẻ quái hay không

    void Start()
    {
        // Ẩn các UI thông báo lớn lúc vừa vào game
        if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(false);
        if (nextWaveButton != null) nextWaveButton.SetActive(false);

        // Bắt đầu chuỗi vòng lặp Wave bất tận bằng Coroutine
        StartCoroutine(GameMasterRoutine());
    }

    // Bộ não trung tâm điều khiển vòng đời của Game
    IEnumerator GameMasterRoutine()
    {
        while (currentWaveIndex < waves.Length)
        {
            // =======================================================
            // GIAI ĐOẠN 1: CHUẨN BỊ TRẬN ĐẤU (30 GIÂY THẢ THẢNH THƠI)
            // =======================================================
            isSpawning = false;
            isSkipPressed = false;

            // 1. Hiện thông báo WAVE lớn ở giữa màn hình
            if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(true);
            if (largeWaveText != null) largeWaveText.text = "WAVE " + (currentWaveIndex + 1);
            if (waveTopText != null) waveTopText.text = $"WAVE: {currentWaveIndex + 1} / {waves.Length}";

            // 2. Bật nút Skip (Người chơi có quyền bỏ qua 30s chờ để đánh luôn)
            if (nextWaveButton != null) nextWaveButton.SetActive(true);

            float timer = preparationTime;
            while (timer > 0 && !isSkipPressed)
            {
                // Kiểm tra nếu game đang bị đóng băng (Game Over) thì dừng đếm
                if (Time.timeScale > 0f)
                {
                    timer -= Time.deltaTime;
                    if (countdownText != null) countdownText.text = "Chuẩn bị: " + Mathf.Ceil(timer).ToString() + "s";
                }
                yield return null; // Chờ khung hình tiếp theo
            }

            // =======================================================
            // GIAI ĐOẠN 2: VÀO TRẬN - QUÁI TRÀN RA (SPAWNING)
            // =======================================================
            isSpawning = true;

            // Ẩn bảng thông báo lớn và TẮT nút Skip khi quái đang đẻ (Cấm bấm loạn)
            if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(false);
            if (nextWaveButton != null) nextWaveButton.SetActive(false);
            if (countdownText != null) countdownText.text = "QUÁI ĐANG TRÀN RA!";

            Wave currentWave = waves[currentWaveIndex];
            for (int i = 0; i < currentWave.enemyCount; i++)
            {
                // Chờ nếu game đang pause
                while (Time.timeScale == 0f) yield return null;

                SpawnEnemy(currentWave.enemyPrefab);
                yield return new WaitForSeconds(1f / currentWave.spawnRate);
            }

            // =======================================================
            // GIAI ĐOẠN 3: CHIẾN ĐẤU SINH TỬ (WAIT FOR CLEAR)
            // =======================================================
            isSpawning = false;
            isSkipPressed = false;

            // QUYỀN HẠN MỚI: Vì quái đã đẻ hết, cho phép hiện lại nút Skip 
            // để người chơi gọi Wave tiếp theo sớm nếu trên map chỉ còn vài con quái yếu
            if (nextWaveButton != null) nextWaveButton.SetActive(true);

            // Vòng lặp đứng đợi: Chỉ thoát ra khi (Hết sạch quái trên map) HOẶC (Người chơi bấm Skip)
            while (CountAliveEnemies() > 0 && !isSkipPressed)
            {
                if (countdownText != null)
                    countdownText.text = $"Còn lại: {CountAliveEnemies()} quái";
                yield return null;
            }

            // =======================================================
            // GIAI ĐOẠN 4: THẮNG WAVE (WAVE CLEARED)
            // =======================================================
            if (nextWaveButton != null) nextWaveButton.SetActive(false);

            if (largeWaveNoticeParent != null) largeWaveNoticeParent.SetActive(true);
            if (largeWaveText != null) largeWaveText.text = "WAVE CLEARED!";
            if (countdownText != null) countdownText.text = "CHIẾN THẮNG!";

            yield return new WaitForSeconds(3f); // Hiện chữ Thắng trong 3 giây ăn mừng

            currentWaveIndex++; // Tăng số Wave lên để chuẩn bị cho vòng lặp tiếp theo
        }

        // KẾT THÚC TOÀN BỘ GAME
        if (largeWaveText != null) largeWaveText.text = "VICTORY!";
        if (countdownText != null) countdownText.text = "BẠN ĐÃ PHÁ ĐẢO!";
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Length == 0 || endPoint == null) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        GameObject enemy = Instantiate(enemyPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

        EnemyMovement moveScript = enemy.GetComponent<EnemyMovement>();
        if (moveScript != null)
        {
            moveScript.SetTarget(endPoint);
        }
    }

    // Hàm đếm số lượng quái vật thực tế đang còn sống dựa vào Tag "Enemy"
    int CountAliveEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length;
    }

    // Hàm này sẽ được gọi khi người chơi bấm vào nút BỎ QUA (Skip Button) ngoài UI
    public void ClickSkipToNextWave()
    {
        // Chỉ cho phép kích hoạt lệnh Skip khi quái KHÔNG trong quá trình đang đẻ
        if (!isSpawning)
        {
            isSkipPressed = true;
            Debug.Log("<color=cyan><b>Người chơi đã chủ động kích hoạt sớm Wave tiếp theo!</b></color>");
        }
    }
}