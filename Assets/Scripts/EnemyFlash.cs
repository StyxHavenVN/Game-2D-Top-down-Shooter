using UnityEngine;
using System.Collections;

/// <summary>
/// Gắn vào mọi quái vật (Enemy Prefab).
/// Khi gọi Flash(), quái sẽ nháy trắng 0.1 giây rồi trở lại bình thường.
/// Hiệu ứng này cho người chơi biết "Ê, mày bắn trúng rồi đó!"
/// </summary>
public class EnemyFlash : MonoBehaviour
{
    [Header("Cài đặt Flash")]
    [Tooltip("Màu nháy khi trúng đạn (trắng = tiêu chuẩn ngành)")]
    public Color flashColor = Color.white;

    [Tooltip("Thời gian nháy (giây)")]
    public float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Material flashMaterial;
    private Coroutine flashCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // Lưu material gốc
            originalMaterial = spriteRenderer.material;

            // Tạo material flash (dùng shader mặc định của Unity)
            flashMaterial = new Material(Shader.Find("GUI/Text Shader"));
            // Nếu shader trên không tồn tại, dùng fallback
            if (flashMaterial.shader == null)
            {
                flashMaterial = new Material(originalMaterial);
            }
        }
    }

    /// <summary>
    /// Gọi hàm này mỗi khi quái bị trúng đạn.
    /// </summary>
    public void Flash()
    {
        if (spriteRenderer == null) return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Đổi màu sang trắng sáng chói
        spriteRenderer.color = flashColor;

        // Chờ một khoảnh khắc ngắn
        yield return new WaitForSeconds(flashDuration);

        // Khôi phục màu gốc
        spriteRenderer.color = Color.white; // Màu mặc định của sprite
        flashCoroutine = null;
    }

    /// <summary>
    /// Reset màu khi quái được lấy ra từ Object Pool (tránh bị kẹt màu trắng).
    /// </summary>
    void OnEnable()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }
}
