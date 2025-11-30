using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    
    [Header("SFX Clips")]
    [SerializeField] private AudioClip gunShoot;
    [SerializeField] private AudioClip laserShoot;
    [SerializeField] private AudioClip rocketShoot;
    [SerializeField] private AudioClip rocketExplosion;
    [SerializeField] private AudioClip swappingSFX;

    
    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.7f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create AudioSources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }
        
        // Set volumes
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        // Start gameplay music
        PlayMusic(gameplayMusic);
    }

    // MUSIC CONTROLS
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    // SFX CONTROLS
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    // SPECIFIC SOUND EFFECTS (Called by events)
    public void PlayGunShoot() => PlaySFX(gunShoot);
    public void PlayLaserShoot() => PlaySFX(laserShoot);
    public void PlayRocketShoot() => PlaySFX(rocketShoot);
    public void PlayRocketExplosion() => PlaySFX(rocketExplosion);
    public void PlaySwappingSFX() => PlaySFX(swappingSFX);

    
    public void PlayGameplayMusic()
    {
        if (gameplayMusic != null)
            PlayMusic(gameplayMusic);
    }
}
