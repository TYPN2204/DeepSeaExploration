using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    
    // THÊM: Biến chỉnh âm lượng
    [Range(0f, 1f)]
    public float menuMusicVolume = 0.1f;
    [Range(0f, 1f)]
    public float gameplayMusicVolume = 0.1f; // Đặt nhỏ để nghe tiếng video

    [Header("SFX")] 
    public AudioSource sfxSource;
    public AudioClip bubbleSound; 
    public AudioClip[] mergeSounds; 

    private void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    public void PlayMenuMusic()
    {
        if (musicSource.clip == menuMusic) return;
        musicSource.clip = menuMusic;
        
        // SỬA LỖI: Set âm lượng
        musicSource.volume = menuMusicVolume;
        
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (musicSource.clip == gameplayMusic) return;
        musicSource.clip = gameplayMusic;
        
        // SỬA LỖI: Set âm lượng
        musicSource.volume = gameplayMusicVolume; 
        
        musicSource.Play();
    }

    public void PlayBubbleSound()
    {
        if (bubbleSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(bubbleSound);
        }
    }

    public void PlayMergeSound(int level)
    {
        if (level >= 0 && level < mergeSounds.Length && mergeSounds[level] != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(mergeSounds[level]);
        }
    }
}