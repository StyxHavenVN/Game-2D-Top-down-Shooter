using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Thiết lập Nguồn phát")]
    public AudioSource effectAudioSource;

    [Header("Các File Âm Thanh")]
    public AudioClip shootClip;
    public AudioClip reloadClip;
    public AudioClip energyClip;

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

    public void PlayShootSound()
    {
        if (shootClip != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(shootClip);
        }
    }

    public void PlayReloadSound()
    {
        if (reloadClip != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(reloadClip);
        }
    }

    public void PlayEnergySound()
    {
        if (energyClip != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(energyClip);
        }
    }
}