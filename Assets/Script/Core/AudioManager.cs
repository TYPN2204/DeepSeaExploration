using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    public void PlayMenuMusic()
    {
        musicSource.clip = menuMusic;
        musicSource.volume = 0.2f;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        musicSource.clip = gameplayMusic;
        musicSource.volume = 0.02f;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}