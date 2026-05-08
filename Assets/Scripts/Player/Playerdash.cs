using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Xử lý toàn bộ cơ chế Dash của người chơi:
/// - Nhấn SPACE hoặc Shift để lướt nhanh theo hướng đang di chuyển
/// - Có i-frames (miễn nhiễm sát thương) trong suốt thời gian dash
/// - Hồi chiêu 3 giây
/// - Hiển thị cooldown trên UI
/// </summary>
public class PlayerDash : MonoBehaviour
{
    // =====================================================================
    // THAM SỐ CÓ THỂ CHỈNH TRONG INSPECTOR
    // =====================================================================

    [Header("Chỉ số Dash")]
    [Tooltip("Tốc độ lướt (gấp bao nhiêu lần tốc độ bình thường)")]
    public float dashSpeed = 20f;

    [Tooltip("Thời gian lướt kéo dài bao lâu (giây)")]
    public float dashDuration = 0.3f;

    [Tooltip("Thời gian hồi chiêu sau mỗi lần dash (giây)")]
    public float dashCooldown = 3f;

    [Header("Hiệu ứng hình ảnh (Tuỳ chọn)")]
    [Tooltip("Kéo SpriteRenderer của Player vào đây để nhấp nháy khi dash")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Màu nhân vật khi đang trong trạng thái miễn nhiễm")]
    public Color invincibleColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("UI Cooldown Dash (Tuỳ chọn)")]
    [Tooltip("Kéo Image cooldown của Dash vào đây (dùng Fill Amount)")]
    public Image dashCooldownImage;

    [Tooltip("Kéo GameObject chứa icon Dash vào đây để ẩn/hiện")]
    public GameObject dashReadyIcon;

    // =====================================================================
    // BIẾN NỘI BỘ (không hiện trên Inspector)
    // =====================================================================

    private Rigidbody2D rb;
    private PlayerMovement playerMovement; // Tham chiếu để khoá/mở di chuyển
    private Health health;                 // Tham chiếu để bật/tắt i-frames

    private bool isDashing = false;        // Đang trong trạng thái lướt không?
    private float dashTimer = 0f;          // Đồng hồ đếm thời gian lướt
    private float cooldownTimer = 0f;      // Đồng hồ đếm thời gian hồi chiêu
    private bool canDash = true;           // Có thể dash không?

    private Vector2 dashDirection;         // Hướng lướt đã được ghi lại
    private Color originalColor;           // Màu gốc của sprite để khôi phục lại

    // =====================================================================
    // KHỞI TẠO
    // =====================================================================

    void Start()
    {
        rb              = GetComponent<Rigidbody2D>();
        playerMovement  = GetComponent<PlayerMovement>();
        health          = GetComponent<Health>();

        // Ghi lại màu gốc của sprite
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Đảm bảo UI bắt đầu ở trạng thái đầy (sẵn sàng dash)
        UpdateCooldownUI(1f);
    }

    // =====================================================================
    // VÒNG LẶP CHÍNH
    // =====================================================================

    void Update()
    {
        HandleDashTimer();
        HandleCooldownTimer();
        HandleDashInput();
    }

    void FixedUpdate()
    {
        // Khi đang dash: đẩy nhân vật với tốc độ cao theo hướng đã chọn
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
    }

    // =====================================================================
    // XỬ LÝ THỜI GIAN DASH
    // =====================================================================

    /// <summary>Đếm ngược thời gian dash, kết thúc dash khi hết giờ.</summary>
    private void HandleDashTimer()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    // =====================================================================
    // XỬ LÝ THỜI GIAN HỒI CHIÊU
    // =====================================================================

    /// <summary>Đếm ngược thời gian hồi chiêu và cập nhật UI.</summary>
    private void HandleCooldownTimer()
    {
        if (canDash) return;

        cooldownTimer -= Time.deltaTime;

        // Cập nhật thanh UI cooldown (từ 0 → 1 khi hồi đầy)
        float fillAmount = 1f - Mathf.Clamp01(cooldownTimer / dashCooldown);
        UpdateCooldownUI(fillAmount);

        if (cooldownTimer <= 0f)
        {
            canDash = true;
            UpdateCooldownUI(1f);

            // Hiện icon "Dash sẵn sàng"
            if (dashReadyIcon != null)
                dashReadyIcon.SetActive(true);
        }
    }

    // =====================================================================
    // XỬ LÝ INPUT
    // =====================================================================

    /// <summary>Phát hiện phím SPACE hoặc Shift trái để kích hoạt dash.</summary>
    private void HandleDashInput()
    {
        // Nếu đang dash hoặc đang hồi chiêu thì bỏ qua
        if (isDashing || !canDash) return;

        bool dashPressed = Keyboard.current != null &&
                           (Keyboard.current.spaceKey.wasPressedThisFrame ||
                            Keyboard.current.leftShiftKey.wasPressedThisFrame);

        if (dashPressed)
        {
            StartDash();
        }
    }

    // =====================================================================
    // BẮT ĐẦU VÀ KẾT THÚC DASH
    // =====================================================================

    /// <summary>Kích hoạt dash: ghi hướng, bật i-frames, khoá chuyển động thường.</summary>
    private void StartDash()
    {
        // --- Xác định hướng dash ---
        // Ưu tiên hướng di chuyển (WASD), nếu đứng yên thì dash về hướng nhìn
        if (playerMovement != null && playerMovement.GetMoveInput() != Vector2.zero)
        {
            dashDirection = playerMovement.GetMoveInput().normalized;
        }
        else
        {
            // Dash về phía con trỏ chuột nếu đứng yên
            if (Mouse.current != null)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                mouseWorldPos.z = 0f;
                dashDirection = ((Vector2)(mouseWorldPos - transform.position)).normalized;
            }
            else
            {
                dashDirection = Vector2.right; // Mặc định
            }
        }

        // --- Bật trạng thái dash ---
        isDashing  = true;
        dashTimer  = dashDuration;
        canDash    = false;
        cooldownTimer = dashCooldown;

        // --- Bật i-frames trên Health script ---
        if (health != null)
            health.SetInvincible(true);

        // --- Khoá PlayerMovement để tránh xung đột velocity ---
        if (playerMovement != null)
            playerMovement.SetDashing(true);

        // --- Hiệu ứng hình ảnh: làm nhân vật mờ đi ---
        if (spriteRenderer != null)
            spriteRenderer.color = invincibleColor;

        // --- Ẩn icon "Dash sẵn sàng" ---
        if (dashReadyIcon != null)
            dashReadyIcon.SetActive(false);

        Debug.Log($"[Dash] Bắt đầu lướt về hướng: {dashDirection}");
    }

    /// <summary>Kết thúc dash: tắt i-frames, mở lại chuyển động thường.</summary>
    private void EndDash()
    {
        isDashing = false;

        // Dừng hẳn velocity để không trượt tiếp
        rb.linearVelocity = Vector2.zero;

        // --- Tắt i-frames ---
        if (health != null)
            health.SetInvincible(false);

        // --- Mở lại PlayerMovement ---
        if (playerMovement != null)
            playerMovement.SetDashing(false);

        // --- Khôi phục màu sprite gốc ---
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        Debug.Log("[Dash] Kết thúc lướt. Bắt đầu hồi chiêu...");
    }

    // =====================================================================
    // CẬP NHẬT UI
    // =====================================================================

    /// <summary>Cập nhật thanh fill cooldown trên HUD.</summary>
    private void UpdateCooldownUI(float fillAmount)
    {
        if (dashCooldownImage != null)
            dashCooldownImage.fillAmount = fillAmount;
    }

    // =====================================================================
    // GETTER CÔNG KHAI (để script khác đọc trạng thái)
    // =====================================================================

    /// <summary>Trả về true nếu nhân vật đang trong trạng thái dash.</summary>
    public bool IsDashing() => isDashing;

    /// <summary>Trả về true nếu dash đã sẵn sàng sử dụng.</summary>
    public bool CanDash() => canDash;

    /// <summary>Trả về tỷ lệ hồi chiêu còn lại (0 = đang hồi, 1 = sẵn sàng).</summary>
    public float GetCooldownPercent()
    {
        if (canDash) return 1f;
        return 1f - Mathf.Clamp01(cooldownTimer / dashCooldown);
    }
}