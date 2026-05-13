using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

/// <summary>
/// Quản lý Pause Menu (ESC).
/// - ESC: Bật/tắt Pause
/// - Resume: Tiếp tục chơi
/// - SetVolume: Chỉnh âm lượng qua Slider
/// - QuitToMenu: Về màn hình chọn vũ khí (không cần scene riêng)
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────
    public static PauseMenuManager Instance { get; private set; }

    // ─── State ────────────────────────────────────────────────
    public static bool isPaused = false;

    // ─── UI References ────────────────────────────────────────
    [Header("UI Panels")]
    public GameObject pauseMenuUI;       // Panel gốc của Pause Menu

    [Header("Audio")]
    public AudioMixer audioMixer;        // Kéo MasterVol.mixer vào đây
    public Slider volumeSlider;          // Slider chỉnh âm lượng
    [Tooltip("Tên Exposed Parameter trong AudioMixer (phải khớp chính xác)")]
    public string mixerVolumeParam = "MasterVol";

    // ─── Giá trị âm lượng mặc định (lưu giữa lần chơi) ──────
    private const string VolumePrefKey = "MasterVolume";

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Ẩn Pause Menu lúc bắt đầu
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        isPaused = false;

        // Khôi phục âm lượng đã lưu từ lần chơi trước
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 0.75f);
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }
        ApplyVolume(savedVolume);
    }

    // ─────────────────────────────────────────────────────────
    void Update()
    {
        // Cho phép bấm ESC bất kỳ lúc nào (trừ khi đang Game Over / Level Up)
        // Dùng New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>Tiếp tục chơi.</summary>
    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    /// <summary>Dừng game và hiện Pause Menu.</summary>
    private void Pause()
    {
        // Không cho pause khi đang ở màn hình chọn vũ khí
        if (MainMenuManager.selectedWeapon == MainMenuManager.WeaponType.None)
            return;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>
    /// Gọi bởi OnValueChanged của Slider âm lượng.
    /// Slider range: 0.001 → 1 (KHÔNG dùng 0 vì Log10(0) = -Infinity)
    /// </summary>
    public void SetVolume(float volume)
    {
        ApplyVolume(volume);
        // Lưu âm lượng vào PlayerPrefs
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float volume)
    {
        if (audioMixer == null) return;
        // Chuyển đổi tuyến tính (0.001→1) sang Logarithmic (-60dB→0dB)
        float dB = Mathf.Log10(Mathf.Max(volume, 0.001f)) * 20f;
        audioMixer.SetFloat(mixerVolumeParam, dB);
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>
    /// Về màn hình chọn vũ khí (trong cùng scene) thay vì load scene mới.
    /// Tránh crash do scene "MainMenu" không tồn tại riêng.
    /// </summary>
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // Reload scene hiện tại — MainMenuManager.Start() sẽ tự reset về màn chọn vũ khí
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}