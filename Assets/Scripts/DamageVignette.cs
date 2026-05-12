using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Singleton quản lý hiệu ứng viền đỏ khi Player bị đánh.
/// Gọi từ bất kỳ đâu: DamageVignette.Instance.Flash();
/// 
/// Cách dùng trong Unity:
/// 1. Tạo một Image con bên trong Canvas (đặt tên: DamageOverlay)
/// 2. Stretch Full màn hình (Anchor Presets: Alt + Stretch-Stretch)
/// 3. Gán ảnh Vignette hoặc để trống (dùng màu đỏ đơn giản)
/// 4. Đặt Raycast Target = false (để không chặn click chuột)
/// 5. Kéo Image đó vào ô "Vignette Image" của script này
/// </summary>
public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance { get; private set; }

    [Header("Tham chiếu UI")]
    [Tooltip("Kéo Image phủ đỏ toàn màn hình vào đây")]
    public Image vignetteImage;

    [Header("Cài đặt hiệu ứng")]
    [Tooltip("Màu chớp khi bị đánh")]
    public Color damageColor = new Color(1f, 0f, 0f, 0.35f); // Đỏ, alpha 35%

    [Tooltip("Thời gian fade out (giây)")]
    public float fadeDuration = 0.3f;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Bắt đầu với alpha = 0 (ẩn hoàn toàn)
        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }
    }

    /// <summary>
    /// Gọi hàm này khi Player nhận sát thương.
    /// Màn hình sẽ chớp đỏ rồi mờ dần đi.
    /// </summary>
    public void Flash()
    {
        if (vignetteImage == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // Bật chớp đỏ ngay lập tức
        vignetteImage.color = damageColor;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            // Giảm dần alpha từ damageColor.a xuống 0
            float alpha = Mathf.Lerp(damageColor.a, 0f, elapsed / fadeDuration);
            Color c = vignetteImage.color;
            c.a = alpha;
            vignetteImage.color = c;

            yield return null;
        }

        // Đảm bảo alpha = 0 khi kết thúc
        Color final_c = vignetteImage.color;
        final_c.a = 0f;
        vignetteImage.color = final_c;

        fadeCoroutine = null;
    }
}
