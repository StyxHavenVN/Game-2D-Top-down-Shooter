using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Music")]
    public AudioClip defaultMusicClip;
    public AudioClip bossMusicClip;

    [Header("Gun Sounds")]
    public AudioClip pistolShootClip;
    public AudioClip shotgunShootClip;

    [Header("Reload Sounds")]
    public AudioClip pistolReloadClip;
    public AudioClip shotgunReloadClip;

    [Header("Other Effects")]
    public AudioClip energyClip;
    public AudioClip swordClip;

    void Awake()
    {
        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
    }

    void Start()
    {
        PlayDefaultMusic();
    }

    public void PlayDefaultMusic()
    {
        if (defaultMusicClip == null) return;

        audioSource.loop = true;
        audioSource.clip = defaultMusicClip;
        audioSource.Play();
    }

    public void PlayBossMusic()
    {
        if (bossMusicClip == null) return;

        audioSource.loop = true;
        audioSource.clip = bossMusicClip;
        audioSource.Play();
    }

    public void PlayPistolShootSound()
    {
        PlayEffect(pistolShootClip);
    }

    public void PlayShotgunShootSound()
    {
        if (shotgunShootClip != null)
        {
            PlayEffect(shotgunShootClip);
        }
        else
        {
            PlayEffect(pistolShootClip);
        }
    }

    public void PlayPistolReloadSound()
    {
        PlayEffect(pistolReloadClip);
    }

    public void PlayShotgunReloadSound()
    {
        if (shotgunReloadClip != null)
        {
            PlayEffect(shotgunReloadClip);
        }
        else
        {
            PlayEffect(pistolReloadClip);
        }
    }

    public void PlayEnergySound()
    {
        PlayEffect(energyClip);
    }

    public void PlaySwordSound()
    {
        PlayEffect(swordClip);
    }

    void PlayEffect(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }
}