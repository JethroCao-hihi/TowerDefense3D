using UnityEngine;
using UnityEngine.Audio; // BẮT BUỘC để gọi được AudioMixer

public class SoundManager : MonoBehaviour // Đã đổi lại tên chuẩn để khớp với MenuManager
{
    public static SoundManager Instance;

    [Header("--- Kết nối Audio Mixer ---")]
    public AudioMixer mainMixer;

    [Header("--- Máy phát nhạc (Loa) ---")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private bool isMusicMuted = false;
    private bool isSFXMuted = false;

    private void Awake()
    {
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

    // ==========================================
    // CÁC HÀM PHÁT NHẠC (Đã đồng bộ tên hàm cho Tower và Menu)
    // ==========================================
    public void PlayMusic(string trackId)
    {
        if (AudioLibrary.Instance == null || musicSource == null) return;

        AudioClip clip = AudioLibrary.Instance.GetMusic(trackId);
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Hàm gọi tiếng súng khớp với TowerShooting.cs
    public void PlayTowerShoot(string sfxId)
    {
        PlaySFX(sfxId);
    }

    // Hàm gọi tiếng hệ thống khớp với MenuManager.cs
    public void PlayUI(string sfxId)
    {
        PlaySFX(sfxId);
    }

    // Hàm phát tiếng động lõi
    public void PlaySFX(string sfxId)
    {
        if (AudioLibrary.Instance == null || sfxSource == null || isSFXMuted) return;

        AudioClip clip = AudioLibrary.Instance.GetSFX(sfxId);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // ==========================================
    // ĐIỀU KHIỂN BẬT/TẮT ĐỘC LẬP TỪ MENU MANAGER
    // ==========================================
    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        if (musicSource != null) musicSource.mute = isMusicMuted;
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        if (sfxSource != null) sfxSource.mute = isSFXMuted;
    }

    // ==========================================
    // ĐIỀU KHIỂN ÂM LƯỢNG TỪ AUDIO MIXER BẰNG SLIDER
    // ==========================================
    public void SetMusicVolume(float sliderValue)
    {
        if (mainMixer != null) 
            mainMixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (mainMixer != null) 
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);
    }
}