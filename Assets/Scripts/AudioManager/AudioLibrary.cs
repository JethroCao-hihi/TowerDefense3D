using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string idName; // Tên định danh (VD: "Menu", "Arrow", "Build")
    public AudioClip clip;
}

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary Instance;

    [Header("--- Thư viện Nhạc Nền (Music) ---")]
    public SoundData[] musicTracks;

    [Header("--- Thư viện Tiếng Động (SFX) ---")]
    public SoundData[] sfxTracks;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public AudioClip GetMusic(string id)
    {
        foreach (var track in musicTracks)
        {
            if (track.idName == id) return track.clip;
        }
        return null;
    }

    public AudioClip GetSFX(string id)
    {
        foreach (var track in sfxTracks)
        {
            if (track.idName == id) return track.clip;
        }
        return null;
    }
}