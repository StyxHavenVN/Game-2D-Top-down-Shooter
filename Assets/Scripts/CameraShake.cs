using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton quản lý hiệu ứng rung camera.
/// Gọi từ bất kỳ đâu: CameraShake.Instance.Shake();
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Cài đặt mặc định")]
    [Tooltip("Cường độ rung mặc định (pixel)")]
    public float defaultIntensity = 0.05f;

    [Tooltip("Thời gian rung mặc định (giây)")]
    public float defaultDuration = 0.05f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Rung camera với cường độ và thời gian MẶC ĐỊNH.
    /// Dùng cho: bắn súng lục, quái trúng đạn.
    /// </summary>
    public void Shake()
    {
        Shake(defaultIntensity, defaultDuration);
    }

    /// <summary>
    /// Rung camera với cường độ TÙY CHỈNH.
    /// Dùng cho: Shotgun (rung mạnh hơn), Boss chết (rung cực mạnh).
    /// </summary>
    public void Shake(float intensity, float duration)
    {
        // Nếu đang rung rồi thì dừng cái cũ, chạy cái mới
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Random offset theo 2 trục X, Y
            float offsetX = Random.Range(-1f, 1f) * intensity;
            float offsetY = Random.Range(-1f, 1f) * intensity;

            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;

            // Giảm dần cường độ rung (fade out) để cảm giác tự nhiên hơn
            intensity = Mathf.Lerp(intensity, 0f, elapsed / duration);

            yield return null;
        }

        // Khôi phục vị trí gốc
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
