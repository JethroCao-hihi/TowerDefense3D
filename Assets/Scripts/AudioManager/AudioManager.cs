using UnityEngine;

// Tạo một struct nhỏ để hiển thị đẹp mắt trong bảng Inspector
[System.Serializable]
public class SoundEffect
{
    public string name; // Sẽ điền tên tháp vào đây (VD: "Cannon", "Laser")
    public AudioClip clip; // File âm thanh tương ứng
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources (Máy phát nhạc)")]
    public AudioSource musicSource; // Máy phát nhạc nền
    public AudioSource sfxSource;   // Máy phát hiệu ứng (tiếng súng, tiếng UI)

    [Header("Nhạc Nền")]
    public AudioClip backgroundMusic;

    [Header("Tiếng Bắn Của Tháp")]
    [Tooltip("Tên (name) ở đây PHẢI TRÙNG KHỚP với biến towerType trong TowerStats")]
    public SoundEffect[] shootingSounds;

    [Header("Tiếng Hệ Thống (UI)")]
    public AudioClip buildSound;
    public AudioClip errorSound; // Tiếng khi không đủ tiền
    public AudioClip winSound;
    public AudioClip loseSound;

    private void Awake()
    {
        // Đảm bảo chỉ có duy nhất 1 AudioManager tồn tại khi chuyển Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tự động bật nhạc nền khi vào game
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true; // Lặp lại liên tục
            musicSource.Play();
        }
    }

    // --- HÀM XỬ LÝ TIẾNG SÚNG THEO TÊN THÁP ---
    public void PlayShoot(string towerType)
    {
        if (sfxSource == null) return;

        // Quét qua danh sách âm thanh xem có trùng tên tháp không
        foreach (SoundEffect sfx in shootingSounds)
        {
            if (sfx.name == towerType && sfx.clip != null)
            {
                // Dùng PlayOneShot để nhiều tháp bắn cùng lúc tiếng không bị đứt quãng
                sfxSource.PlayOneShot(sfx.clip);
                return;
            }
        }
        
        Debug.LogWarning($"⚠️ Không tìm thấy file âm thanh cho tháp loại: {towerType}");
    }

    // --- CÁC HÀM XỬ LÝ ÂM THANH CHUNG ---
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Gọi nhanh các tiếng UI từ các script khác (VD: AudioManager.Instance.PlayBuildSound(); )
    public void PlayBuildSound() { PlaySFX(buildSound); }
    public void PlayErrorSound() { PlaySFX(errorSound); }
    public void PlayWinSound() { PlaySFX(winSound); }
    public void PlayLoseSound() { PlaySFX(loseSound); }
}