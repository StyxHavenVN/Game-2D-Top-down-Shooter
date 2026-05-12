using UnityEngine;

/// <summary>
/// Singleton quản lý toàn bộ âm thanh (SFX & Music) trong game.
/// Gọi từ bất kỳ đâu: AudioManager.Instance.PlayShootPistol();
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Nguồn phát nhạc nền (BGM)")]
    public AudioSource musicSource;
    [Tooltip("Nguồn phát hiệu ứng âm thanh (SFX)")]
    public AudioSource sfxSource;

    [Header("Vũ Khí (Weapons)")]
    public AudioClip pistolShootClip;
    public AudioClip shotgunShootClip;
    public AudioClip swordSlashClip;

    [Header("Tương tác & Hệ thống")]
    public AudioClip enemyHitClip;
    public AudioClip levelUpClip;
    public AudioClip pickupItemClip;
    public AudioClip enemyDeathClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tùy chọn: Giữ cho AudioManager không bị xóa khi chuyển Scene (Menu -> Game)
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Phát nhạc nền (BGM)
    /// </summary>
    public void PlayMusic(AudioClip bgmClip)
    {
        if (musicSource != null && bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // ─── CÁC HÀM PHÁT SFX ──────────────────────────────────────────

    public void PlayPistolShoot()
    {
        if (sfxSource != null && pistolShootClip != null)
            sfxSource.PlayOneShot(pistolShootClip);
    }

    public void PlayShotgunShoot()
    {
        if (sfxSource != null && shotgunShootClip != null)
            sfxSource.PlayOneShot(shotgunShootClip);
    }

    public void PlaySwordSlash()
    {
        if (sfxSource != null && swordSlashClip != null)
            sfxSource.PlayOneShot(swordSlashClip);
    }

    public void PlayEnemyHit()
    {
        if (sfxSource != null && enemyHitClip != null)
            sfxSource.PlayOneShot(enemyHitClip, 0.7f); // Giảm âm lượng một chút để không bị đinh tai khi bắn chùm
    }

    public void PlayEnemyDeath()
    {
        if (sfxSource != null && enemyDeathClip != null)
            sfxSource.PlayOneShot(enemyDeathClip);
    }

    public void PlayPickupItem()
    {
        if (sfxSource != null && pickupItemClip != null)
            sfxSource.PlayOneShot(pickupItemClip);
    }

    public void PlayLevelUp()
    {
        if (sfxSource != null && levelUpClip != null)
            sfxSource.PlayOneShot(levelUpClip);
    }

    public void PlayEnergySound()
    {
        if (energyClip != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(energyClip);
        }
    }
}